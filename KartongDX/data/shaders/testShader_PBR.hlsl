
#include "segments/per_object_buffer.hlsl"
#include "segments/per_frame_buffer.hlsl"
#include "segments/utility.hlsl"

static const float PI = 3.141592;
static const float F_DIELECTRIC = 0.04f;



struct VS_Input
{
	float3 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD;
	float4 color : COLOR;
	float3 tangent : TANGENT;
	float3 bitangent : BITANGENT;

};

struct VS_Output
{
	float4 position : SV_POSITION;
	float4 pos : TEXCOORD1;
	float3 N : NORMAL;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	float3 viewDir : NORMAL1;
	float3 T : NORMAL3;
	float3 B : NORMAL4;
	float3x3 TBN : TEXCOORD5;
};


VS_Output vs_main(const VS_Input input)
{
	VS_Output output;
	output.position = mul(float4(input.position.xyz, 1.0f), WorldViewProj);
	output.pos = mul(float4(input.position.xyz, 1.0f), World);
	output.color = input.color;
	output.uv = input.uv;
	output.N = normalize(mul(input.normal.xyz, (float3x3)NormalMatrix));
	output.T = mul(
		normalize(input.tangent.xyz - input.normal.xyz * dot(input.normal.xyz, input.tangent.xyz)), 
		(float3x3)NormalMatrix);
	output.B = normalize(cross(output.N, output.T));

	output.TBN = float3x3(
		output.T,
		output.B,
		output.N
		);
	
	return output;
};

Texture2D<float4> _Diffuse : register(t0);
Texture2D<float4> _Normal : register(t1);
Texture2D<float4> _Roughness : register(t2);
Texture2D<float4> _Metallic : register(t3);
TextureCube Sky : register(SKYBOX_ENV_SLOT);
TextureCube Irradiance : register(SKYBOX_IRR_SLOT); //TODO check why define not working
SamplerState Sampler : register(s0);

float3 cookTorrance(float D, float F, float G, float ndl, float ndv)
{
	return (F * G * D) / (4 * (ndv * ndl));
}

float3 fresnel(float3 F0, float3 cosTheta)
{
	return F0 + (1.0f - F0) * pow(1.0f - cosTheta, 5.0f);
}

float directional(float NdotH, float roughness)
{
	float a = square(roughness);
	float aSqr = square(a);
	float denom = square(NdotH) * (aSqr - 1.0f) + 1.0f;
	return aSqr / (PI * square(denom));
}

float G1(float k, float ndv)
{
	return ndv / (ndv * (1.0f - k) + k);
}

float geometry(float NdotL, float NdotV, float roughness)
{
	float k = square(roughness + 1.0f) / 8.0f;
	float g1L = G1(k, NdotL);
	float g1V = G1(k, NdotV);
	return g1L * g1V;
}

float4 ps_main(const VS_Output input) : SV_TARGET
{
	float3 albedo = _Diffuse.Sample(Sampler, input.uv).rgb;
	float roughness = _Roughness.Sample(Sampler, input.uv).r;
	float metallic = _Metallic.Sample(Sampler, input.uv).r;
	float3 normal = normalize((2.0f * _Normal.Sample(Sampler, input.uv).xyz) - 1.0f);

	float3x3 TBN = input.TBN;

	float3 viewDir = -normalize(ViewPosition.xyz - input.pos.xyz);
	float3 lightDir = normalize(LightDir);

	// Transform View and Light Dir to TangentSpace
	viewDir = mul(TBN, viewDir);
	lightDir = mul(TBN, lightDir);

	//Calcuate Reflection in TangentSpace and transform back to WorldSpace
	float3 lightRef = reflect(viewDir, normal);
	lightRef = mul(transpose(TBN), lightRef);

	//float2 envUV = SpericalEnvMap(lightRef);

	float3 sky = Sky.Sample(Sampler, lightRef).rgba;
	float3 irr = Irradiance.Sample(Sampler, lightRef).rgba;

	//return float4(irr.rgb, 1.0f);

	float3 halfDir = normalize(lightDir + viewDir);

	float NdotL = saturate(dot(normal, lightDir));
	float NdotV = saturate(dot(normal, viewDir));
	float NdotH = saturate(dot(normal, halfDir));

	//return float4(sky, 1.0f);

	// -- DIRECT --
	float k = square(roughness + 1.0f) / 8.0f;
	float3 F0 = lerp(F_DIELECTRIC, albedo, metallic);

	float D = directional(NdotH, roughness);
	float F = fresnel(F0, max(0.0f, dot(halfDir, viewDir)));
	float G = geometry(NdotL, NdotV, roughness);

	float3 SpecularBRDF = cookTorrance(D, F, G, NdotL, NdotV);

	float3 kd = lerp(float3(1, 1, 1) - F, float3(0, 0, 0), metallic);
	float3 DiffuseBRDF = kd * albedo;

	float3 directLight = (DiffuseBRDF + SpecularBRDF) * 1.0f /* Radiance */ * NdotL;
	directLight = clamp(directLight, 0.0f, 10.0f);

	// -- AMBIENT --

	float3 FF = F;//fresnel(F0, max(0.0f, dot(normal, viewDir)));
	float3 kd2 = lerp(1.0f - FF, 0.0f, metallic);
	float3 DiffuseIBL = kd2 * albedo * irr;

	float3 specularIrradiance = sky; //MIP map sample here
	float3 SpecularIBL = F0 * specularIrradiance;

	float3 ambientLight = DiffuseIBL + SpecularIBL;


	// Final
	float3 final = directLight + ambientLight;
	//final = directLight;
	//final = ambientLight;

	//final = ambientLight;
	//final = DiffuseIBL;
	//final = SpecularIBL;
	//final = SpecularBRDF;
	//final.r += 1;
	//final.g += 1;
	//final.b += 1;
	return float4(final, 1.0f);
}


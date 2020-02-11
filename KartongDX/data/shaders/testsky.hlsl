
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
	float4 pos : POSITION;
	float3 N : NORMAL;
	float2 uv : TEXCOORD;
	float4 color : COLOR;
	float3x3 tangentFrame : TEXCOORD2;
	float3 viewDir : NORMAL1;
	float3 viewDir2 : NORMAL2;
	float3 T : NORMAL3;
	float3 B : NORMAL4;
	float3x3 TBN : TEXCOORD5;
};


VS_Output vs_main(const VS_Input input)
{
	VS_Output output;
	output.position = mul(float4(input.position.xyz, 1.0), WorldViewProj);
	output.pos = mul(float4(input.position.xyz, 1.0f), NormalMatrix);
	output.color = input.color;
	output.uv = input.uv;
	output.N = normalize(mul(input.normal.xyz, (float3x3)NormalMatrix));
	output.T = mul(normalize(input.tangent.xyz - input.normal.xyz * dot(input.normal.xyz, input.tangent.xyz)), (float3x3)NormalMatrix);
	output.B = normalize(cross(output.N, output.T));

	output.TBN = float3x3(
		output.T,
		output.B,
		output.N
	);

	return output;
};

TextureCube Diffuse : register(t0);
Texture2D<float4> Normal : register(t1);
Texture2D<float4> _Roughness : register(t2);
Texture2D<float4> _Metallic : register(t3);
Texture2D<float4> Sky : register(t4);
Texture2D<float4> Irradiance : register(t6);
SamplerState Sampler : register(s0);

float2 SampleSphericalMap(float3 v)
{
	float2 invAtan = float2(0.1591, 0.3183);
	float2 uv = float2(atan2(v.z, v.x), asin(v.y));
	uv *= invAtan;
	uv += 0.5;
	return uv;
}

float4 ps_main(VS_Output input) : SV_TARGET
{
	//float2 uv = SampleSphericalMap(normalize(input.pos.xyz));
	//float3 color = Sky.Sample(Sampler, uv).rgb;

	//color = Diffuse.Sample(Sampler, input.uv).rgb;
	float4 diffuse = Diffuse.Sample(Sampler, input.pos).rgba;

	return float4(diffuse.xyz, 1.0f);
	//return float4(color, 1.0);
}
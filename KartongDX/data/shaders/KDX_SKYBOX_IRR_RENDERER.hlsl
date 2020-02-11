#include "segments/per_object_buffer.hlsl"
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
};


VS_Output vs_main(const VS_Input input)
{
	VS_Output output;
	output.position = mul(float4(input.position.xyz, 1.0), WorldViewProj);
	output.pos = float4(input.position, 1.0f);

	return output;
};

Texture2D<float4> EnviromentMap : register(t0);
SamplerState Sampler : register(s0);

float2 SampleSphericalMap(float3 v)
{
	float2 invAtan = float2(0.1591, 0.3183);
	float2 uv = float2(atan2(v.z, v.x), asin(v.y));
	uv *= invAtan;
	uv += 0.5;
	return uv;
}
float4 ps_main(const VS_Output input) : SV_TARGET
{
	float3 normal = normalize(input.pos.xyz);
	float3 irradiance = float3(0.0f, 0.0f, 0.0f);

	float3 up = float3(0.0f, 1.0f, 0.0f);
	float3 right = cross(up, normal);
	up = cross(normal, right);

	float sampleDelta = 0.025f;
	float nrSamples = 0.0f;

	for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
	{
		for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
		{
			float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));

			float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;

			float2 uv = SampleSphericalMap(sampleVec);

			irradiance += EnviromentMap.Sample(Sampler, uv).rgb * cos(theta) * sin(theta);
			nrSamples++;
		}
	}
	irradiance = PI * irradiance * (1.0 / nrSamples);
	return float4(irradiance, 1.0f);
}
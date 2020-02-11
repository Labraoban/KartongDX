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
	float2 uv = SampleSphericalMap(normalize(input.pos.xyz));
	float3 color = EnviromentMap.Sample(Sampler, uv).rgb;

	return float4(color, 1.0f);
}
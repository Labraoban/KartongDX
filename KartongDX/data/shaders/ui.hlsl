
#include "segments/per_object_buffer.hlsl"
#include "segments/per_frame_buffer.hlsl"
#include "segments/utility.hlsl"

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
	float3 normal : NORMAL;
	float2 uv : TEXCOORD;
	float4 color : COLOR;
};

VS_Output vs_main(const VS_Input input)
{
	VS_Output output;
	output.position = mul(float4(input.position.rgb, 1.0), WorldViewProj);
	output.color = input.color;
	output.uv = input.uv;
	output.normal = input.normal;// normalize(mul(float4(input.normal, 0), InverseWorld));

	return output;
};

Texture2D<float4> Diffuse : register(t0);
Texture2D<float4> Normal : register(t1);
SamplerState Sampler : register(s0);

float4 get_diffuse(VS_Output input)
{
	return Diffuse.Sample(Sampler, input.uv).rgba;
}

float4 ps_main(VS_Output input) : SV_TARGET
{
	return get_diffuse(input);
	return float4(input.uv, 1.0f, 1.0f);


	
}
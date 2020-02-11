
#include "segments/per_object_buffer.hlsl"
#include "segments/per_frame_buffer.hlsl"
#include "segments/utility.hlsl"

static const float PI = 3.141592;

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
	float3 pos : POSITION0;
	float2 uv		: TEXCOORD;
	float4 color	: COLOR;
};

VS_Output vs_main(const VS_Input input)
{
	VS_Output output;

													  // 1.0f to display Skybox as a worldobject
	output.position		= mul(float4(input.position.xyz, 0.0f), View);
	output.position		= mul(float4(output.position.xyz, 1.0f), Proj).xyww;

	output.color		= input.color;
	output.uv			= input.uv;

	output.pos = input.position.xyz;

	return output;
};


SamplerState Sampler : register(s0);
TextureCube Skybox : register(t0);


float4 ps_main(VS_Output input) : SV_TARGET
{
	//float4 diffuse = Diffuse.Sample(Sampler, input.uv).rgba;
	float4 diffuse = Skybox.Sample(Sampler, input.pos).rgba;

	return float4(diffuse.xyz, 1.0f);
}
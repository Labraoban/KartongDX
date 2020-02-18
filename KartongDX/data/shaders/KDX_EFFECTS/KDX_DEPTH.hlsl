
#include "segments/per_object_buffer.hlsl"
#include "segments/per_frame_buffer.hlsl"
#include "segments/utility.hlsl"

static const float PI = 3.141592;
static const float F_DIELECTRIC = 0.04f;

struct VS_Input
{
	uint vI : SV_VERTEXID;
};

struct VS_Output
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
};

#include "segments/effect_vs_main.hlsl"

Texture2D<float4> _RenderTex : register(t0);
Texture2D<float4> _DepthTex : register(t1);
SamplerState Sampler : register(s0);

float4 ps_main(VS_Output input) : SV_TARGET
{
	float3 color = _DepthTex.Sample(Sampler, input.uv).r;
	float depth = color.r;

	float4 ndcCoords = float4(0, 0, depth, 1.0f);
	float4 viewCoords = mul(InvProj, ndcCoords);
	float linearDepth = (viewCoords.z / viewCoords.w);

	//return float4(color, 1.0f);

	//float zw = depth;
	//float linearDepth = Proj._43 / (zw - Proj._33);
	//linearDepth = 1.0f - depth;
	//	linearDepth = linearDepth / _ZFar;

	return float4(linearDepth, linearDepth, linearDepth, 1.0f);
}

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

float3 ReinhardTonemap(float3 hdrColor, float gamma)
{
	float3 mapped = hdrColor / (hdrColor + float3(1.0, 1.0f, 1.0f));
	mapped = pow(mapped, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));

	return mapped;
}

float3 ExposureMap(float3 hdrColor, float exposure, float gamma)
{
	float3 mapped = float3(1.0f, 1.0f, 1.0f) - exp(-hdrColor * exposure);
	mapped = pow(mapped, float3(1.0f / gamma, 1.0f / gamma, 1.0f / gamma));
	return mapped;
}

float4 ps_main(VS_Output input) : SV_TARGET
{
	float3 hdrColor = _RenderTex.Sample(Sampler, input.uv).rgb;
	//float3 tonemapped = ReinhardTonemap(hdrColor, 0.9f);
	float3 tonemapped = ExposureMap(hdrColor, 0.1f, 2.2f);

#ifdef _24_BIT_COLOR_
	return float4(hdrColor, 1.0f);
#else
	return float4(tonemapped, 1.0f);
#endif

}
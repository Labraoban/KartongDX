
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



VS_Output vs_main(VS_Input input)
{
	VS_Output output;

	float2 texcoord = float2(input.vI & 1, input.vI >> 1);

	output.uv = texcoord;
	output.pos = float4((texcoord.x - 0.5f) * 2, -(texcoord.y - 0.5f) * 2, 0, 1);

	return output;
}

Texture2D<float4> _RenderTex : register(t0);
Texture2D<float4> _DepthTex : register(t1);
SamplerState Sampler : register(s0);

float4 ps_main(VS_Output input) : SV_TARGET
{
	float3 color = _RenderTex.Sample(Sampler, input.uv).rgb;
	float gray = (color.r + color.g + color.b) / 3;
	return float4(gray, gray, gray, 1.0f);
}
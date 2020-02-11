#ifndef __EFFECT_VS_MAIN__
#define __EFFECT_VS_MAIN__




VS_Output vs_main(VS_Input input)
{
	VS_Output output;

	float2 texcoord = float2(input.vI & 1, input.vI >> 1);

	output.uv = texcoord;
	output.pos = float4((texcoord.x - 0.5f) * 2, -(texcoord.y - 0.5f) * 2, 0, 1);

	return output;
}

#endif // __PER_OBJECT_BUFFER__
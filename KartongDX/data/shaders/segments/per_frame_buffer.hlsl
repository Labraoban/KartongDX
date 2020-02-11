#ifndef __LIGHT_CONSTANT_BUFFER__
#define __LIGHT_CONSTANT_BUFFER__

cbuffer PerFrameBuffer : register(b1)
{
    float3 LightDir;
    float3 ViewPosition;
    float3 ViewDir;
	float _ZFar;
};

#endif // __LIGHT_CONSTANT_BUFFER__
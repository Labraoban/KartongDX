#ifndef __PER_OBJECT_BUFFER__
#define __PER_OBJECT_BUFFER__

cbuffer PerObjectBuffer : register(b0)
{
	matrix World;
	matrix View;
	matrix Proj;
    matrix WorldViewProj;
	matrix NormalMatrix;
	matrix InvProj;
};

#endif // __PER_OBJECT_BUFFER__
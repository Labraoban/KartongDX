#ifndef __UTILITY__
#define __UTILITY__

#define SKYBOX_ENV_SLOT t32
#define SKYBOX_IRR_SLOT t33
#define SKYBOX_RAD_SLOT t34;
#define SKYBOX_BRDF_SLOT t35;

inline float NdL(float3 n, float3 l)
{
    return max(dot(n, l), 0.0f);
};

float2 DirectionToEnvUV(float3 normal, float3 viewdir)
{
	float3 r = reflect(-viewdir, normal);
	float m = 2.0f * sqrt(pow(r.x, 2.0f) + pow(r.y, 2.f) + pow(r.z + 1.0f, 2.0f));
	float2 reflectionCoord = r.xy / m + .5;
	return reflectionCoord;
}

float2 SpericalEnvMap(float3 v)
{
	const float2 invAtan = float2(0.1591f, 0.3183f);
	float2 uv = float2(atan2(v.z, v.x), asin(v.y));
	uv *= invAtan;
	uv += 0.5f;
	return uv;
}


inline float square(float x)
{
	return x * x;
}

#endif // __UTILITY__
uniform sampler2D color;
uniform sampler2D normal;
uniform sampler2D depth;

uniform vec2 iResolution;

uniform float iGlobalTime;

in vec2 uv;

#define MOD3 vec3(.1031,.11369,.13787)

vec2 uvScale = vec2(16.0 / iResolution.x, 16.0 / iResolution.y);

vec3 hash22(vec2 p)
{
	vec3 p3 = fract(vec3(p.xyx) * MOD3);
    p3 += dot(p3, p3.yzx+19.19);
    return fract(vec3((p3.x + p3.y)*p3.z, (p3.x+p3.z)*p3.y, (p3.y+p3.z)*p3.x));
}

vec3 getRandom(vec2 uv) {
    return normalize(hash22(uv*126.1231) * 2. - 1.);
}

void main() 
{
	vec3 randomSampleOffsets[24] = {
		vec3(1, 0, 0),
		vec3(-1, 0, 0),
		vec3(0, 1, 0),
		vec3(0, -1, 0),

		vec3(1, 1, 0),
		vec3(-1, 1, 0),
		vec3(1, -1, 0),
		vec3(-1, -1, 0),

		vec3(1, 0, 1),
		vec3(-1, 0, 1),
		vec3(0, 1, 1),
		vec3(0, -1, 1),

		vec3(1, 1, 1),
		vec3(-1, 1, 1),
		vec3(1, -1, 1),
		vec3(-1, -1, 1),

		vec3(1, 0, -1),
		vec3(-1, 0, -1),
		vec3(0, 1, -1),
		vec3(0, -1, -1),

		vec3(1, 1, -1),
		vec3(-1, 1, -1),
		vec3(1, -1, -1),
		vec3(-1, -1, -1)
	};

	float depthAtPos = texture2D(depth, uv).x;

	float ao = 0.0;

	float sampleSize = 200.0;

	for(float i = 0.0; i < sampleSize; i += 1.0)
	{
		vec3 randomSampleOffset = getRandom(vec2(i*uv));

		float actualSampleDepth = texture2D(depth, clamp(uv + randomSampleOffset.xy * uvScale,0.0001,0.999)).x;

		float dist = (depthAtPos+0.1*randomSampleOffset.z - actualSampleDepth);

		ao += clamp(dist / 0.01, 0.0, 1.0);
	}

	ao = clamp((1.0 - ao / sampleSize)*2.0, 0.0, 1.0);
	
	//float isBelow05 = step(ao, 0.5);
	//ao = (isBelow05 * (ao / 0.5) * 0.9) + (1.0 - isBelow05) * (0.9 + ((ao - 0.5) * 2.0) * 0.1);

	gl_FragColor = vec4(ao);
}
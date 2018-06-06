uniform sampler2D color;
uniform sampler2D normal;
uniform sampler2D depth;

uniform vec2 iResolution;

in vec2 uv;

vec2 uvScale = vec2(4.0 / iResolution.x, 4.0 / iResolution.y);

float rand(in float a) 
{ 
	return fract((cos(dot(vec2(a,a * a) ,vec2(12.9898,78.233))) * 43758.5453)); 
}


vec3 randomCoord(int seed)
{
	return vec3(rand(seed), rand(seed * seed), rand(seed * seed * seed));
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

	for(int i = 0; i < 24; i++)
	{
		vec3 randomSampleOffset = normalize(randomSampleOffsets[i]);

		float sampleDepth = depthAtPos + randomSampleOffset.z;

		float actualSampleDepth = texture2D(depth, uv + randomSampleOffset.xy * uvScale).x;

		ao += step(sampleDepth - 2e-4, actualSampleDepth) / 24.0;
	}
	
	vec4 col = texture2D(color, uv);

	gl_FragColor = vec4(vec3(1) * ao, col.a);
}
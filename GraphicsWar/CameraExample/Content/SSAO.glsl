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
	float depthAtPos = texture2D(depth, uv).x;

	depthAtPos = step(depthAtPos, 0.001) * 1000 + (1 - step(depthAtPos, 0.001)) * depthAtPos;

	float ao = 0.0;

	vec3 randomSampleOffsets[8] = {
		vec3(1, 0, 0),
		vec3(-1, 0, 0),
		vec3(0, 1, 0),
		vec3(0, -1, 0),

		vec3(1, 1, 0),
		vec3(-1, 1, 0),
		vec3(1, -1, 0),
		vec3(-1, -1, 0)

		/*vec3(1, 0, 1),
		vec3(1, 0, -1),
		vec3(1, 1, 0),
		vec3(1, -1, 0),
		vec3(-1, 0, 1),
		vec3(-1, 0, -1),
		vec3(-1, 1, 0),
		vec3(-1, -1, 0),

		vec3(1, 0, 1),
		vec3(-1, 0, 1),
		vec3(0, 1, 1),
		vec3(0, -1, 1),
		vec3(1, 0, -1),
		vec3(-1, 0, -1),
		vec3(0, 1, -1),
		vec3(0, -1, -1),

		vec3(0, 1, 1),
		vec3(1, 1, 0),
		vec3(0, 1, -1),
		vec3(-1, 1, 0),
		vec3(0, -1, 1),
		vec3(1, -1, 0),
		vec3(0, -1, -1),
		vec3(-1, -1, 0)*/
	};

	for(int i = 0; i < 8; i++)
	{
		vec3 randomSampleOffset = normalize(randomSampleOffsets[i]);

		//float scale = float(i) / 6.0;
		//scale *= rand(i * 153);
		//randomSampleOffset *= mix(0.1, 1.0, scale * scale);

		randomSampleOffset.xy *= uvScale;

		vec3 newPoint = vec3(uv + randomSampleOffset.xy, depthAtPos+randomSampleOffset.z);

		float sampleDepth = texture2D(depth, newPoint.xy).x;


		ao += step(newPoint.z, sampleDepth) / 8.0;
	}
	
	vec4 col = texture2D(color, uv);

	gl_FragColor = vec4(vec3(1) * ao, col.a);
}
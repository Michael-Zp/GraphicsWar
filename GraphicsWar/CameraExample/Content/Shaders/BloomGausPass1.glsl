#version 430 core

const float PI = 3.14159265359;
const float Euler = 2.71828182846;
const float GaussSigma = 20;
const int GaussSize = 20;

uniform sampler2D image;
uniform float effectScale = 0.3;

in vec2 uv;

float gaus(float x)
{
	return (1 / sqrt(2 * PI) * GaussSigma) * pow(Euler, -pow(x, 2) / (2 * pow(GaussSigma, 2)));
}

void main()
{
	vec3 gx = vec3(0);
	float factorCount = 0;

	
	for (int i = 0 - int(floor(GaussSize / 2.0)); i <= floor(GaussSize / 2.0); i++) 
	{
		vec3 aSample  = texelFetch(image, ivec2(gl_FragCoord) + ivec2(i, 0), 0).rgb;
		float factor = gaus(i);
		gx += factor * aSample;
		factorCount += factor;
	}

	gx /= factorCount;

	float bloom = step(1.3, length(gx));

	vec3 color = bloom * gx + (1 - bloom) * texelFetch(image, ivec2(gl_FragCoord), 0).rgb;

	gl_FragColor = vec4(color, 1.0);
}

#version 430 core

const float PI = 3.14159265359;
const float Euler = 2.71828182846;
const float GaussSigma = 20;

uniform sampler2D image;
uniform float effectScale = 0.3;
uniform float GaussSize = 20;

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
		vec3 aSample  = texelFetch(image, ivec2(gl_FragCoord) + ivec2(0, i), 0).rgb;
		float factor = gaus(i);
		gx += factor * aSample;
		factorCount += factor;
	}

	gx /= factorCount;
	
	gl_FragColor = vec4(vec3(gx), 1.0);
}

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
	vec4 gx = vec4(0);
	float factorCount = 0;

	
	for (int i = 0 - int(floor(GaussSize / 2.0)); i <= floor(GaussSize / 2.0); i++) 
	{
		vec4 aSample  = texelFetch(image, ivec2(gl_FragCoord) + ivec2(i, 0), 0);
		float factor = gaus(i);
		gx += factor * aSample;
		factorCount += factor;
	}

	gx /= factorCount;
	
	gl_FragColor = gx;
}

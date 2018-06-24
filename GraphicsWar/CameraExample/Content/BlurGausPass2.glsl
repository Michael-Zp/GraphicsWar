#version 430 core

const float PI = 3.14159265359;
const float Euler = 2.71828182846;
const float GaussSigma = 20;
const int GaussSize = 20;

uniform sampler2D image;
uniform float effectScale = 0.3;

in vec2 uv;

float grayScale(vec3 color)
{
	return 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
}

float gaus(float x)
{
	return (1 / sqrt(2 * PI) * GaussSigma) * pow(Euler, -pow(x, 2) / (2 * pow(GaussSigma, 2)));
}

void main()
{
	float gx = 0;
	float factorCount = 0;

	
	for (int i = 0 - int(floor(GaussSize / 2.0)); i <= floor(GaussSize / 2.0); i++) 
	{
		vec3 aSample  = texelFetch(image, ivec2(gl_FragCoord) + ivec2(0, i), 0).rgb;
		float factor = gaus(i);
		gx += factor * grayScale(aSample);
		factorCount += factor;
	}

	gx /= factorCount;
	
	gl_FragColor = vec4(vec3(gx), 1.0);
}

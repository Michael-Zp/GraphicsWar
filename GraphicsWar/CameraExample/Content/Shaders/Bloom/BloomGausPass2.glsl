#version 430 core

const float PI = 3.14159265359;
const float Euler = 2.71828182846;
const float GaussSigma = 20;
const int GaussSize = 20;

uniform sampler2D image;

in vec2 uv;

float gaus(float x)
{
	return Euler / (2 * PI * pow(GaussSigma,2) - pow(x,2)/(2 * pow(GaussSigma, 2)));
}

void main()
{
	vec3 gx = vec3(0);
	float factorCount = 0;

	float isBlurred = 0;
	
	for (int i = 0 - int(floor(GaussSize / 2.0)); i <= floor(GaussSize / 2.0); i++) 
	{
		vec3 asample  = texelFetch(image, ivec2(gl_FragCoord) + ivec2(0, i), 0).rgb;
		float factor = gaus(i);
		gx += factor * asample;
		factorCount += factor;

		isBlurred += step(0.99,asample.r);
		isBlurred += step(0.99,asample.g);
		isBlurred += step(0.99,asample.b);
	}

	isBlurred = min(isBlurred, 1);

	gx /= factorCount;

	float bloom = step(0.5, isBlurred);

	vec3 color = bloom * gx + (1 - bloom) * texelFetch(image, ivec2(gl_FragCoord), 0).rgb;

	gl_FragColor = vec4(color, 1.0);
}

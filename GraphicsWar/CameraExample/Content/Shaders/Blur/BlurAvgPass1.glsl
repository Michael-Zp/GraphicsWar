#version 430 core

const float PI = 3.14159265359;
const float Euler = 2.71828182846;
const float GaussSigma = 20;

uniform sampler2D image;
uniform float Size = 20;

in vec2 uv;

void main()
{
	vec4 gx = vec4(0);
	float factorCount = 0;

	
	for (int i = 0 - int(floor(Size / 2.0)); i <= floor(Size / 2.0); i++) 
	{
		vec4 aSample  = texelFetch(image, ivec2(gl_FragCoord) + ivec2(i, 0), 0);
		gx += aSample;
	}

	gx /= Size;
	
	gl_FragColor = gx;
}

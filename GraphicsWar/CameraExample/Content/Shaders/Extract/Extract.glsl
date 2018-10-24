#version 430 core

uniform sampler2D image;

void main()
{

	vec4 color = texelFetch(image, ivec2(gl_FragCoord), 0);

	float isBlurred = 0;

	isBlurred += step(0.99,color.r);
	isBlurred += step(0.99,color.g);
	isBlurred += step(0.99,color.b);

	isBlurred = min(isBlurred, 1);

	gl_FragColor = isBlurred * color;
}

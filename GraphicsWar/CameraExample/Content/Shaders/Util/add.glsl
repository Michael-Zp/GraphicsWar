#version 430 core

uniform sampler2D image1;
uniform sampler2D image2;
uniform float factor;

in vec2 uv;

out vec4 color;

void main() 
{
	float alphaFactor = factor * texture2D(image2, uv).a;
	color = mix(texture2D(image1, uv), texture2D(image2, uv), alphaFactor);
}
#version 430 core

uniform sampler2D depth1;
uniform sampler2D depth2;
uniform sampler2D bufferOne1;
uniform sampler2D bufferOne2;
uniform sampler2D bufferTwo1;
uniform sampler2D bufferTwo2;
uniform sampler2D bufferThree1;
uniform sampler2D bufferThree2;

in vec2 uv;

out float depth;
out vec4 bufferOne;
out vec4 bufferTwo;
out vec4 bufferThree;

void main() 
{
	float depth1Value = texture(depth1, uv).x;
	float depth2Value = texture(depth2, uv).x;

	float depth2Greater = step(depth1Value, depth2Value);
	
	depth = depth2Greater * depth1Value + (1. - depth2Greater) * depth2Value;
	bufferOne = depth2Greater * texture(bufferOne1, uv) + (1. - depth2Greater) * texture(bufferOne2, uv);
	bufferTwo = depth2Greater * texture(bufferTwo1, uv) + (1. - depth2Greater) * texture(bufferTwo2, uv);
	bufferThree = depth2Greater * texture(bufferThree1, uv) + (1. - depth2Greater) * texture(bufferThree2, uv);
}
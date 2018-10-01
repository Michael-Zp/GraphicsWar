#version 430 core

in Data {
	vec3 normal;
	vec3 position;
	float depth;
} i;

out float depth;


void main() 
{
	depth = i.depth;
}
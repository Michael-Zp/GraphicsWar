#version 430 core

in Data {
	vec3 normal;
	vec3 position;
	float depth;
} i;

out float depth;
out vec3 normal;


void main() 
{
	normal = i.normal;
	depth = i.depth;
}
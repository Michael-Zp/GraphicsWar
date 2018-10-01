#version 430 core

in Data {
	vec3 normal;
	vec3 position;
	float depth;
} i;

out vec4 color;
out vec3 normal;
out float depth;
out vec3 position;


void main() 
{
	color = vec4(1, 0, 0, 1);
	normal = i.normal;	
	depth = i.depth;
	position = i.position;
}
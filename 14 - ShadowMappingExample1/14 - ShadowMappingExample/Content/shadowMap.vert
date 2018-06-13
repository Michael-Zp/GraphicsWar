#version 430 core	

uniform mat4 camera;

in vec4 position;
in vec3 normal;

out blockData
{
	vec3 normal;
} o;

void main() 
{
	o.normal = normal;
	gl_Position = camera * position;
}
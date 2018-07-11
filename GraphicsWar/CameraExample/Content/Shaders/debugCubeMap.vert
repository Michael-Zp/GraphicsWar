#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;

out Data {
	out vec3 normal;
	out vec3 position;
} o;

void main() 
{
	o.normal = normal;
	o.position = position;

	vec4 outPos = camera * vec4(position, 1.0);

	gl_Position = outPos;
}
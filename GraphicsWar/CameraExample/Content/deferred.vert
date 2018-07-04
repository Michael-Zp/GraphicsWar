#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;

out vec3 n;
out vec3 pos;
out float d;

void main() 
{
	n = (transform * vec4(normal,0)).xyz;
	pos = (transform * vec4(position, 1.0)).xyz;

	vec4 outPos = camera * vec4(pos, 1.0);

	gl_Position = outPos;
	d = outPos.z;
}
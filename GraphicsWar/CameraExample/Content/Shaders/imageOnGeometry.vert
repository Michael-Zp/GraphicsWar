#version 430 core

uniform mat4 camera;

in vec3 position;

out float d;

void main() 
{
	vec4 pos = camera * vec4(position, 1.0);
	gl_Position = pos;
	d = pos.z;
}
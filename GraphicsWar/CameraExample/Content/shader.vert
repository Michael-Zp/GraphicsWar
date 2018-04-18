#version 430 core

uniform float time;
uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;

out vec3 n;

void main() 
{
	n = normal;

	gl_Position = camera * transform * vec4(position, 1.0);
}
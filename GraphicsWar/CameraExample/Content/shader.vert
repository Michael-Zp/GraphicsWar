#version 430 core

uniform float time;
uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;

out vec3 n;
out vec3 pos;
out float d;

void main() 
{
	n = (transform*vec4(normal,0)).xyz;
	pos = (transform * vec4(position, 1.0)).xyz;
	gl_Position = camera * vec4(pos, 1.0);
	d = gl_Position.z;
}
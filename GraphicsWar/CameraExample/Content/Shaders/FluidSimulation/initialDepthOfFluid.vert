#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;

out Data {
	vec3 normal;
	vec3 position;
	float depth;
} o;

void main() 
{
	o.normal = normal;
	o.position = (transform * vec4(position, 1.0)).xyz;
	vec4 outPos = camera * vec4(o.position, 1.0);

	gl_Position = outPos;
	o.depth = outPos.z;
}
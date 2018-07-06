#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;
in vec2 uv;

out Data {
	vec3 normal;
	vec3 position;
	float depth;
	vec2 uv;
} o;

void main() 
{
	o.normal = (transform * vec4(normal, 0.0)).xyz;
	o.position = (transform * vec4(position, 1.0)).xyz;
	o.uv = uv;

	vec4 outPos = camera * vec4(o.position, 1.0);

	gl_Position = outPos;
	o.depth = outPos.z;
}
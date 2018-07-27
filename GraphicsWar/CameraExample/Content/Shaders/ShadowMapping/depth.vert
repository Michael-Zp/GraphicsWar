#version 430 core

uniform mat4 camera;

in vec3 position;
in mat4 transform;

out Data {
	float depth;
} o;

void main() 
{
	vec4 pos = vec4((transform * vec4(position, 1.0)).xyz, 1.0);

	vec4 outPos = camera * pos;

	gl_Position = outPos;
	o.depth = outPos.z/outPos.w;
}
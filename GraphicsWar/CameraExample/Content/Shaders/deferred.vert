#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;
in vec2 uv;
in vec3 tangent;
in vec3 bitangent;

out Data {
	vec3 normal;
	vec3 position;
	float depth;
	vec2 uv;
	mat4 transform;
	vec3 tangent;
	vec3 bitangent;
} o;

void main() 
{
	o.normal = normal;
	o.position = (transform * vec4(position, 1.0)).xyz;
	o.uv = uv;
	o.transform = transform;
	o.tangent = tangent;
	o.bitangent = bitangent;

	vec4 outPos = camera * vec4(o.position, 1.0);

	gl_Position = outPos;
	o.depth = outPos.z;
}
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
	mat3 tbn;
} o;

void main() 
{
	o.normal = normal;
	o.position = (transform * vec4(position, 1.0)).xyz;
	o.uv = uv;
	o.transform = transform;
	
	vec3 t = vec4(normalize(tangent), 0).xyz;
	vec3 b = vec4(normalize(bitangent), 0).xyz;
	vec3 n = vec4(normalize(normal), 0).xyz;
	o.tbn = mat3(t, b, n);

	vec4 outPos = camera * vec4(o.position, 1.0);

	gl_Position = outPos;
	o.depth = outPos.z;
}
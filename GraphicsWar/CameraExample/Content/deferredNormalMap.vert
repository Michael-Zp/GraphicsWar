#version 430 core

uniform mat4 camera;
uniform sampler2D normalMap;

in vec3 position;
in vec3 normal;
in mat4 transform;
in vec2 uv;

out vec3 n;
out vec3 pos;
out float d;

void main() 
{
	n = vec3(0);//texture2D(normalMap, uv).xyz;
	pos = (transform * vec4(position, 1.0)).xyz;

	vec4 outPos = camera * vec4(pos, 1.0);

	gl_Position = outPos;
	d = outPos.z;
}
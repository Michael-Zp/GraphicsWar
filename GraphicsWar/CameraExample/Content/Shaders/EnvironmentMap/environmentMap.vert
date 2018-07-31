#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in mat4 transform;

out Data {
	out vec3 normal;
	out vec3 position;
	out float depth;
} o;

void main() 
{
	o.normal = (transform * vec4(normal,0)).xyz;
	o.position = (transform * vec4(position, 1.0)).xyz;

	vec4 pos = camera * vec4(o.position, 1.0);
	o.depth = pos.z;

	gl_Position = pos;

}
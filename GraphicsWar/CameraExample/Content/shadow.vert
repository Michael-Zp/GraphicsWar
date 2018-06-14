#version 430 core

uniform mat4 lightCamera;
uniform mat4 camera;

in vec3 position;
in mat4 transform;

out Data {
	vec4 pos_light;
} o;

void main() 
{
	vec4 pos = transform * vec4(position, 1.0);

	o.pos_light = lightCamera * pos;

	gl_Position = camera * pos;
}
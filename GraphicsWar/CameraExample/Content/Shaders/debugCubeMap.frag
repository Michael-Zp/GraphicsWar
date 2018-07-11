#version 430 core

#extension GL_NV_shadow_samplers_cube : enable

uniform samplerCube cubeMap;

in Data {
	in vec3 normal;
	in vec3 position;
} i;

out vec4 color;

void main() 
{
	color = vec4(textureCube(cubeMap, i.normal));
	//color = texture(cubeMap, i.normal);
}
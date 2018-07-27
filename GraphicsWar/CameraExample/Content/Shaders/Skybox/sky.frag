#version 430 core

#extension GL_NV_shadow_samplers_cube : enable

uniform samplerCube cubeMap;

uniform float mipmapLevel;

in Data {
	in vec3 normal;
	in vec3 position;
} i;

out vec4 color;

void main() 
{
	color = texture(cubeMap, i.normal, mipmapLevel);
}
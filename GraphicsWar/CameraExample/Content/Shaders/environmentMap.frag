#version 430 core

#extension GL_NV_shadow_samplers_cube : enable

uniform vec2 iResolution;
uniform samplerCube cubeMap;
uniform sampler2D depth;
uniform vec3 camPos;

in Data {
	in vec3 normal;
	in vec3 position;
	in float depth;
} i;

out vec4 color;

void main() 
{
	vec2 uv = gl_FragCoord.xy / iResolution;

	float isOccluded = step(texture(depth,uv).x, i.depth - 0.01);

	vec3 reflection = reflect(i.position - camPos, i.normal);
	color = mix(textureCube(cubeMap, reflection), vec4(0), isOccluded);
}
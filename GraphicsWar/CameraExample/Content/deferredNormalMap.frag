#version 430 core

#include "lightCalculation.glsl"

uniform vec3 camPos;
uniform sampler2D normalMap;

in Data {
	vec3 normal;
	vec3 position;
	float depth;
	vec2 uv;
} i;

out vec4 color;
out vec3 normal;
out float depth;
out vec3 position;

void main() 
{
	vec4 materialColor = vec4(0.8,0.8,0.8,1);
	vec3 norm = normalize(texture2D(normalMap, i.uv).xyz);
	
	/*
	vec3 materialColor = vec3(1.0);
	vec3 lightColor = vec3(1.0);
	vec3 ambientLightColor = vec3(0.15);
	vec3 lightDirection = normalize(vec3(0,-1.0,0));
	vec3 viewDirection = normalize(i.position - camPos);

	color = calculateLight(materialColor, lightColor, ambientLightColor, lightDirection, viewDirection, norm);
	*/

	color = materialColor;
	normal = normalize(norm);
	depth = i.depth;
	position = i.position;

}
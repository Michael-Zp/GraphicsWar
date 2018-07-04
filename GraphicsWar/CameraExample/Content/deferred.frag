#version 430 core

#include "lightCalculation.glsl"

uniform vec3 camPos;

in vec3 n;
in float d;
in vec3 pos;

out vec4 color;
out vec3 normal;
out float depth;
out vec3 position;

void main() 
{
	vec4 materialColor = vec4(0.8,0.8,0.8,1);
	
	/*
	vec3 materialColor = vec3(1.0);
	vec3 lightColor = vec3(1.0);
	vec3 ambientLightColor = vec3(0.15);
	vec3 lightDirection = normalize(vec3(0,-1.0,0));
	vec3 viewDirection = normalize(pos - camPos);
	vec3 norm = normalize(n);

	color = calculateLight(materialColor, lightColor, ambientLightColor, lightDirection, viewDirection, norm);
	*/


	color = materialColor;
	normal = normalize(n);
	depth = d;
	position = pos, 0.0;

}
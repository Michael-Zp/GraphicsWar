#version 430 core

#include "lightCalculation.glsl"

uniform vec3 camPos;
uniform vec3 lightDirection;
uniform vec3 lightColor;
uniform vec3 ambientColor;
uniform sampler2D normal;
uniform sampler2D materialColor;
uniform sampler2D shadowSurface;
uniform sampler2D position;

in vec2 uv;

out vec4 color;

void main() 
{
	vec3 matColor = texture2D(materialColor, uv).rgb;
	vec3 viewDirection = normalize(texture2D(position, uv).xyz - camPos);
	vec3 norm = normalize(texture2D(normal, uv).xyz);

	color = calculateLight(matColor, lightColor, ambientColor, lightDirection, viewDirection, norm);

	color *= texture2D(shadowSurface, uv).x;
}
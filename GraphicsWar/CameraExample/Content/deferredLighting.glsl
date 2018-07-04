#version 430 core

#include "lightCalculation.glsl"

uniform vec3 camPos;
uniform vec3 ambientColor;
uniform sampler2D normals;
uniform sampler2D materialColor;
uniform sampler2D shadowSurface;
uniform sampler2D position;

struct Light
{
	vec3 lightPos;
	float align1;
	vec3 lightDir;
	float align2;
	vec3 lightCol;
	float lightIntense;
};

layout(std430) buffer Lights
{
	Light light[];
};

in vec2 uv;

out vec4 color;


void main() 
{
	vec3 matColor = texture2D(materialColor, uv).rgb;
	vec3 viewDirection = normalize(texture2D(position, uv).xyz - camPos);
	vec3 norm = normalize(texture2D(normals, uv).xyz);

	color = vec4(0);

	for(int i = 0; i < 8; i++) 
	{
		color += calculateLight(matColor, light[i].lightCol, ambientColor, light[i].lightDir, viewDirection, norm) * light[i].lightIntense;
	}

	color *= texture2D(shadowSurface, uv).x;
}
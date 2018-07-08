#version 430 core

#include "lightCalculation.glsl"

uniform vec3 camPos;
uniform sampler2D normalMap;

in Data {
	vec3 normal;
	vec3 position;
	float depth;
	vec2 uv;
	mat4 transform;
	vec3 tangent;
	vec3 bitangent;
} i;

out vec4 color;
out vec3 normal;
out float depth;
out vec3 position;

void main() 
{
	vec4 materialColor = vec4(0.8,0.8,0.8,1);
	vec3 norm = normalize((i.transform * vec4(texture2D(normalMap, i.uv).xyz, 0)).xyz);
	vec3 t = normalize(i.tangent);
	vec3 b = normalize(i.bitangent);


	mat3 tbn = mat3(t.x, t.y, t.z, b.x, b.y, b.z, norm.x, norm.y, norm.z);

	norm = norm * tbn;

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
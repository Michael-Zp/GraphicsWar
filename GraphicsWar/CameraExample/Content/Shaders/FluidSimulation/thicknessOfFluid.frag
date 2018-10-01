#version 430 core

uniform vec3 camPos;

in Data {
	vec3 normal;
	vec3 position;
	float depth;
} i;

out vec4 thickness;


void main() 
{
	vec4 eyeDirection = vec4(normalize(camPos - i.position), 0);

	//If hit dead center -> 1; If at edge -> 0
	float thicknessFactor = clamp(dot(normalize(i.normal), eyeDirection.xyz), 0, 1);

	thickness = vec4(.1, 0, 0, thicknessFactor);
}
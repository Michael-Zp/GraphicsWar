#version 430 core

uniform sampler2D depthTex;
uniform sampler2D thicknessTex;
uniform vec3 camPos;
uniform mat4 camera;
uniform mat4 projection;
uniform mat4 view;

in vec2 uv;

out vec4 color;


void main() 
{
	float depth = texture2D(depthTex, uv).x;
	float thickness = clamp(texture2D(thicknessTex, uv).x, 0, 1);

	vec3 lightBlue = vec3(144, 195, 212);
	lightBlue /= 255;

	vec3 darkBlue = vec3(14, 30, 56);
	darkBlue /= 255;

	float thicccEnough = step(0.1, thickness);

	color = thicccEnough * vec4(mix(lightBlue, darkBlue, thickness), thickness) + (1 - thicccEnough) * vec4(0);

	/*
	vec4 clipSpacePosition = vec4(uv, depth, 1.0);
	vec4 viewSpacePosition = inverse(projection) * clipSpacePosition;
	viewSpacePosition /= viewSpacePosition.w;
	
	
	position = thicccEnough * normalize((inverse(camera) * viewSpacePosition).xyz) + (1 - thicccEnough) * vec3(-10000);
	*/

	/*
	vec4 pos = vec4(uv, depth, 1.0);
	vec4 wpos = inverse(view) * inverse(projection) * pos;

	position = wpos.xyz / wpos.w;
	*/

	/*
	vec4 pos = vec4(uv * 2 - 1, depth, 1.0);
	vec4 wpos = inverse(camera) * pos;
	position = thicccEnough * normalize(wpos.xyz) + (1 - thicccEnough) * vec3(-10000);
	*/
	
}
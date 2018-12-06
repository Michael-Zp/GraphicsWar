#version 430 core

uniform sampler2D lightDepth;
uniform vec3 lightDirection;
uniform mat4 lightCamera;
uniform sampler2D positions;
uniform sampler2D normals;

in vec2 uv;

out float color;

void main() 
{
	vec3 pos = texture(positions, uv).xyz;

	vec4 lightPos = lightCamera * vec4(pos,1.);

	vec3 coord = lightPos.xyz / lightPos.w;
	float depth = texture(lightDepth, coord.xy * 0.5 + 0.5).x;

	color = step(coord.z, depth + (1-abs(dot(texture(normals, uv).xyz, lightDirection)))*0.004);
}
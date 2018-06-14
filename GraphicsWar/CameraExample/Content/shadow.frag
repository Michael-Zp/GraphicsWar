#version 430 core

uniform sampler2D lightDepth;

in Data {
	vec4 pos_light;
} i;

out float color;

void main() 
{
	vec3 coord = i.pos_light.xyz / i.pos_light.w;
	float depth = texture(lightDepth, coord.xy * 0.5 + 0.5).x;

	color = step(coord.z, depth +0.001);
}
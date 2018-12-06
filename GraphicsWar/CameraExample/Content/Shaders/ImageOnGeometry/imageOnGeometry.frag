#version 430 core

uniform sampler2D image;
uniform sampler2D depth;
uniform vec2 iResolution;

in float d;

out vec4 color;

void main() 
{
	vec2 uv = gl_FragCoord.xy / iResolution;

	float inside = step(texture(depth, uv).x, d);

	color = inside * texture(image, uv) + (1-inside)* vec4(0.);
}
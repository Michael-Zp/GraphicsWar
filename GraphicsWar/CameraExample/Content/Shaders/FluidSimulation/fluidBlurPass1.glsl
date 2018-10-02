#version 430 core

uniform vec3 camPos;
uniform mat4 projection;
uniform sampler2D depthTex;
uniform sampler2D normalTex;
uniform sampler3D positionTex;

in vec2 uv;

out float depth;
out vec3 position;


float depthDerivate(ivec2 pos, ivec2 dir)
{
	float beforeDepth = texelFetch(depthTex, (ivec2(gl_FragCoord.xy) - dir, 0);
	float afterDepth = texelFetch(depthTex, (ivec2(gl_FragCoord.xy) + dir, 0);

	return (afterDepth - beforeDepth);
}

float depthAccelDerivate(ivec2 pos, ivec2 dir)
{
	float beforeAccel = depthDerivate(pos - dir, dir);
	float afterAccel = depthDerivate(pos + dir, dir);

	return (afterAccel - beforeAccel);
}

float calculateDDerivate(ivec2 dir, vec2 c, float sampleDepth)
{
	vec2 zoverxyBefore = vec2(depthDerivate((ivec2(gl_FragCoord.xy) - dir, vec2(1, 0)), depthDerivate((ivec2(gl_FragCoord.xy) - dir, ivec2(0, 1)));
	vec2 zoverxyAfter = vec2(depthDerivate((ivec2(gl_FragCoord.xy) + dir, vec2(1, 0)), depthDerivate((ivec2(gl_FragCoord.xy) + dir, ivec2(0, 1)));

	float dBefore = c.y * c.y * zoverxyBefore.x * zoverxyBefore.x + c.x * c.x * zoverxyBefore.y * zoverxyBefore.y + c.x * c.x * c.y * c.y * sampleDepth * sampleDepth;
	float dAfter = c.y * c.y * zoverxyAfter.x * zoverxyAfter.x + c.x * c.x * zoverxyAfter.y * zoverxyAfter.y + c.x * c.x * c.y * c.y * sampleDepth * sampleDepth;

	return (dAfter - dBefore);
}

void main()
{
	float sampleDepth = texture2D(depthTex, uv).x;
	ivec2 v = textureSize(depthTex, 0);
	ivec2 f = vec2(tan(3.1415 / 2) / size.x, tan(3.1415 / 2) / size.y));
	
	vec2 W = vec2(((2 * (ivec2(gl_FragCoord.xy).x / v.x) - 1) / f.x, ((2 * (ivec2(gl_FragCoord.xy).y / v.y) - 1) / f.y);
	vec3 Pxy = vec3(W.x, W.y, 1.0);

	vec2 C = vec2(2 / (v.x * f.x), 2 / (f.x * f.y));

	vec3 Nxy = texture2D(normalTex, uv);

	vec2 zoverxy = vec2(depthDerivate((ivec2(gl_FragCoord.xy), ivec2(1, 0)), depthDerivate((ivec2(gl_FragCoord.xy), ivec2(0, 1)));

	float d = c.y * c.y * zoverxy.x * zoverxy.x + c.x * c.x * zoverxy.y * zoverxy.y + c.x * c.x * c.y * c.y * sampleDepth * sampleDepth;

	vec2 e = vec2(0);
	e.x = 0.5 * zoverxy.x * calculateDDerivate(ivec2(1, 0), c, sampleDepth) - depthAccelDerivate((ivec2(gl_FragCoord.xy), ivec2(1, 0)) * d;
	e.y = 0.5 * zoverxy.y * calculateDDerivate(ivec2(0, 1), c, sampleDepth) - depthAccelDerivate((ivec2(gl_FragCoord.xy), ivec2(0, 1)) * d;

	float twoH = (c.y * e.y + c.x * e.x) / pow(d, 3/2);

	depth = twoH;

	position = camPos + normalize((texture2D(positionTex, uv).xyz - camPos)) * depth;
}

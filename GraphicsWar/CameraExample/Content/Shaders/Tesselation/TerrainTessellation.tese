#version 420 core

uniform mat4 camera;
uniform sampler2D displacementMap;
uniform int instanceSqrt = 5;
uniform float iGlobalTime;

layout (quads, equal_spacing, ccw) in;

out Data
{
	flat int instanceID;
	vec3 normal;
	float depth;
	vec3 position;
} o;

in vec4 tcPos[gl_MaxPatchVertices];
in vec2 tcTexCoord[gl_MaxPatchVertices];

patch in int instanceID;

vec2 interpolate(vec2 v1, vec2 v2, vec2 v3, vec2 v4)
{
	vec2 aX = mix(v1, v2, gl_TessCoord.x);
	vec2 bX = mix(v4, v3, gl_TessCoord.x);
	return mix(aX, bX, gl_TessCoord.y);
}

vec4 interpolate(vec4 v1, vec4 v2, vec4 v3, vec4 v4)
{
	vec4 aX = mix(v1, v2, gl_TessCoord.x);
	vec4 bX = mix(v4, v3, gl_TessCoord.x);
	return mix(aX, bX, gl_TessCoord.y);
}

vec4 getCenter(vec4 v1, vec4 v2, vec4 v3, vec4 v4)
{
	vec4 aX = mix(v1, v2, 0.5f);
	vec4 bX = mix(v4, v3, 0.5f);
	return mix(aX, bX, 0.5f);
}

float rand(float seed)
{
	return fract(sin(seed) * 1231534.9);
}

float rand(vec2 seed) 
{ 
    return rand(dot(seed, vec2(12.9898, 783.233)));
}

vec2 rand2(vec2 seed)
{
	const float pi = 3.1415926535897932384626433832795;
	const float twopi = 2 * pi;
	float r = rand(seed) * twopi;
	return vec2(cos(r), sin(r));
}

float noise(float u)
{
	float i = floor(u); // integer position

	//random value at nearest integer positions
	float v0 = rand(i);
	float v1 = rand(i + 1);

	float f = fract(u);
	float weight = f; // linear interpolation
	// weight = smoothstep(0, 1, f); // cubic interpolation
	// weight = quinticInterpolation(f);

	return mix(v0, v1, weight);
}

vec2 currentGridPos;
vec4 center;

vec2 getCurrentGridPos(vec2 coord)
{
	float rowCount = 3;
	float columnCount = 3;
	float row = floor(coord.x / (1 / rowCount));
	float column = floor(coord.y / (1 / columnCount));

	return vec2(row, column);
}

vec2 getGridPointCenterDisplacement(vec2 gridPos)
{
	return normalize(rand2(gridPos)) * 0.45;
}

vec2 getTargetGridCenter(vec2 pos)
{
	//Is either 0|0.5|1 -> * 2 = 0|1|2 -1 -> -1|0|1 -> Works with 3 by 3 square pretty well.
	vec2 dir = vec2(gl_TessCoord.y, gl_TessCoord.x) * 2 - vec2(1);
	
	float leftRight = gl_TessCoord.y * 2 - 1;
	float upDown = gl_TessCoord.x * 2 - 1;
			
	return pos + dir + getGridPointCenterDisplacement(currentGridPos + dir);
}

vec4 getPosition()
{
	vec4 pos = interpolate(tcPos[0], tcPos[1], tcPos[2], tcPos[3]);

	if(instanceID == 3)
	{
		vec2 targetGridCenter = getTargetGridCenter(pos.xz);

		vec2 posDif = ((vec4(targetGridCenter.x, 0, targetGridCenter.y, 0) - pos) / 2).xz; 

		return pos + vec4(posDif.x, 0, posDif.y, 0);
	}
	else
	{
		return pos;
	}
}


void main() 
{
	vec2 texCoord = interpolate(tcTexCoord[0], tcTexCoord[1], tcTexCoord[2], tcTexCoord[3]);

	currentGridPos = getCurrentGridPos(texCoord);
	center = getCenter(tcPos[0], tcPos[1], tcPos[2], tcPos[3]);

	float isCenter = step(0.45, gl_TessCoord.x) * step(gl_TessCoord.x, 0.55) * step(0.45, gl_TessCoord.y) * step(gl_TessCoord.y, 0.55);

	vec2 centerDisplacement = vec2(0);//getGridPointCenterDisplacement(currentGridPos);
	vec4 pos = getPosition();
	pos = (1 - isCenter) * pos + isCenter * (pos + vec4(centerDisplacement.x, 0, centerDisplacement.y, 0));
	
	pos.y = instanceID;

	o.normal = vec3(0, 1, 0);
	o.position = pos.xyz;

	gl_Position = camera * pos;
	o.instanceID = instanceID;
	o.depth = (camera * pos).z;
}
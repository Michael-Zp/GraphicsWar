#version 420 core

uniform mat4 camera;
uniform sampler2D displacementMap;
uniform int instanceSqrt = 5;
uniform float iGlobalTime;

layout (quads, fractional_odd_spacing, ccw) in;

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

float displacementY(vec2 coord)
{
	float rowCount = 10;
	float columnCount = 10;
	float row = floor(coord.x / (1 / rowCount));
	float column = floor(coord.y / (1 / columnCount));	

	float dist = 100000;
	vec2 closestGridPoint = vec2(row, column);

	for(int x = -1; x <= 1; x++)
	{
		for(int y = -1; y <= 1; y++)
		{
			vec2 currentGridPoint = vec2(row, column) + vec2(x, y);

			vec2 currentCenter = rand2(vec2(currentGridPoint.x, currentGridPoint.y));
			currentCenter.x *= 1 / rowCount / 2;
			currentCenter.y *= 1 / columnCount / 2;

			currentCenter += vec2(1 / rowCount / 2, 1 / columnCount / 2);
			currentCenter.x += currentGridPoint.x * (1 / rowCount);
			currentCenter.y += currentGridPoint.y * (1 / columnCount);
			
			float curDist = distance(currentCenter, coord);

			if(curDist < dist)
			{
				dist = curDist;
				closestGridPoint = currentGridPoint;
			}
		}
	}

	return rand(closestGridPoint * 123) * 10;
}

vec3 getNormal(vec2 hitPoint, float delta)
{
	vec2 deltaVec = vec2(delta, 0);

	float nextDeltaX = displacementY(hitPoint - deltaVec.xy);
	float nextDeltaZ = displacementY(hitPoint - deltaVec.yx);
	float previousDeltaX = displacementY(hitPoint + deltaVec.xy);
	float previousDeltaZ = displacementY(hitPoint + deltaVec.yx);

	vec3 unnormalizedGradient = vec3(nextDeltaX - previousDeltaX, 1.0, nextDeltaZ - previousDeltaZ);


	return normalize(unnormalizedGradient);
}



void main() 
{
	vec4 pos = interpolate(tcPos[0], tcPos[1], tcPos[2], tcPos[3]);
	vec2 texCoord = interpolate(tcTexCoord[0], tcTexCoord[1], tcTexCoord[2], tcTexCoord[3]);
	//vec3 terrain = displacement(texCoord) * 4;
	//pos.y = terrain.x;
	pos.y = displacementY(texCoord);

	o.normal = getNormal(texCoord, 1e-2);
	o.position = pos.xyz;

	gl_Position = camera * pos;
	o.instanceID = instanceID;
	o.depth = (camera * pos).z;
}
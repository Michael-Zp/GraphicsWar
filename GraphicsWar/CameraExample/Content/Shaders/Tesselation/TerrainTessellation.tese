#version 420 core

uniform mat4 camera;
uniform sampler2D displacementMap;
uniform int instanceSqrt = 5;
uniform float iGlobalTime;
uniform float size = 2;

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
float isCenter;

vec2 getCurrentGridPos()
{
	float rowCount = 3;
	float columnCount = 3;
	float row = floor(float(instanceID) / rowCount);
	float column = mod(float(instanceID), columnCount);

	return vec2(row, column);
}

vec2 getGridPointCenterDisplacement(ivec2 gridPos)
{
	return normalize(rand2(gridPos)) * 0.45;
}

vec2 getTargetGridCenter(ivec2 dir)
{			
	vec2 otherCenter = center.xz + (dir * size * 2);
	return otherCenter;// + getGridPointCenterDisplacement(currentGridPos + dir);
}

//y = m*x + b;
struct EqualDistanceLineBetweenCenters
{
	float m;
	float b;
};

vec4 getPosition()
{
	vec4 pos = interpolate(tcPos[0], tcPos[1], tcPos[2], tcPos[3]);
	int x = int(gl_TessCoord.x * 2);
	int y = int(gl_TessCoord.y * 2);
	
		
	int arrayPos = x + y * 3;
	arrayPos -= int(step(4.5, float(arrayPos))); //Fourth pos at gl_TessCoord = (0.5, 0.5) should be ignored, as it is the center;
	
	vec2 targetCenters[8] = {
		getTargetGridCenter(ivec2(-1, -1)),
		getTargetGridCenter(ivec2(-1,  0)),
		getTargetGridCenter(ivec2(-1,  1)),
		getTargetGridCenter(ivec2( 0, -1)),
		//Leave out 0,0 because it is already the center of the current gridPos
		getTargetGridCenter(ivec2( 0,  1)),
		getTargetGridCenter(ivec2( 1, -1)),
		getTargetGridCenter(ivec2( 1,  0)),
		getTargetGridCenter(ivec2( 1,  1)),
	};

	struct EqualDistanceLineBetweenCenters lines[8];
	vec2 centerBetweenCenters[8];
	vec2 myRotatedVector = vec2(0);

	//Get the lines that have the same distance from the center and every target center.
	//Get line between centers VecCC. Get exact point between centers. Rotate VecCC by 90 degree and move it onto the exact point betweeen centers.
	for(int i = 0; i < 8; i++)
	{
		vec2 vector = targetCenters[i] - center.xz;
		//The center of the current gridPos is the origin of the coord system
		centerBetweenCenters[i] = vector / 2;
		//Turn 90 deg
		vec2 rotatedVector = vec2(vector.y, -vector.x);
//		float isZero = step(abs(rotatedVector.x), 1e-3);
//		rotatedVector.x = isZero * 1e-3 * (rotatedVector.x / abs(rotatedVector.x)) + (1 - isZero) * rotatedVector.x;

		if(i == arrayPos)
		{
			myRotatedVector = rotatedVector;
		}

		lines[i].m = rotatedVector.y / rotatedVector.x;

		//y at point x is given and m is known. b unknown.
		//y = m * x + b
		//y - m * x = b
		lines[i].b = centerBetweenCenters[i].y - lines[i].m * centerBetweenCenters[i].x;
	}
	
	float nearestIntersectionDistance = 1e38;
	vec2 nearestIntersection = vec2(10);


	vec2 intersections[8] = {
		vec2(1),
		vec2(2),
		vec2(3),
		vec2(4),
		vec2(5),
		vec2(6),
		vec2(7),
		vec2(8),
	};
	int nearestIntersectionIndex = 0;

	//Look at every other line, but not the chosen line itself
	//for(int i = (arrayPos + 1) % 8; i != arrayPos; i = (i + 1) % 8)
	for(int i = 0; i < 8; i++)
	{
		if(i == arrayPos)
		{
			continue;
		}

		//(-b1 - b2) / (m2 - m1) = x
		
		float areParallel = step(abs(lines[arrayPos].m - lines[i].m), 1e-3);
		float gradient = areParallel * (lines[arrayPos].m + 10) + (1 - areParallel) * lines[arrayPos].m; //Surpress division by zero

		
        float m1 = gradient;
        float b1 = lines[arrayPos].b;

//		float m1 = 0;
//		float b1 = 0;

        float m2 = lines[i].m;
        float b2 = lines[i].b;

        float xIntersect = (b2 - b1) / (m1 - m2);
        float yIntersect = m2 * xIntersect + b2;

		intersections[i] = areParallel * (center.xz + vec2(1e30)) + (1 - areParallel) * vec2(xIntersect, yIntersect); //If parallel the intersection is pretty far away ^^
		

		//Squared length of intersection, because center of coord is center of gridPoint only the length matters
		float dist = dot(intersections[i], intersections[i]);


		if(dist < nearestIntersectionDistance)
		{
			nearestIntersectionDistance = dist;
			nearestIntersection = intersections[i];
			nearestIntersectionIndex = i;
		}
	}
	
	
	vec4 newPos = pos;

	if(x == 1 && y == 2)
	{
		newPos = vec4(nearestIntersection.x, 0, nearestIntersection.y, 0) + center;
		newPos = vec4(lines[int(floor(iGlobalTime)) % 8].m, 0, 0, 0) + center;
//		newPos = vec4(intersections[int(floor(iGlobalTime)) % 8].x, 0, intersections[int(floor(iGlobalTime)) % 8].y, 0) + center;
//		newPos = vec4(centerBetweenCenters[arrayPos].x, 0, centerBetweenCenters[arrayPos].y, 0) + center + (vec4(myRotatedVector.x, 0, myRotatedVector.y, 0) * sin(iGlobalTime));
//		newPos = vec4(intersections[arrayPos + 1].x, 0, intersections[arrayPos + 1].y, 0) + center;
//		newPos = vec4(-4, 0, 0, 0) + center;
	}

	return isCenter * pos + (1 - isCenter) * newPos;
}


void main() 
{
	vec2 texCoord = interpolate(tcTexCoord[0], tcTexCoord[1], tcTexCoord[2], tcTexCoord[3]);

	currentGridPos = getCurrentGridPos();
	center = getCenter(tcPos[0], tcPos[1], tcPos[2], tcPos[3]);

	isCenter = step(0.45, gl_TessCoord.x) * step(gl_TessCoord.x, 0.55) * step(0.45, gl_TessCoord.y) * step(gl_TessCoord.y, 0.55);

	vec2 centerDisplacement = vec2(0);//getGridPointCenterDisplacement(currentGridPos);
	vec4 pos = getPosition();
	pos = (1 - isCenter) * pos + isCenter * (pos + vec4(centerDisplacement.x, 0, centerDisplacement.y, 0));
	
	int x = int(gl_TessCoord.x * 2);
	int y = int(gl_TessCoord.y * 2);
	int arrayPos = x + y * 3;
	//arrayPos -= int(step(4.5, float(arrayPos)));

	pos.y = (currentGridPos.x + currentGridPos.y * 3) / 2;

	o.normal = vec3(0, 1, 0);
	o.position = pos.xyz;

	gl_Position = camera * pos;
	o.instanceID = instanceID;
	o.depth = (camera * pos).z;
}
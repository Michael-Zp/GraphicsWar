#version 430 core

uniform mat4 camera;
uniform float time;

in vec3 position;
in vec3 normal;
in mat4 transform;

const float PI = 3.141592;

out Data {
	vec3 normal;
	vec3 position;
	float depth;
	vec2 uv;
	mat4 transform;
	mat3 tbn;
} o;

float sin2(float x)
{
	float value = sin(x);
	return value * value;
}

float cos2(float x)
{
	float value = cos(x);
	return value * value;
}

void main() 
{
	o.normal = normal;
	o.uv = vec2(0);
	o.transform = transform;
	o.tbn = mat3(1, 0, 0, 0, 1, 0, 0, 0, 1);
	
	float vertID = mod(gl_VertexID, 3);

	float isVertexZero = step(-0.5f, vertID) * step(vertID, 0.5f);
	float isVertexOne  = step( 0.5f, vertID) * step(vertID, 1.5f);
	float isVertexTwo  = step( 1.5f, vertID) * step(vertID, 2.5f);

	vec3 wobblePosition = isVertexZero * position * (0.5+0.5*(0.5+0.5*sin(time*6)));
	wobblePosition += isVertexOne * position * (0.5+0.5*(0.5+0.5*sin(time*6 + 2.0f / 3.0f * PI)));
	wobblePosition += isVertexTwo * position * (0.5+0.5*(0.5+0.5*sin(time*6 + 4.0f / 3.0f * PI)));

	
	o.position = (transform * vec4(wobblePosition, 1.0)).xyz;
	
	vec4 outPos = camera * vec4(o.position, 1.0);

	gl_Position = outPos;
	o.depth = outPos.z;
}
#version 420 core
uniform int instanceSqrt = 2;

out Data
{
	flat int instanceID;
	vec2 texCoord;
} o;

void main() 
{
	const float size = 1;
	const vec4 vertices[4] = vec4[4] (
		vec4(-size, -size, 0, 0),
		vec4( -size, size, 0, 1),
		vec4( size,  size, 1, 1),
		vec4(size,  -size, 1, 0)
	);
	
	float x = gl_InstanceID % instanceSqrt;
	float y = floor(gl_InstanceID / instanceSqrt);
	o.instanceID = gl_InstanceID;

	vec2 pos = vertices[gl_VertexID].xy + vec2(size * 2) * vec2(x - (instanceSqrt / 2), y - (instanceSqrt / 2));

	o.texCoord = vec2((pos.x + size * instanceSqrt) / (size * 2 * instanceSqrt), (pos.y + size * instanceSqrt) / (size * 2 * instanceSqrt));


	gl_Position = vec4(pos.x, 0, pos.y, 1.0);
}
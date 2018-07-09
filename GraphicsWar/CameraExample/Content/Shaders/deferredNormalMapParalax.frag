#version 430 core

//#include "lightCalculation.glsl"
float lambert(vec3 n, vec3 l)
{
	return max(0, dot(n, l));
}

float specular(vec3 n, vec3 l, vec3 v, float shininess)
{
	vec3 r = reflect(-l, n);
	float iluminated = step(dot(n, l),0);
	return pow(max(0, dot(r, v)), shininess)*iluminated;
}

vec4 calculateLight(vec3 materialColor, vec3 lightColor, vec3 ambientLightColor, vec3 lightDirection, vec3 viewDirection, vec3 normal)
{
	vec3 ambient = ambientLightColor * materialColor;
	vec3 diffuse = materialColor * lightColor * lambert(normal, -lightDirection);
	vec3 specular = lightColor * specular(normal, lightDirection, viewDirection, 100);

	return vec4(ambient + diffuse + specular, 1.0);
}
//endinclude

uniform vec3 camPos;
uniform sampler2D normalMap;
uniform sampler2D heightMap;

in Data {
	vec3 normal;
	vec3 position;
	float depth;
	vec2 uv;
	mat4 transform;
	vec3 tangent;
	vec3 bitangent;
} i;

out vec4 color;
out vec3 normal;
out float depth;
out vec3 position;

void main() 
{
	vec4 materialColor = vec4(0.8,0.8,0.8,1);
	vec3 t = normalize(i.tangent);
	vec3 b = normalize(i.bitangent);
	vec3 n = normalize(i.normal);

	mat3 tbn = mat3(t.x, t.y, t.z, b.x, b.y, b.z, n.x, n.y, n.z);


	mat3 tempInverse = inverse(tbn);
	mat4 inverseTbn = mat4(tempInverse[0].x, tempInverse[1].y, tempInverse[2].z, 0, tempInverse[0].x, tempInverse[1].y, tempInverse[2].z, 0, tempInverse[0].x, tempInverse[1].y, tempInverse[2].z, 0, 0, 0, 0, 0);
	mat4 inverseTransform = inverse(i.transform);

	vec4 eyeDirection = vec4(normalize(camPos - i.position), 0);

	vec2 viewDirectionInTangent = normalize(inverseTbn * inverseTransform * eyeDirection).xy;

	float height = texture2D(heightMap, i.uv).x * 2 - 1;

	float hn = height * 0.01 - 0.5 * 0.01;

	vec2 tn = i.uv + vec2(hn * viewDirectionInTangent);
	
	vec3 norm = normalize(texture2D(normalMap, tn).xyz);
	
	norm = norm * tbn;

	norm = (i.transform * vec4(norm, 0.0)).xyz;

	/*
	vec3 materialColor = vec3(1.0);
	vec3 lightColor = vec3(1.0);
	vec3 ambientLightColor = vec3(0.15);
	vec3 lightDirection = normalize(vec3(0,-1.0,0));
	vec3 viewDirection = normalize(i.position - camPos);

	color = calculateLight(materialColor, lightColor, ambientLightColor, lightDirection, viewDirection, norm);
	*/

	color = materialColor;
	//color = vec4(inverseTbn[0].xyz, 1);
	normal = normalize(norm);
	depth = i.depth;
	position = i.position;

}
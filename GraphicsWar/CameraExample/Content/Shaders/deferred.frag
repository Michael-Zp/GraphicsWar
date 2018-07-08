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

in vec3 n;
in float d;
in vec3 pos;

out vec4 color;
out vec3 normal;
out float depth;
out vec3 position;

void main() 
{
	//vec4 materialColor = vec4(0.8,0.8,0.8,1);
	
	
	
	vec3 materialColor = vec3(1.0);
	vec3 lightColor = vec3(1.0);
	vec3 ambientLightColor = vec3(0.15);
	vec3 lightDirection = normalize(vec3(0,-1.0,0));
	vec3 viewDirection = normalize(pos - camPos);
	vec3 norm = normalize(n);

	color = calculateLight(materialColor, lightColor, ambientLightColor, lightDirection, viewDirection, norm);


	//color = materialColor;
	color = vec4(n, 1.0);


	normal = normalize(n);
	depth = d;
	position = pos, 0.0;

}
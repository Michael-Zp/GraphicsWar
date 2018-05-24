#version 430 core

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

uniform vec3 camPos;

in vec3 n;
in vec3 pos;

out vec4 color;

void main() 
{
	vec3 materialColor = vec3(0.8);
	vec3 lightColor = vec3(1.0);
	vec3 ambientLightColor = vec3(0.15);
	vec3 lightDirection = vec3(0.0,-1.0,0.0);
	vec3 viewDirection = normalize(pos - camPos);
	vec3 normal = normalize(n);

	color = calculateLight(materialColor, lightColor, ambientLightColor, lightDirection, viewDirection, normal);
}
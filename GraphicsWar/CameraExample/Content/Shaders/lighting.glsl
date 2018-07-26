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
uniform vec3 ambientColor;
uniform sampler2D normals;
uniform sampler2D materialColor;
uniform sampler2D shadowSurface;
uniform sampler2D position;

struct Light
{
	vec3 lightPos;
	float align1;
	vec3 lightDir;
	float align2;
	vec3 lightCol;
	float lightIntense;
};

layout(std430) buffer Lights
{
	Light light[];
};

in vec2 uv;

out vec4 color;


void main() 
{
	vec3 matColor = texture2D(materialColor, uv).rgb;
	vec3 viewDirection = normalize(texture2D(position, uv).xyz - camPos);
	vec4 notNormNormal = texture2D(normals, uv);
	vec3 norm = normalize(notNormNormal.xyz);

	color = vec4(0);

	for(int i = 0; i < 8; i++) 
	{
		color += calculateLight(matColor, light[i].lightCol, ambientColor, light[i].lightDir, viewDirection, norm) * light[i].lightIntense;
	}

	color *= step(0.5, length(notNormNormal.xyz));

	//color *= texture2D(shadowSurface, uv).x;
}
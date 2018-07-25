uniform sampler2D image1;
uniform sampler2D image2;
uniform float factor;

in vec2 uv;

void main() 
{
	float alphaFactor = factor * texture2D(image2, uv).a;
	gl_FragColor = mix(texture2D(image1, uv), texture2D(image2, uv), alphaFactor);
}
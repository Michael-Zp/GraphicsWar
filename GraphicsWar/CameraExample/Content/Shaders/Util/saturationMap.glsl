uniform sampler2D saturation;
uniform sampler2D image;

in vec2 uv;

void main() 
{
	gl_FragColor = vec4((texture2D(image, uv) * texture2D(saturation, uv).x).xyz,1);
}
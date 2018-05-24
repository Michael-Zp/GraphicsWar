uniform sampler2D color;
uniform sampler2D normal;
uniform sampler2D depth;

in vec2 uv;

void main() {
	vec3 color = texture(color, uv).rgb;
	
if(uv.x>0.3333){
color = texture(normal, uv).rgb;
}

if(uv.x>0.6666){
color = texture(depth, uv).rgb;
}
		
	gl_FragColor = vec4(color, 1.0);
}
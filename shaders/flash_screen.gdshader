shader_type canvas_item;

uniform vec3 color : source_color;
uniform float fac;
uniform float outerRadius;

void fragment() {
	float x = abs(UV.x - 0.5)*2.0;
	float y = abs(UV.y - 0.5)*2.0;
	float v = (sqrt((x*x)+(y*y))/outerRadius);
	COLOR.rgb = color;
	COLOR.a = mix(0, 1, fac) * v;
	
}

shader_type canvas_item;
render_mode blend_add;

uniform float outerRadius = 2;

void fragment() {
	float x = abs(UV.x - 0.5)*2.0;
	float y = abs(UV.y - 0.5)*2.0;
	float v = (sqrt((x*x)+(y*y))/outerRadius);
	COLOR.a *= v;
}

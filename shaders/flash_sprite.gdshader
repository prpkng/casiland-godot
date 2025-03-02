shader_type canvas_item;

uniform bool flash;
uniform float force : hint_range(0.0, 1.0, 0.05) = .9;

void fragment() {
	COLOR.rgb = mix(COLOR.rgb, vec3(force), step(0.5, float(flash)) * force);
}

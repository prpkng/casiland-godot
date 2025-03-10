shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest, repeat_disable;
uniform float zoom = 1;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = (SCREEN_UV - 0.5) / zoom + 0.5;
	COLOR = texture(screen_texture, uv);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}

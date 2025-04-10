shader_type canvas_item;

uniform int color_count = 5;
uniform int pallete_count = 6;
instance uniform int pallete_index = 0 ;
uniform sampler2D pallete_sprite : filter_nearest;

instance uniform bool flash;
uniform float flash_force : hint_range(0.0, 1.0, 0.05) = .9;

void fragment() {
	vec4 clr = texture(TEXTURE, UV);

	float idx = floor(clr.r * float(color_count));

	vec3 pallete_clr = texture(pallete_sprite, vec2(idx / float(color_count), float(pallete_index) / float(pallete_count))).rgb;

	COLOR.rgb = pallete_clr;
	
//	Flash sprite
	COLOR.rgb += vec3(flash_force) * float(flash);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}

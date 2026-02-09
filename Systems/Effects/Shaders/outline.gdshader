shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
instance uniform bool outline_enabled = false;
uniform float line_thickness : hint_range(0, 10) = 1.0;
uniform vec4 line_color : source_color = vec4(1, 1, 1, 1);

const vec2 OFFSETS[4] = {
	vec2(-1, 0), vec2(0, -1), vec2(0, 1), vec2(1, 0)
};

void fragment() {
	vec4 c = textureLod(screen_texture, SCREEN_UV, 0.0);
	
	if (c.a > 0.0001) {
		c.rgb /= c.a;
	}

	if (!outline_enabled) {
		COLOR = c;
	}
	else 
	{
		vec2 size = SCREEN_PIXEL_SIZE * line_thickness;
		float outline = 0.0;
		
		for (int i = 0; i < 4; i++) {
			outline += texture(screen_texture, SCREEN_UV + size * OFFSETS[i]).a;
		}
		outline = min(outline, 1.0);
		
		COLOR = mix(c, line_color, outline - c.a);
	}
}
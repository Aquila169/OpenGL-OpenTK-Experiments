#version 140

out vec4 out_frag_color;

in vec3 pos;

void main(void)
{
	out_frag_color = vec4(pos, 1.0);
}
#version 460

in vec4 vPositionWeights;
in vec4 vTexCoords;
in vec4 vOffsetsPositionSpeed;
in vec4 vRangesPositionSpeed;

out vec4 offset;
out vec4 range;
out vec2 morphState;
out vec2 texCoords;

void main()
{
    offset = vOffsetsPositionSpeed;
    range = vRangesPositionSpeed;
    morphState = vec2(vPositionWeights.zw);
    texCoords = vTexCoords.xy;
	
    vec4 pos = vec4(vPositionWeights.xy, 1.0, 1.0);
    //pos.y = -vPositionWeights.y;
    gl_Position = pos;
}

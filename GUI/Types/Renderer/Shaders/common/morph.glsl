#version 460

#define F_MORPH_SUPPORTED 0

#if F_MORPH_SUPPORTED == 1
    uniform sampler2D morphCompositeTexture;
    uniform vec2 morphCompositeTextureSize;
    uniform int morphVertexIdOffset;

    vec2 getMorphUV()
    {
        int vertexId = gl_VertexID + morphVertexIdOffset;
        return vec2(
            (1.5 + mod(vertexId, morphCompositeTextureSize.x)) / 2048.0,
            1 - (1.5 + floor(vertexId / morphCompositeTextureSize.x)) / 2048.0
        );
    }
    
    vec3 getMorphPositionOffset()
    {
        return texture(morphCompositeTexture, getMorphUV()).xyz;
    }

    vec3 getMorphNormalOffset()
    {
        return texture(morphCompositeTexture, getMorphUV() + vec2(0.5, 0.0)).xyz;
    }
#else
    vec3 getMorphPositionOffset()
    {
        return vec3(0, 0, 0);
    }

    vec3 getMorphNormalOffset()
    {
        return vec3(0, 0, 0);
    }
#endif

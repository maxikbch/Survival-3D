#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
    
float4x4 World; // Matriz de mundo
float4x4 View; // Matriz de vista
float4x4 Projection; // Matriz de proyeccion
// float4x4 InverseTransposeWorld; // Usada para la iluminacion
float2 resolution;

//Los tres colores de la fuente de luz
float3 ambientColor;
float3 diffuseColor;
float3 specularColor;

//Las tres constantes del material renderizado
float KAmbient;
float KDiffuse;
float KSpecular;

float shininess;

float3 lightPosition; // Posicion de la fuente de luz
float3 eyePosition; // Camera position

//Sombras
static const float modulatedEpsilon = 0.000041200182749889791011810302734375;
static const float maxEpsilon = 0.000023200045689009130001068115234375;
float2 shadowMapSize;
float4x4 LightViewProjection;

texture shadowMap;
sampler2D shadowMapSampler =
sampler_state
{
    Texture = <shadowMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture procesedTexture;
sampler2D procesedTextureSampler = sampler_state
{
    Texture = (procesedTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture normalsTexture;
sampler2D normalsTextureSampler = sampler_state
{
    Texture = (normalsTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture borderTexture;
sampler2D borderTextureSampler = sampler_state
{
    Texture = (borderTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

//Tomada de https://gist.github.com/mattatz/86fff4b32d198d0928d0fa4ff32cf6fa
float4x4 inverse(float4x4 m)
{
    float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
    float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
    float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
    float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

    float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
    float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
    float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
    float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

    float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
    float idet = 1.0f / det;

    float4x4 ret;

    ret[0][0] = t11 * idet;
    ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
    ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
    ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

    ret[1][0] = t12 * idet;
    ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
    ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
    ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

    ret[2][0] = t13 * idet;
    ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
    ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
    ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

    ret[3][0] = t14 * idet;
    ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
    ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
    ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

    return ret;
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
    float4 LightSpacePosition : TEXCOORD3;
    float4 WorldSpacePosition : TEXCOORD4;
    float3 ViewSpaceNormal : TEXCOORD5;
    float4 Color : COLOR0;
};

struct DepthPassVertexShaderInput
{
    float4 Position : POSITION0;
};

struct DepthPassVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 ScreenSpacePosition : TEXCOORD1;
};

struct DrawRenderVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct DrawRenderVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);

    output.WorldPosition = worldPosition;

    float4x4 inverseTransposeWorld = transpose(inverse(World));
    output.Normal = mul(float4(input.Normal.xyz, 0.0), inverseTransposeWorld);
    
    float4 viewSpaceNormal = mul(float4(input.Normal.xyz, 0.0), View);
    output.ViewSpaceNormal = viewSpaceNormal.xyz;
	
    output.WorldSpacePosition = mul(input.Position, World);
    
    output.LightSpacePosition = mul(output.WorldSpacePosition, LightViewProjection);
    
    output.Color = input.Color;

    return output;
}


float4 BlingPhong(VertexShaderOutput input)
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
    float3 ambientLight = ambientColor * KAmbient;

	// Calculate the diffuse light
    float NdotL = saturate(dot(input.Normal.xyz, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;

	// Calculate the specular light
    float NdotH = dot(input.Normal.xyz, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(ambientLight + diffuseLight) * input.Color.rgb + specularLight, input.Color.a);
    return finalColor;
}

float4 Shadows(VertexShaderOutput input, float4 color)
{
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowMapTextureCoordinates = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowMapTextureCoordinates.y = 1.0f - shadowMapTextureCoordinates.y;
	
    float3 normal = normalize(input.Normal.rgb);
    float3 lightDirection = normalize(lightPosition - input.WorldSpacePosition.xyz);
    float inclinationBias = max(modulatedEpsilon * (1.0 - dot(normal, lightDirection)), maxEpsilon);
	
	// Sample and smooth the shadowmap
	// Also perform the comparison inside the loop and average the result
    float notInShadow = 0.0;
    float2 texelSize = 1.0 / shadowMapSize;
    for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        {
            float pcfDepth = tex2D(shadowMapSampler, shadowMapTextureCoordinates + float2(x, y) * texelSize).r + inclinationBias;
            notInShadow += step(lightSpacePosition.z, pcfDepth) / 9.0;
        }
	
    float4 baseColor = color;
    baseColor.rgb *= 0.5 + 0.5 * notInShadow;
    return baseColor;
}

float4 NormalShader(VertexShaderOutput input)
{
    float3 normal = normalize(input.ViewSpaceNormal);
	
    float4 baseColor;
    baseColor = float4(abs(normal.x), normal.y, abs(normal.z), 1.0);
    return baseColor;
}

float BorderShader(DrawRenderVertexShaderInput input, float2 direction)
{
    float4 baseColor = float4(1.0, 1.0, 1.0, 1.0);
    float dx = 1.0 / resolution.x * direction.x;
    float dy = 1.0 / resolution.y * direction.y;
    
    float3 center = tex2D(normalsTextureSampler, input.TextureCoordinates);
    
    //
    float3 top = tex2D(normalsTextureSampler, input.TextureCoordinates + float2(0.0, dy));
    float3 topRight = tex2D(normalsTextureSampler, input.TextureCoordinates + float2(dx, dy));
    float3 right = tex2D(normalsTextureSampler, input.TextureCoordinates + float2(dx, 0.0));
    
    float3 t = center - top;
    float3 r = center - right;
    float3 tr = center - topRight;
    
    t = abs(t);
    r = abs(r);
    tr = abs(tr);
    
    float n = 0;
    n = max(n, t.x);
    n = max(n, t.y);
    n = max(n, t.z);
    n = max(n, r.x);
    n = max(n, r.y);
    n = max(n, r.z);
    n = max(n, tr.x);
    n = max(n, tr.y);
    n = max(n, tr.z);
    
    return n;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 finalColor;
    float4 blanco = float4(1.0, 1.0, 1.0, 1.0); //un blanco que uso para testear cosas
    float4 negro = float4(0.0, 0.0, 0.0, 1.0); //un negro que uso para testear cosas
    finalColor = BlingPhong(input);
    //finalColor = blanco;
    finalColor = Shadows(input, finalColor);
    //finalColor = BorderShader(input, finalColor);
    return finalColor;
}

DepthPassVertexShaderOutput DepthVS(in DepthPassVertexShaderInput input)
{
    DepthPassVertexShaderOutput output;
    float4x4 WorldViewProjection = mul(mul(World, View), Projection);
    output.Position = mul(input.Position, WorldViewProjection);
    output.ScreenSpacePosition = mul(input.Position, WorldViewProjection);
    return output;
}

float4 DepthPS(in DepthPassVertexShaderOutput input) : COLOR
{
    float depth = input.ScreenSpacePosition.z / input.ScreenSpacePosition.w;
    return float4(depth, depth, depth, 1.0);
}

DrawRenderVertexShaderOutput DrawRenderVS(in DrawRenderVertexShaderInput input)
{
    DrawRenderVertexShaderOutput output;
    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 DrawRenderPS(in DrawRenderVertexShaderOutput input) : COLOR
{
    float4 procesedColor = tex2D(procesedTextureSampler, input.TextureCoordinates);
    float4 borderColor = tex2D(borderTextureSampler, input.TextureCoordinates);
    float4 black = float4(1.0, 1.0, 1.0, 1.0);
    borderColor = black - borderColor;
    return procesedColor - borderColor;
}

float4 DrawNormalsPS(in VertexShaderOutput input) : COLOR
{
    float4 finalColor;
    finalColor = NormalShader(input);
    return finalColor;
}

float4 DrawBordersPS(in DrawRenderVertexShaderInput input) : COLOR
{
    float4 black = float4(1.0, 1.0, 1.0, 1.0);
    float4 finalColor = black;
    float n1 = BorderShader(input, float2(1.0, 1.0));
    float n2 = BorderShader(input, float2(-1.0, -1.0));
    n2 = 0; //Comentar esto da unos bordes mas gruesos
    
    float n = max(n1, n2);
    // threshold and scale.
    n = 1.0 - clamp(clamp((n * 2.0) - 0.8, 0.0, 1.0) * 1.5, 0.0, 1.0);

    finalColor.rgb = finalColor.rgb * (0.1 + 0.9 * n);
    
    return finalColor;
}

technique BasicColorDrawing
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

technique DepthPass
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DepthVS();
        PixelShader = compile PS_SHADERMODEL DepthPS();
    }
};

technique DrawRenderTargets
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DrawRenderVS();
        PixelShader = compile PS_SHADERMODEL DrawRenderPS();
    }
};

technique DrawBorders
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL DrawRenderVS();
        PixelShader = compile PS_SHADERMODEL DrawBordersPS();
    }
};

technique DrawNormals
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DrawNormalsPS();
    }
};
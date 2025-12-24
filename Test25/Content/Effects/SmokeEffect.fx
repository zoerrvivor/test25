#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix MatrixTransform;
float Time;
float Wind;

Texture2D SpriteTexture;
sampler2D SpriteSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, MatrixTransform);
	output.Color = input.Color;
	output.TexCoord = input.TexCoord;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TexCoord;
    
    // Multi-frequency distortion for organic smoke look
    float noise = sin(uv.y * 12.0 - Time * 5.0) * 0.03;
    noise += sin(uv.x * 8.0 + Time * 3.0) * 0.02;
    
    // Apply wind shift to UV (subtle)
    uv.x += noise + (Wind * 0.0005 * (1.0 - uv.y));
    
    float4 texColor = tex2D(SpriteSampler, uv);
    
    // Soft edge mask to keep smoke "puffy"
    float dist = distance(uv, float2(0.5, 0.5));
    float edgeFade = 1.0 - smoothstep(0.35, 0.5, dist);
    
    float4 finalColor = texColor * input.Color;
    finalColor.a *= edgeFade;
    
    // Smoke coloring: dark grayish with a hint of blue
    finalColor.rgb *= float3(0.35, 0.38, 0.42); 
    
	return finalColor;
}



technique SmokeDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};


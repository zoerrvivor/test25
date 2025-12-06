#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float WaveTime;
Texture2D WaterTexture;
sampler2D WaterSampler = sampler_state
{
	Texture = <WaterTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 pos = input.Position;
    // Simple wave displacement
    // Modulate Y based on X and Time
    pos.y += sin(pos.x * 0.05 + WaveTime * 5.0) * 5.0;
    
	output.Position = mul(pos, WorldViewProjection);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Sample water texture
    float4 color = tex2D(WaterSampler, input.TexCoord);
    // Add a blue tint
	return color * float4(0.6, 0.8, 1.0, 0.8);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

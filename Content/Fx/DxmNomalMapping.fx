/*
Email: xintalalai@live.com.mx
Autor: Carlos Osnaya Medrano
Multiples luces.
Modelo de iluminación: Especular o Difusa.
Tipo de fuente: Direccional, Puntual y Spot.
Técnica empleada: PixelShader.
Material clásico.
*/

float4x4 world: World;
float4x4 view : View;
float4x4 projection : Projection;

float alfa
<
	string UIWidget = "slider";
	float UIMin = 0.0F;
	float UIMax = 1.0F;
	float UIStep = 0.05F;
	string UIName = "Solar";
> = {1.0F};

float solar
<
	string UIWidget = "slider";
	float UIMin = 1.0F;
	float UIMax = 10.0F;
	float UIStep = 0.05F;
	string UIName = "Solar";
> = {1.0F};
float ks
<
	string UIWidget = "slider";
	float UIMin = 0.0F;
	float UIMax = 10.0F;
	float UIStep = 0.05F;
	string UIName = "Coeficiente de reflexión especular";
> = {0.05F};

float n
<
	string UIWidget = "slider";
	float UIMin = 0.0F;
	float UIMax = 128.0F;
	float UIStep = 1.0F;
	string UIName = "Exponente de reflexión especular";
> = 5.0F;

float4 colorMaterialAmbiental
< string UIName = "Material ambiental";
  string UIWidget = "Color";
>  = {0.07F, 0.07F, 0.07F, 1.0F};

float4 colorMaterialDifuso
<
 string UIName = "Color Material";
 string UIWidget = "Color";
> = {0.24F ,0.34F, 0.39F, 1.0F};

float4 colorMaterialEspecular
< 
	string UIName = "Material especular";
  	string UIWidget = "Color";
> = {1.0F, 1.0F, 1.0F, 1.0F};

bool habiEspec 
<
	string UIName = "Modelo Iluminación Especular";
> = false;

texture2D texturaModelo;
sampler modeloTexturaMuestra = sampler_state
{
	Texture = <texturaModelo>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture2D NormalTexture;
sampler NormalSampler = sampler_state 
{
	Texture = <NormalTexture>;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
	AddressU = Wrap;
    AddressV = Wrap;
};

texture2D especularTextura;
sampler EspecularSampler = sampler_state
{
	Texture = <especularTextura>;
	MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
	AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderEntrada
{
	float4 Posicion : POSITION;
	float3 Normal : NORMAL; 
	float3 Tangente : TANGENT;
    float3 Binormal : BINORMAL;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderSalida
{
	float4 Posicion : POSITION;	
	float2 CoordenadaTextura : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	
	//float4 oPosition : POSITION;
	//float2 oTexCoord : TEXCOORD0;
	float2 NormalCoord : TEXCOORD3;
	//float3 LightVec : TEXCOORD4;
	float3x3 TBNMatrix : TEXCOORD4;
	//float att : TEXCOORD3;
};

struct PixelShaderEntrada
{
	float2 CoordenadaTextura : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
	
	float4 Posicion : POSITION;
	//float2 oTexCoord : TEXCOORD0;
	float2 NormalCoord : TEXCOORD3;
	//float3 LightVec : TEXCOORD4;
	float3x3 TBNMatrix : TEXCOORD4;
	//float att : TEXCOORD3;
};

struct LuzDireccional 
{
    float3 Direccion;
	float4 Color;
	int Encender;
};

struct LuzPuntual
{
	float3 Posicion;
	float4 Color;
	float Rango;
	float C1, C2, C3;
	int Atenuar;
	int Encender;
};

int numLuces = 4;

VertexShaderSalida MainVS(VertexShaderEntrada entrada)
//float3 posicion : POSITION, float3 normal : NORMAL, float2 coordenadaTextura : TEXCOORD0)
{
	VertexShaderSalida salida;
	float4x4 mvp = mul(world, mul(view, projection));
	salida.Posicion = mul(entrada.Posicion, mvp);
	salida.WorldNormal = mul(entrada.Normal, world);
	//float4 worldPosition = mul(float4(posicion, 1.0F), world);
	//salida.WorldPosition = worldPosition / worldPosition.w;
	salida.WorldPosition = mul(entrada.Posicion, world);
	salida.CoordenadaTextura = entrada.TexCoord * solar;
	salida.NormalCoord = entrada.TexCoord * solar;
	
	salida.TBNMatrix = float3x3(entrada.Tangente, entrada.Binormal , entrada.Normal); 
    //salida.lightVec = mul(TBNMatrix, light); 
	
 	return salida;
}// fin del vertexshader MainVS

float4 MainPSAmbiental(PixelShaderEntrada entrada, uniform bool habiTextura) : COLOR
{
	float4 color = colorMaterialDifuso;
	if(habiTextura)
	{
		color *=  tex2D(modeloTexturaMuestra, entrada.CoordenadaTextura);
	}
	float4 colorFinal = color * colorMaterialAmbiental;
	colorFinal.a = alfa;
	return colorFinal;
	//return colorMaterialAmbiental * tex2D(modeloTexturaMuestra, entrada.CoordenadaTextura);
}// fin del pixelshader MainPSAmbiental

// Función que calcula la atenuación de la distancia de la fuente al objeto
float AtenuarFuente(float3 worldPosicion, float3 posicionLuz, float rango,
float c1, float c2, float c3)
{
	float distancia = distance(worldPosicion, posicionLuz);
	float atenuacionD = 0;
	if(distancia <= rango)
	{
		atenuacionD = min(1 / (c1 + (c2 * distancia) + 
		(c3 * pow(distancia, 2.0F))), 1.0F);	
	}// fin del if
	
	return atenuacionD;
}// fin de la función AtenuarFuente

// Función que calcula la reflexión especular.
float4 ReflexionEspecular(float3 direccionLuz, float3 worldPosicion, 
float3 worldNormal, float4 colorLuz, float3x3 dominioTangencial, bool texturizado,
float4 colorEspecular)
{
	float3 reflexion = normalize(2 * worldNormal *
	max(dot(direccionLuz, worldNormal), 0.0F) - direccionLuz);	
	
	float3 posicionObservador = mul(-view._m30_m31_m32, transpose(view)); 
	float3 direccionCamara; // = normalize(posicionObservador - worldPosicion);
	
	if(texturizado)
	{
		posicionObservador = mul(dominioTangencial, posicionObservador);
		worldPosicion = mul(dominioTangencial, worldPosicion);
		direccionCamara = normalize(posicionObservador - worldPosicion);
		//direccionCamara = normalize(mul(dominioTangencial, direccionCamara));
		colorMaterialEspecular = colorEspecular;
		//reflexion = normalize(mul(dominioTangencial, reflexion));
	}
	else
	{
		direccionCamara = normalize(posicionObservador - worldPosicion);
	}
	//float3 direccionCamara = normalize(posicionCamara - worldPosicion);
	float reflexionEspecular = pow(max(dot(reflexion, direccionCamara), 0.0F), n);
	
	//reflexionEspecular *= tex2D(EspecularSampler, entrada.CoordenadaTextura);
	
	return (ks * reflexionEspecular) * (colorLuz * colorMaterialEspecular);
	//return (ks * reflexionEspecular) * colorLuz;
	//return (ks * reflexionEspecular) * colorMaterialEspecular;
}// fin de la función ReflexionEspecular

float4 Phong(float3 direccionLuz, float3 worldPosicion, float3 worldNormal, float4 colorLuz,
float4 colorMater, float3x3 dominioTangencial, bool texturizado,
float4 colorEspecular)
{
	//if(texturizado)
	//	direccionLuz = normalize(mul(dominioTangencial, direccionLuz));
	
	float reflexionDifusa = max(dot(direccionLuz, worldNormal), 0.0F);
	//float4 phong = reflexionDifusa * colorLuz;
	float4 phong = reflexionDifusa * colorLuz * colorMater;	
	// activación de la reflexión especular
	if(habiEspec)
	{
		phong += ReflexionEspecular(direccionLuz, worldPosicion, worldNormal, 
		colorLuz, dominioTangencial,
		texturizado, colorEspecular)
		* colorMater
		* reflexionDifusa;
	}// fin del if
	
	return phong;
}// fin de la función Phong

// Función que calcula la luz tipo direccional
float4 FuenteDireccional(LuzDireccional luz, float3 worldPosicion, float3 worldNormal,
float4 colorMater, float3x3 dominioTangencial, bool texturizado, float4 colorEspecular)
{	
	float3 direccion;
	if(texturizado)
		direccion = -normalize(mul(dominioTangencial, luz.Direccion));
	else
		direccion = -normalize(luz.Direccion);
	//return Phong(-normalize(luz.Direccion), // dirección de luz
	return Phong(direccion, // dirección de luz
	worldPosicion,
	worldNormal, luz.Color,
	colorMater, dominioTangencial, texturizado,
	colorEspecular);
}// fin de la función FuenteDireccional

float4 FuentePuntual(LuzPuntual luz, float3 worldPosicion, float3 worldNormal,
float4 colorMater, float3x3 dominioTangencial, bool texturizado, float4 colorEspecular)
{
	float3 direccion;
	
	if(texturizado)
	{
		//luz.Posicion = normalize(mul(dominioTangencial, luz.Posicion));
		direccion = normalize(mul(dominioTangencial, luz.Posicion - worldPosicion));
	}
	else
	{
		direccion = normalize(luz.Posicion - worldPosicion);
	}
	
	//float4 color = Phong(normalize(luz.Posicion - worldPosicion),	// dirección de luz
	float4 color = Phong(direccion,	// dirección de luz
	worldPosicion,
	worldNormal, luz.Color,
	colorMater, dominioTangencial, texturizado,
	colorEspecular);
	// atenuar luz
	if(luz.Atenuar != 0)
	{
		color *= AtenuarFuente(worldPosicion, luz.Posicion, luz.Rango,
		luz.C1, luz.C2, luz.C3);
	}// fin del if
	
	return color;
}// fin de la función FuentePuntual

LuzDireccional direccionales[6];
LuzPuntual puntuales[6];


float4 MainPS(PixelShaderEntrada entrada, uniform bool texturizado) : COLOR
{
	float4 color = 0;
	float4 colorDifuso = colorMaterialDifuso;
	float4 colorEspecular = 1;
	
	if(texturizado)
	{
		colorDifuso *= tex2D(modeloTexturaMuestra, entrada.CoordenadaTextura);
		entrada.WorldNormal = 2.0f * tex2D(NormalSampler, entrada.NormalCoord).rgb - 1.0f; 
		colorEspecular = tex2D(EspecularSampler, entrada.CoordenadaTextura);
	}// fin del if
	
	for(int i = 0; i < numLuces; i++)
	{
		if(direccionales[i].Encender != 0)
		{
			color +=  FuenteDireccional(direccionales[i], entrada.WorldPosition,
						entrada.WorldNormal,
						colorDifuso, entrada.TBNMatrix, texturizado,
						colorEspecular);
		}
		if(puntuales[i].Encender != 0)
		{
			color += FuentePuntual(puntuales[i], entrada.WorldPosition,
					entrada.WorldNormal,
					colorDifuso, entrada.TBNMatrix, texturizado,
					colorEspecular);
		}
		
	}// fin del for
	color.a = alfa;
	//1.0F;
	return color;
}// fin del pixelShader mainPS

technique Texturizado
{
	pass Ambiental 
	{/*
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;*/
		
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPSAmbiental(true);
	}
	pass MultiLuces
	{/*
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;*/
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPS(true);
	}
}// fin de la técnica Texturizado

technique Material
{
	pass Ambiental 
	{	/*AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;*/
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPSAmbiental(false);
	}
	
	pass MultiLuces
	{ /*
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;*/
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPS(false);
	}
}// fin de la técnica Texturizado
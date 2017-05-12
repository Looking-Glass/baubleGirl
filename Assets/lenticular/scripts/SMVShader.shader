// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SMV/SMV Shader" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 300

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass{
		CGPROGRAM
#pragma target 3.5
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	float4 _MainTex_ST;

	v2f vert(appdata_base v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	sampler2D _MainTex;
	sampler2D _ColorTest;
	sampler2D _NumTest;
	float numViews;
	float tilesX;
	float tilesY;
	float blending;
	float tilt;
	float pitch;
	int flipX;
	int offsetX;
	int offsetY;
	float uselessP;
	float pitchOffsetX;
	int colorTest;
	int numTest;

	float4 texArr(float3 uvz) {
		//decide which section to take from based on the z.
		float z = floor(uvz.z);
		float x = fmod(z, tilesX) / tilesX;
		float y = floor(z / tilesX) / tilesY;
		x += uvz.x / tilesX;
		y += uvz.y / tilesY;
		float4 c = tex2D(_MainTex, float2(x, y));
		float4 cTest = tex2D(_ColorTest, float2(x, y));
		float4 cNumTest = tex2D(_NumTest, float2(x, y));
		c = c * (1 - colorTest) + cTest * colorTest;
		c = c * (1 - numTest) + cNumTest * numTest;
		return c;
	}

	float4 texArrSoft(float3 uvz, float softness) {
		float4 c1 = texArr(float3(uvz.xy, floor(uvz.z)));
		float4 c2 = texArr(float3(uvz.xy, floor(clamp(uvz.z + 1.0, 0, numViews - 1.0))));
		float z = fmod(uvz.z, 1);
		z = clamp(((z - (1.0 - softness)) / softness), 0, 1);
		float4 c = lerp(c1, c2, z);
		return c;
	}

	float4 frag(v2f IN) : COLOR{
		float4 rgb[3];
		float subp = 1.0 / (_ScreenParams.x * 3.0) * (1 - flipX * 2);
		float py = 1.0 / _ScreenParams.y;

		float3 nuv = float3(IN.uv.xy, 0);
		nuv.x = nuv.x * (1 - flipX) + (1 - nuv.x) * (flipX);
		nuv.x -= offsetX * subp;
		nuv.y -= offsetY * py;

		//border black
		float black = (nuv.x >= 0) * (nuv.x <= 1) * (nuv.y >= 0) * (nuv.y <= 1);

		//useless %
		//nuv.x *= 1 / (1 - uselessP);	
		//float uselessBlack = (nuv.z >= 0) * (nuv.z <= 1);

		for (int i; i < 3; i++) {
			//nuv.x += i * subp;
			nuv.z = (nuv.x + i * subp + IN.uv.y * tilt) * pitch - pitchOffsetX;
			nuv.z = fmod(nuv.z + 90, 1);
			nuv.z -= uselessP / 2;
			nuv.z *= 1 / (1 - uselessP);
			float uselessBlack = (nuv.z >= 0) * (nuv.z <= 1);
			nuv.z *= numViews;
			rgb[i] = texArr(nuv);
			rgb[i] *= uselessBlack;
		}

		return float4(rgb[0].r, rgb[1].g, rgb[2].b, 1) * black;
	}
		ENDCG
	}
	}
		FallBack Off
}
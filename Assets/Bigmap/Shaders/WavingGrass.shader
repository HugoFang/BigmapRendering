Shader "BigMap/WavingGrass" {
	Properties {
		_CameraPos("_Camera Position",Vector) = (0.0,0.0,0.0)
		_GrassRootPosition("_GrassRootPosition",Vector)= (0.0,0.0,0.0)
		_TerrainRootPosition("_TerrainRootPosition",Vector) =(0.0,0.0,0.0)
		_GrassMap("Grass Map",2D) = "white"{}
		_GrassTextures("Grass Textures",2DArray) = ""{}
		_HeightMap("Height Map",2D) = "white"{}
		_NormalMap("Normal Map",2D) = "white"{}
		_Cutoff("Tranparent Cut off",Range(0,1)) = 0.7
		_DistanceCutoff("Distance Cut off",Range(0,1)) = 0.8
		_UpperBound("Distance Upper Bound",Float) = 800
		_LowerBound("Distance Lower Bound",Float) = 200
		_TimeScale("Scale of the waving",Float) = 1.0
		_Meter("Terrain Meter",Float) = 2.0
		_HeightFactor("Height Factor",Float) = 512
		_MaxResolution("Max resolution",Float) = 1024
		_DensityFactor("Grass Density Factor",Float) = 0.25
		_CellSize("Terrain CellSize",Float) = 2
	}
	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alphatest:_Cutoff vertex:vert nometa noforwardadd exclude_path:deferred exclude_path:prepass
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.5

		uniform float _TimeScale;
		uniform float3 _TerrainRootPosition;
		uniform sampler2D _GrassMap;
		uniform sampler2D _HeightMap;
		uniform sampler2D _NormalMap;
		uniform float _Meter;
		uniform float _MaxResolution;
		uniform float _HeightFactor;
		uniform float _DensityFactor;
		uniform float _CellSize;
		float3 _CameraPos;
		//float3 _GrassRootPosition;
		UNITY_DECLARE_TEX2DARRAY(_GrassTextures);

		uniform float _UpperBound;
		uniform float _LowerBound;
		uniform float _DistanceCutoff;

		//float _Cutoff;
		struct Input {
			float2 uv_GrassTextures;
			//float distanceFactor;
			//float fitcut;
			float grasstype;
		};

		/*struct appdata_grass
		{
			float4 vertex :	POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			fixed4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
*/
		//the composition of texcoord.xyzw means its grass type,so it max support 8 kind of grass
		//the texcoord2.xyz its the color that if it's not same to the color,it should be culled
		//the texcoord2.w is means the grass density

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float3, _GrassRootPosition)
		UNITY_INSTANCING_CBUFFER_END

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float4 offset = float4(0.0, 0.0, 0.0, 0.0);

			float3 grassRootPosition = UNITY_ACCESS_INSTANCED_PROP(_GrassRootPosition);
			float height = 0.0;
			//float3 grassRootPosition = _GrassRootPosition;
			float3 wpos = float3(v.texcoord.z,0,v.texcoord.w) + grassRootPosition;
			float tm = _MaxResolution * _Meter;
			float2 wuv = (wpos.zx - _TerrainRootPosition.zx) / tm;

			float distanceFactor = 1 - min(
				max(0.0, (length(_CameraPos.xz - wpos.xz) - _LowerBound)
					/ (_UpperBound - _LowerBound)),
				1.0);
			float3 grassfit = tex2Dlod(_GrassMap, float4(wuv.xy, 0, 0)).rgb;
			float2 vcut = 0.0;
			vcut.x = dot(grassfit, v.texcoord2.xyz) - v.texcoord2.w * _DensityFactor + 0.01;
			vcut.y = distanceFactor  - _DistanceCutoff - 0.01;
			//vcut = max(vcut, float2(0.0,0.0));
			if (vcut.x <0.0 || vcut.y <0.0)
			{
				v.vertex.xyz = 0;
			}
			else
			{

				float2 wuvl = (wpos.zx + float2(0, -_CellSize)) / tm;
				float2 wuvr = (wpos.zx + float2(0, _CellSize)) / tm;
				float2 wuvb = (wpos.zx + float2(-_CellSize, 0)) / tm;
				float2 wuvt = (wpos.zx + float2(_CellSize, 0)) / tm;

				float3 tnormal = normalize(2 * tex2Dlod(_NormalMap, float4(wuv.yx, 0.0f, 0.0f)).rgb - 1);
				height += tex2Dlod(_HeightMap, float4(wuv.yx, 0.0f, 0.0f)).r/** tnormal.y*/;
				//height += (tex2Dlod(_HeightMap, float4(wuvr.yx, 0.0f, 0.0f)).r - tex2Dlod(_HeightMap, float4(wuvl.yx, 0.0f, 0.0f)).r)* tnormal.x;
				//height += (tex2Dlod(_HeightMap, float4(wuvt.yx, 0.0f, 0.0f)).r - tex2Dlod(_HeightMap, float4(wuvb.yx, 0.0f, 0.0f)).r)* tnormal.x;
				height += tex2Dlod(_HeightMap, float4(wuvl.yx, 0.0f, 0.0f)).r* (1 - tnormal.x) + tex2Dlod(_HeightMap, float4(wuvr.yx, 0.0f, 0.0f)).r* tnormal.x;
				height += tex2Dlod(_HeightMap, float4(wuvt.yx, 0.0f, 0.0f)).r* (1 - tnormal.z) + tex2Dlod(_HeightMap, float4(wuvb.yx, 0.0f, 0.0f)).r* tnormal.z;
				height /= 3.0; 

				v.vertex.y += height * _HeightFactor;
				offset.x = sin(3.1415926 * _Time.y *
					clamp(v.texcoord.y - 0.5, 0, 1))
					* _TimeScale;
				v.vertex = v.vertex + offset;
			}
			o.uv_GrassTextures = v.texcoord.xyz;
			o.grasstype = 1 * v.texcoord3.x + 2 * v.texcoord3.y + 4 * v.texcoord3.z + 8 * v.texcoord3.w;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_GrassTextures, float3(IN.uv_GrassTextures, IN.grasstype));
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}

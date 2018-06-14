Shader "BigMap/BasicTerrain" {
	Properties {

		_Color ("Color", Color) = (1,1,1,1)
		_HeightMap("Height Map",2D) = "white"{}
		_HeightFactor("The Height Factor",Float) = 1000
		_TerrainRootPosition("The Root Position Of the Terrain",Vector) = (0,0,0,0)
		_ChunkRootPosition("The Root Position Of The Chunk",Vector) =  (0,0,0,0)
		_CameraPos("Test Camera Pos",Vector) = (0,0,0,0)
		_TotalResolution("terrain total resolution",Float) = 1024
		_Meter("How many meter between pixels in Map",Float) = 2
		_CellSize("How many meter between two vertex of a chunk",Float) = 2
		//Describe the LOD RelationShip Between Chunk and its neighbour
		_NeighbourVector("The NeighBour LOD Status",Vector) = (0,0,0,0)
		_BumpFactor("Normal Factor",Float) = 1.0
		_Shininess("Shininess of Specular",Range(0,1)) = 0.5 

		// _DisctanceFactor("Disctance Factor",Range(0,1)) = 0.0
		_UpperBound("The Distance Gates Upper Bound",Float) = 1200
		_LowerBound("The Distance Gates Lower Bound",Float) = 400
		//_AverageHeight("The Average Height Of The Node",Float) = 100

		_DetailMap ("Detial Map", 2D) = "white" {}
		_DetailMapRepeatSize("Detail Map RepeatSize",Float) = 128


		//Here the _DetailTex1 is the background color
		//That means the _DetailTex1 is the default color of the terrain
		_DetailTex1("Detail Tex 1",2D) = "white" {}
		_DetailNormal1("Detail Normal 1",2D) = "white"{}
		_DetailTex2("Detail Tex 2",2D) = "white" {}
		_DetailNormal2("Detail Normal 2",2D) = "white"{}
		_DetailTex3("Detail Tex 3",2D) = "white" {}
		_DetailNormal3("Detail Normal 3",2D) = "white"{}
		_DetailTex4("Detail Tex 4",2D) = "white" {}
		_DetailNormal4("Detail Normal 4",2D) = "white"{}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		// Use Lambert lighting model and use shadow caster pass no deffered and meta pass is needed
		#pragma surface surf BlinnPhong addshadow nometa noforwardadd vertex:vert exclude_path:deferred
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		uniform sampler2D _HeightMap;
		uniform float _TotalResolution;
		uniform float _Meter;
		uniform float3 _TerrainRootPosition;

		float4 _NeighbourVector;
		float3 _ChunkRootPosition;
		float3 _CameraPos;
		float _UpperBound;
		float _LowerBound;
		float _CellSize;
		float _BumpFactor;
		float _Shininess;
		

		uniform float _HeightFactor;
		//float AverageHeight;
		uniform float _DetailMapRepeatSize;
		uniform sampler2D _DetailMap;

		uniform sampler2D _DetailTex1;
		uniform sampler2D _DetailTex2;
		uniform sampler2D _DetailTex3;
		uniform sampler2D _DetailTex4;

		uniform sampler2D _DetailNormal1;
		uniform sampler2D _DetailNormal2;
		uniform sampler2D _DetailNormal3;
		uniform sampler2D _DetailNormal4;

		//struct appdata_full
		//{
		//	float4 vertex :	POSITION;
		//	float4 tangent : TANGENT;
		//	float3 normal : NORMAL;
		//	float4 texcoord : TEXCOORD0;
		//	float4 texcoord1 : TEXCOORD1;
		//	float4 texcoord2 : TEXCOORD2;
		//	float4 texcoord3 : TEXCOORD3;
		//	fixed4 color : COLOR;
		//	UNITY_VERTEX_INPUT_INSTANCE_ID
		//};
		//When texcoord2.x is 1 that means the vertex is a border point
		//When texcoord2.y is 1 that means the vertex is a R border point
		//When texcoord2.z is 1 that means the vertex is a T border point
		//In texcoord3,
		//texcoord3.x is 1 means it has to move to left
		//texcoord3.y is 1 means it has to move to top
		struct Input {

			float2 wuv : TEXCOORD0;
			float3 worldPos :TEXCOORD1;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		float3 BlendColor(float3 src, float3 dst,float srcAlpha)
		{
			return srcAlpha * src + (1 - srcAlpha) * dst;
		}
		fixed3 expandNormalRGB(fixed3 packednormal)
		{
			return (packednormal.xyz - 0.5) * 2;
		}
		fixed3 expandNormal(fixed4 packednormal)
		{
			fixed3 normal;
			normal.xy = packednormal.wy * 2 - 1;
			normal.z = sqrt(1 - normal.x *normal.x - normal.y*normal.y);
			return normal;
		}

		void vert(inout appdata_full v, out Input o)
		{
			//Here we need to modify the v.normal v.vertex,
			//v.tangent can be ignore
			UNITY_INITIALIZE_OUTPUT(Input, o);
			//Get the wposition;
			float3 wpos = v.vertex.xyz + _ChunkRootPosition;

			//The wpos need to calculate the offset caused by distance factor
			//First the wpos.xz need to be move to the right place considering the _NeighbourVector,
			//texcoord2 and _DistanceFactor
			//texcoord2.x means it's a border point, texcoord2.y and texcoord2.z combine it's direction
			int4 neighbourVector = int4(int(_NeighbourVector.x), int(_NeighbourVector.y), int(_NeighbourVector.z), int(_NeighbourVector.w));
			int3 border = int3(int(v.texcoord2.x),int(v.texcoord2.y), int(v.texcoord2.z));
			//Make use of neighbourVector and border to determin is the vertex a border point and
			//Should it move to de save point

			float offsetFactor = border.x &
				((neighbourVector.x & ~border.y & ~border.z)|
				(neighbourVector.y & ~border.y & border.z) |
				(neighbourVector.z & border.y & ~border.z) |
				(neighbourVector.w & border.y & border.z));

			float distanceFactor = min(max(0.0,(length(_CameraPos.xz - wpos.xz)-_LowerBound)/ (_UpperBound - _LowerBound)),1.0);
			offsetFactor = max(offsetFactor, distanceFactor);

			float2 offset = offsetFactor * _CellSize * v.texcoord3.xy;
			wpos.xz += offset;

			//Calculate the uv of all the maps and get the Y value in world position
			float2 wuv = (wpos.zx - _TerrainRootPosition.zx)
				/(_TotalResolution * _Meter);

			wpos.y += tex2Dlod(_HeightMap,float4(wuv,0.0f,0.0f)).r * _HeightFactor;
			v.vertex.xyz = wpos - _ChunkRootPosition;

			float2 tex = wpos.zx / _DetailMapRepeatSize;

			o.wuv = wuv;
			o.worldPos = wpos;
		}
		//Cause we use Lambert Lighting model so the output is SurfaceOutput
		void surf (Input IN, inout SurfaceOutput o) {

			float4 d = tex2D (_DetailMap, IN.wuv);
			float weight = dot(d, float4(1, 1, 1, 1));
			d /= (weight + 1e-3f);

			float2 uv = IN.worldPos.zx /  _DetailMapRepeatSize;

			float4 detail_tex1 = tex2D(_DetailTex1, uv);
			float4 detail_tex2 = tex2D(_DetailTex2, uv);
			float4 detail_tex3 = tex2D(_DetailTex3, uv);
			float4 detail_tex4 = tex2D(_DetailTex4, uv);


			float4 c = detail_tex1 * d.r + detail_tex2 * d.g
				+ detail_tex3 * d.b + detail_tex4 * d.a;

			float4 nrm = 0.0f;

			nrm += d.r * tex2D(_DetailNormal1, uv);
			nrm += d.g * tex2D(_DetailNormal2, uv);
			nrm += d.b * tex2D(_DetailNormal3, uv);
			nrm += d.a * tex2D(_DetailNormal4, uv);
			
			float3 normal = UnpackNormal(nrm);

			o.Normal = normal;
			o.Alpha = weight;
			o.Albedo = c.rgb;
			o.Gloss = c.a;
			o.Specular = _Shininess;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

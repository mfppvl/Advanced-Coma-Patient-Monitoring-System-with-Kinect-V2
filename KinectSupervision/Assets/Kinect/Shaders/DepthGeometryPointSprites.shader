Shader "Custom/DepthGeometryPointSprites" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_WorldScale ("WorldScale", Float) = 1000.0
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM
				#pragma target 5.0
				#include "UnityCG.cginc" 
				#include "KinectCommon.cginc"
				#pragma vertex VS_Empty
				#pragma geometry GS_Main			
				#pragma fragment FS_Passthrough				

				float _WorldScale;
				Texture2D _MainTex;

				[maxvertexcount(4)]
				void GS_Main(point EMPTY_INPUT p[1], uint primID : SV_PrimitiveID, inout TriangleStream<POSCOLOR_INPUT> triStream)
				{
					POSCOLOR_INPUT output;							
				
				    int3 textureCoordinates = int3(primID % DepthWidth, primID / DepthWidth, 0);				
										
					float depth = DepthFromPacked4444(_MainTex.Load(textureCoordinates));
					
					// don't output quads for pixels with invalid depth data
					if (depth < MinDepthMM || depth > MaxDepthMM)
					{
						return;
					}					
					
					// color based on depth
					output.color.rgb = 1.0 - ((depth - MinDepthMM) / (MaxDepthMM - MinDepthMM));
					output.color.a = 1.0;					
					
					// convert to meters and scale to world				
					float worldScaledDepth = depth * MillimetersToMetersScale * _WorldScale;

					// Invert Y (0 Y is the top of the texture, but Y increases in the 3D world as you go upwards)				
					float4 worldPos = float4(textureCoordinates.x, DepthHeight - textureCoordinates.y, worldScaledDepth, 1.0);
					
					// Center coordinates such that 0,0 is the center in the world					
					worldPos.xy -= DepthHalfWidthHeight;					
					
					// Data is projected onto the 2D sensor, but knowing the depth and field of view we can reconstruct world space position
					worldPos.xy *= XYSpread * worldScaledDepth;					
					
					float4 viewPos = mul(UNITY_MATRIX_V, worldPos);

					for (int i = 0; i < 4; ++i)
					{
						// expand vertices in view space so that they're always the same size no matter what direction you're looking at them from
						float4 viewPosExpanded = viewPos + (quadOffsets[i] * XYSpread * worldScaledDepth);
						output.pos = mul(UNITY_MATRIX_P, viewPosExpanded);
						triStream.Append(output);						
					}
				}
				
			ENDCG
		}
	} 
}

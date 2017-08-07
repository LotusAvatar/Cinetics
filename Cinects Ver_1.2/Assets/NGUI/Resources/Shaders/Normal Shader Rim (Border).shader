Shader "Custom/Normal Shader Rim Border"{
Properties {
//    _Color ("Main Color", Color) = (1,1,1,1)
//    _MainTex ("Diffuse", 2D) = "white" { }
	_Depth("Layer Depth", Range(0,10)) = 1
	_Amount("Size Multiplier", Range(0,10)) = 1
}

SubShader {
Tags {"Queue" = "Geometry+100" }
    
//    ZTest Always
    ZTest LEqual
    
    ZWrite On 
    
    Cull Back 

	Lighting Off
	
    Pass {
    
    
    
		
CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
//#pragma exclude_renderers gles
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0

//#pragma noforwardadd

//////#pragma only_renderers gles opengl psp2p2p2

#include "UnityCG.cginc"
float _Amount;
float _Depth;
uniform float4x4 curve_proj;
uniform float4x4 curve_param;
#define PARAMETER_VIEW_REF_PTN_FLOAT3 curve_param._m00_m01_m02
#define PARAMETER_VIEW_DOWN_VEC_FLOAT3 curve_param._m10_m11_m12
#define PARAMETER_VIEW_Z_VEC_UNIT_FLOAT3 curve_param._m20_m21_m22
#define PARAMETER_CURVATURE_FLOAT curve_param._m03
#define PARAMETER_CURVATURE_STARTING_POINT_FLOAT curve_param._m13

inline float evalEq(in float dst){
	dst -= PARAMETER_CURVATURE_STARTING_POINT_FLOAT;
	return dst*dst*(PARAMETER_CURVATURE_FLOAT)*step(0,dst);
}

inline float3 processVertexViewSpace(in float3 input){
	float3 vecDiff = input - PARAMETER_VIEW_REF_PTN_FLOAT3;
	//project over z vec
	float length = dot(vecDiff,PARAMETER_VIEW_Z_VEC_UNIT_FLOAT3);
	return input - PARAMETER_VIEW_DOWN_VEC_FLOAT3 * evalEq(abs(length)) ;
}

//float4 _Color;
//sampler2D _MainTex;
//float4 _MainTex_ST;

struct v2f {
    float4  pos : SV_POSITION;
//    float2  uv : TEXCOORD0;
};

// appdata_base
// appdata_full
v2f vert (appdata_base v) {
    v2f o;
    
    v.vertex.xyz += v.normal * _Amount;
    
   
    float3 objSpaceCameraPos = ObjSpaceViewDir(v.vertex);
    
    objSpaceCameraPos = normalize(-objSpaceCameraPos);
    
    v.vertex.xyz += objSpaceCameraPos*_Depth;
    
    //o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
    
	o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
	
	
	//o.pos.xy += v.normal.xy;
	
//	o.pos.xyz = processVertexViewSpace(o.pos.xyz);
//	
//	o.pos = mul(curve_proj, o.pos);
	
	
	
   // o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
    return o;
}

#if SHADER_API_PS4
fixed4 frag (v2f i) : SV_Target
#else
fixed4 frag (v2f i) : COLOR
#endif
{

    return fixed4(1.0,1.0,1.0,1.0);
}
ENDCG

    }
}


//Fallback "VertexLit"
} 

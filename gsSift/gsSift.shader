//update 1
Shader "gs/gsSift"
{
  SubShader
  {
    Blend SrcAlpha OneMinusSrcAlpha
    Cull Off
    Pass
    {
      CGPROGRAM
        #pragma target 5.0
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "../GS/GS_Shader.cginc"
        #include "gsSift.cginc"
        #include "gsSift.cs"
      ENDCG
    }
  }
  Fallback Off
}
#if (!gs_shader) && (!gs_compute)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using GSharp;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

[TAtt("SIFT_AliS|SIFT_AliS")] // 2020/04/06 12:04:31 AM
public class gsSIFT_AliS : GS
{
  #region Generated Code
  // <<<<< G# Libraries
  // G# Libraries >>>>>
  // <<<<< G# Enums
  // G# Enums >>>>>
  // <<<<< G# cFields
  [Header("gsSIFT_AliS Settings")] public uint2 imageSize;[HideInInspector] public uint2 _imageSize;
  public uint2 guassMatrixSize;[HideInInspector] public uint2 _guassMatrixSize;
  public uint2 nImageSize;[HideInInspector] public uint2 _nImageSize;
  public uint octaves;[HideInInspector] public uint _octaves;
  public uint layers;[HideInInspector] public uint _layers;
  public uint oTracking;[HideInInspector] public uint _oTracking;
  public uint lTracking;[HideInInspector] public uint _lTracking;
  public uint processCtrl;[HideInInspector] public uint _processCtrl;
  public bool ValuesChanged { get { return any((uint2)_imageSize != imageSize) || any((uint2)_guassMatrixSize != guassMatrixSize) || any((uint2)_nImageSize != nImageSize) || _octaves != octaves || _layers != layers || _oTracking != oTracking || _lTracking != lTracking || _processCtrl != processCtrl; } set { if (!value) { _imageSize = imageSize; _guassMatrixSize = guassMatrixSize; _nImageSize = nImageSize; _octaves = octaves; _layers = layers; _oTracking = oTracking; _lTracking = lTracking; _processCtrl = processCtrl; } else { _imageSize -= u10; } } }
  // G# cFields >>>>>
  // <<<<< G# ExecuteInEditor
  // G# ExecuteInEditor >>>>>
  // <<<<< G# G_property
  public GSIFT_AliS G { get { if (gSIFT_AliS == null) AddComputeBuffer(ref gSIFT_AliS, 1); gSIFT_AliS.GetData(); return gSIFT_AliS[0]; } set { gSIFT_AliS[0] = value; gSIFT_AliS.SetData(); } }
  // G# G_property >>>>>
  // <<<<< G# kernelWrappers
  public void Step1_ExtremaDetection() { var g = G; Gpu(gSIFT_AliS_Step1_ExtremaDetection, g.nImageSize, new { gSIFT_AliS }, new { OriginalImage }, new { BlurredImages }, new { Guassian }, new { FeaturePntIDs }); }
  // G# kernelWrappers >>>>>
  // <<<<< G# AssignEnums
  // G# AssignEnums >>>>>
  // <<<<< G# AssignConsts
  public const int drawPoint = 0, drawSphere = 1, drawLine = 2, drawArrow = 3, drawSignal = 4, drawLineSegment = 5, drawTexture2D = 6, drawWebCam = 7;
  // G# AssignConsts >>>>>
  #endregion Generated Code
  void OnGUI()
  {
    // <<<<< G# UpdateActions
    if (ValuesChanged) { OnValueChanged(); ValuesChanged = false; }
    // G# UpdateActions >>>>>
    // <<<<< G# onGui
    if (!isEditor) return;
    InitFields();
    // G# onGui >>>>>
  }
  // <<<<< G# Actions
  // G# Actions >>>>>
  public static gsSIFT_AliS This;

  //GenerateGuassianMatrix function
  private float GuassianFunction(float sigma, int row, int col) { float guass = 1 / (2 * PI * sqr(sigma)) * exp(-(sqr(col - guassMatrixSize.x / 2) + sqr(row - guassMatrixSize.y / 2)) / (2 * sqr(sigma))); return guass; }
  private float GenerateGuassianMatrix(uint oct, uint layer)
  {
    float sigma = (float)(1.6 * pow(2, oct + ((float)layer / (float)(layers - 3))));//layers-3 to make sure the continues of the Guassian pyrimid
    uint n = 0; float normalizing = 0;
    for (int j = 0; j < guassMatrixSize.y; j++) { for (int i = 0; i < guassMatrixSize.x; i++) { Guassian[n] = GuassianFunction(sigma, j, i); normalizing += Guassian[n]; n++; } }
    for (int i = 0; i < Guassian.Length; i++) { Guassian[i] = Guassian[i] / normalizing; }//normalization
    Guassian.SetData(Guassian);
    return sigma;
  }//Generate Guassian Matrix Method End
  void Awake()
  {
    This = this;
    InitFields();
  }

  public override void onLoaded()
  {
    base.onLoaded();
    imageSize = new uint2(image.width, image.height);
    guassMatrixSize = new uint2((uint)(1.6 * 6), (uint)(1.6 * 6));
    octaves = 5; layers = 8; //octaves = 0 ~ 2, layers = 0 ~ 4
    oTracking = 0; lTracking = 0; processCtrl = 0; nImageSize = imageSize;
    uint blurredSize = 0;//cpu variable
    for (int i = 0; i < octaves; i++) { double t = Math.Pow(4, i); blurredSize += (uint)(product(imageSize) * layers / t); }
    InitBuffers();
    AddComputeBufferData(ref OriginalImage, image.GetPixels32()); AddComputeBuffer(ref BlurredImages, blurredSize); AddComputeBuffer(ref FeaturePntIDs, 1); AddComputeBuffer(ref Guassian, guassMatrixSize);
    var g = G;
    //-------------------------STEP 1---------------------------------------//
    //Resize & Blurry
    g.processCtrl = 1; g.nImageSize = g.imageSize;
    for (g.oTracking = 0; g.oTracking < octaves; g.oTracking++) { for (g.lTracking = 0; g.lTracking < layers; g.lTracking++) { G = g; GenerateGuassianMatrix(g.oTracking, g.lTracking); Step1_ExtremaDetection(); } g.nImageSize = g.nImageSize / 2; }
    //doG Calculation
    g.processCtrl = 2; g.nImageSize = g.imageSize;
    for (g.oTracking = 0; g.oTracking < octaves; g.oTracking++) { for (g.lTracking = 0; g.lTracking < layers - 1; g.lTracking++) { G = g; Step1_ExtremaDetection(); } g.nImageSize = g.nImageSize / 2; }
    //Find Extrema
    g.processCtrl = 3; g.nImageSize = g.imageSize;
    for (g.oTracking = 0; g.oTracking < octaves; g.oTracking++) { for (g.lTracking = 1; g.lTracking < layers - 2; g.lTracking++) { G = g; Step1_ExtremaDetection(); } g.nImageSize = g.nImageSize / 2; }
    //-----------------------------STEP1 DONE-----------------------------//

  }
  // <<<<< G# ActionMethods
  public void OnValueChanged()
  {
    // ***************** Specify action when field value changes ***********************
  }
  // G# ActionMethods >>>>>

  void InitBuffers()
  {
    var g = G;
    g.imageSize = imageSize;
    g.guassMatrixSize = guassMatrixSize;
    g.nImageSize = nImageSize;
    g.octaves = octaves;
    g.layers = layers;
    g.oTracking = oTracking;
    g.lTracking = lTracking;
    g.processCtrl = processCtrl;
    G = g;
    // <<<<< G# InitBuffers
    //var g = G;
    //g.imageSize = imageSize;
    //g.guassMatrixSize = guassMatrixSize;
    //g.nImageSize = nImageSize;
    //g.octaves = octaves;
    //g.layers = layers;
    //g.oTracking = oTracking;
    //g.lTracking = lTracking;
    //g.processCtrl = processCtrl;
    //g.imageSize = imageSize;
    //g.guassMatrixSize = guassMatrixSize;
    //g.nImageSize = nImageSize;
    //g.octaves = octaves;
    //g.layers = layers;
    //g.oTracking = oTracking;
    //g.lTracking = lTracking;
    //g.processCtrl = processCtrl;
    //G = g;
    //AddComputeBuffer(ref OriginalImage, 1);
    //AddComputeBuffer(ref BlurredImages, 1);
    //AddComputeBuffer(ref Guassian, 1);
    //AddComputeBuffer(ref FeaturePntIDs, 1);
    // G# InitBuffers >>>>>
  }

  public Material material;
  public GameObject cam;
  protected override bool onRenderObject()
  {
    //_WorldSpaceCameraPos = (float3)mainCam.transform.position;
    // <<<<< G# RenderSetValues
    GpuSetValues(material, new { gSIFT_AliS }, new { image }, new { OriginalImage }, new { BlurredImages }, new { FeaturePntIDs }, new { _PaletteTex });
    // G# RenderSetValues >>>>>
    var g = G;
    uint N = 0;
    N += g.octaves * g.layers; //green sphere test
    //frag(vert(N, 0)); //for debugging
    return RenderQuads(material, N);
  }

#endif //!gs_compute && !gs_shader //variables in both compute shader and material shader
  //public Texture2D gQuadTexture;
  // <<<<< G# compute_or_material_shader
  public struct GSIFT_AliS
  {
    public uint2 imageSize;
    public uint2 guassMatrixSize;
    public uint2 nImageSize;
    public uint octaves;
    public uint layers;
    public uint oTracking;
    public uint lTracking;
    public uint processCtrl;
  };
  public RWStructuredBuffer<GSIFT_AliS> gSIFT_AliS;
  public RWStructuredBuffer<Color32> OriginalImage;
  public RWStructuredBuffer<Color32> BlurredImages;
  public RWStructuredBuffer<float> Guassian;
  public RWStructuredBuffer<uint> FeaturePntIDs;
  public Texture2D image;
  // G# compute_or_material_shader >>>>>
  //*********************GPU Methods*******************//
  uint GetIndex(uint octv, uint layer, uint i)
  {
    GSIFT_AliS g = G;
    uint index = 0; int t = 1;
    for (int oc = 0; oc < octv; oc++) { index += (uint)(g.layers * product(g.imageSize) / t); t = (int)(pow(4, oc + 1)); }
    index += (uint)(layer * product(g.imageSize) / t); index += i;
    return index;
  }
  Color32 GetColorFromBuffer(uint octv, uint layer, uint i) { Color32 color = BlurredImages[GetIndex(octv, layer, i)]; return color; }
  float4 ExpandedFirstOctaveColor(uint i)//Calculate octv 0, layer 0's ith pixel's color - (expand original image & smooth it first). 
  {
    GSIFT_AliS g = G;
    uint id1 = 0, id2 = 0, id3 = 0, id4 = 0;
    if (i % g.imageSize.x == g.imageSize.x - 1 && i / g.imageSize.x == g.imageSize.y - 1) { id1 = i; id2 = i - 1; id3 = i - g.imageSize.x; id4 = i - g.imageSize.x - 1; }
    else if (i % g.imageSize.x == g.imageSize.x - 1 && i / g.imageSize.x < g.imageSize.y - 1) { id1 = i; id2 = i - 1; id3 = i + g.imageSize.x; id4 = i + g.imageSize.x - 1; }
    else if (i % g.imageSize.x != g.imageSize.x - 1 && i / g.imageSize.x == g.imageSize.y - 1) { id1 = i; id2 = i + 1; id3 = i - g.imageSize.x; id4 = i - g.imageSize.x + 1; }
    else { id1 = i; id2 = i + 1; id3 = i + g.imageSize.x; id4 = i + g.imageSize.x + 1; }
    float4 c1 = c32_f4(OriginalImage[id1]), c2 = c32_f4(OriginalImage[id2]), c3 = c32_f4(OriginalImage[id3]), c4 = c32_f4(OriginalImage[id4]);
    float red = (c1.r + c2.r + c3.r + c4.r) / 4.0f, green = (c1.g + c2.g + c3.g + c4.g) / 4.0f, blue = (c1.b + c2.b + c3.b + c4.b) / 4.0f, alpha = (c1.a + c2.a + c3.a + c4.a) / 4.0f;
    float4 color = new float4(red, green, blue, alpha); return color;
  }
  float4 GetBeforeBlurPixel(uint octv, uint i)//using current octv and current i, to find the pixel from 2 times expanded original images. 
  {
    GSIFT_AliS g = G;
    uint index = i; uint2 currentSize = g.imageSize / (1 << (int)octv); uint2 currentID = i_to_id(i, currentSize);
    for (int oc = 0; oc < octv; oc++) { currentSize = currentSize * 2; index = currentID.y * currentSize.x * 2 + currentID.x * 2; currentID = i_to_id(index, currentSize); }
    //float4 color = c32_f4(OriginalImage[index]); return color;
    float4 color = ExpandedFirstOctaveColor(index); return color;
  }
  //********GPU Methods End*******//

#if (!gs_shader)
  // <<<<< G# kernels
  int kernel_gSIFT_AliS_Step1_ExtremaDetection;[numthreads(numthreads2, numthreads2, 1)]
  void gSIFT_AliS_Step1_ExtremaDetection
#if (gs_compute)
    (uint3 id : SV_DispatchThreadID)
#else
    (uint3 id)
#endif
  {
    GSIFT_AliS g = G;
    if (all(id.xy < g.nImageSize))
    {
      uint i = id_to_i(id, g.nImageSize);
      //****Change Size*** (UNECESSARY, JUST FOR TEST)// 
      if (g.processCtrl == 0)
      {
        float4 color = GetBeforeBlurPixel(g.oTracking, i); uint index0 = GetIndex(g.oTracking, 0, i);
        for (uint ly = 0; ly < g.layers; ly++) { uint index = (uint)product(g.nImageSize) * ly + index0; BlurredImages[index] = f4_c32(color); }
      }
      //******************Gassian blurry start ********************// 
      else if (g.processCtrl == 1)
      {
        float4 color = f0001; uint currentID = GetIndex(g.oTracking, g.lTracking, i);
        for (int m = 0; m < g.guassMatrixSize.y; m++)
        {
          for (int n = 0; n < g.guassMatrixSize.x; n++)
          {
            uint guaID = id_to_i(new uint2(n, m), g.guassMatrixSize);
            int2 imageID = new int2(n + id.x - ((g.guassMatrixSize.x - 1) / 2), m + id.y - ((g.guassMatrixSize.y - 1) / 2));
            if (imageID.x < 0 || imageID.y < 0 || imageID.y >= g.nImageSize.y || imageID.x >= g.nImageSize.x) { color.r += Guassian[guaID] * 0.5f; color.g += Guassian[guaID] * 0.5f; color.b += Guassian[guaID] * 0.5f; }
            else { uint imID = id_to_i(imageID, g.nImageSize); float4 colorE = GetBeforeBlurPixel(g.oTracking, imID); color.r += (Guassian[guaID] * colorE.r); color.g += (Guassian[guaID] * colorE.g); color.b += (Guassian[guaID] * colorE.b); }
          }
        }
        BlurredImages[currentID] = f4_c32(color);
      }
      //***********doG calculation***********//
      else if (g.processCtrl == 2)
      {
        if (g.lTracking != g.layers - 1)
        {
          uint currentID = GetIndex(g.oTracking, g.lTracking, i), nextID = GetIndex(g.oTracking, (g.lTracking + 1), i);
          float4 current = c32_f4(BlurredImages[currentID]), next = c32_f4(BlurredImages[nextID]);
          float red = current.x - next.x, green = current.y - next.y, blue = current.z - next.z;
          if (red < 0) { red = 1 + red; }
          if (green < 0) { green = 1 + green; }
          if (blue < 0) { blue = 1 + blue; }
          BlurredImages[currentID] = f4_c32(new float4(red, green, blue, 1));
        }
      }
      //********Find extrema************//
      else if (g.processCtrl == 3)
      {
        int counter1 = 0, counter2 = 0; uint currentID = GetIndex(g.oTracking, g.lTracking, i);
        if (id.x > 0 && id.x < g.nImageSize.x - 1 && id.y > 0 && id.y < g.nImageSize.y - 1)
        {
          for (int k = 0; k < 3; k++)
          {
            for (int q = 0; q < 3; q++)//y
            {
              for (int p = 0; p < 3; p++)//x
              {
                int iT = k - 1; int ii = (int)(i + (q - 1) * g.nImageSize.x + (p - 1));
                uint comparingID = GetIndex(g.oTracking, (uint)(g.lTracking + iT), (uint)ii);
                float4 comparing = c32_f4(BlurredImages[comparingID]), current = c32_f4(BlurredImages[currentID]);
                float4 c = new float4(comparing.r - current.r, comparing.g - current.g, comparing.b - current.b, 1);
                if (c.r > 0 && c.g > 0 && c.b > 0) { counter1++; } else if (c.r < 0 && c.g < 0 && c.b < 0) { counter2++; }

              }//x-end
            }//y-end
          }//z-end
        }//if point i valid end
        uint featureID = GetIndex(g.oTracking, g.layers - 1, i); float4 test = c32_f4(BlurredImages[featureID]);
        if (g.lTracking == 1) { if (counter1 == 26 || counter2 == 26) { BlurredImages[featureID] = f4_c32(f1111); } else { BlurredImages[featureID] = f4_c32(f0001); } }
        else { if ((counter1 == 26 || counter2 == 26)) { BlurredImages[featureID] = f4_c32(f1111); } }


      }

    }
  }
  // G# kernels >>>>>
#endif //!gs_shader


#if (!gs_compute) //shader code

  public Texture2D _PaletteTex;

  struct v2f
  {
#if (gs_shader)
    public float4 pos : SV_POSITION, color : COLOR, ti : TEXCOORD0, tj : TEXCOORD5, tSIFTo : TEXCOORD6, tSIFTl : TEXCOORD7; public float3 normal : NORMAL, p0 : TEXCOORD1, p1 : TEXCOORD2, wPos : TEXCOORD3; public float2 uv : TEXCOORD4;
#else
    public float4 pos, color, ti, tj, tSIFTo, tSIFTl; public float3 normal, p0, p1, wPos; public float2 uv;
#endif
  };

  v2f vert_DrawPoint(float3 p, float4 color, uint i, v2f o) { o.pos = UnityObjectToClipPos(new float4(p, 1)); o.ti.z = drawPoint; o.color = color; return o; }
  v2f vert_DrawSphere(float3 p, float r, float4 color, uint i, uint j, v2f o) { float4 p4 = new float4(p, 1), quadPoint = Sphere_quadPoint(r, j); o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, p4) + quadPoint); o.wPos = p; o.uv = quadPoint.xy / r; o.normal = -f001; o.ti.z = drawSphere; o.color = color; o.ti.x = i; return o; }
  v2f vert_DrawLineArrow(float dpf, float3 p0, float3 p1, float r, float4 color, uint i, uint j, v2f o) { o.p0 = p0; o.p1 = p1; o.uv = LineArrow_uv(dpf, p0, p1, r, j); o.pos = UnityObjectToClipPos(LineArrow_p4(dpf, p0, p1, _WorldSpaceCameraPos, r, j)); o.color = color; o.ti.z = (dpf == 1 ? drawLine : drawArrow); o.ti.w = r; o.ti.x = i; return o; }
  v2f vert_DrawLine(float3 p0, float3 p1, float r, float4 color, uint i, uint j, v2f o) { o.p0 = p0; o.p1 = p1; o.uv = Line_uv(p0, p1, r, j); o.pos = UnityObjectToClipPos(LineArrow_p4(1, p0, p1, _WorldSpaceCameraPos, r, j)); o.color = color; o.ti.z = drawLine; o.ti.w = r; o.ti.x = i; return o; }
  v2f vert_DrawLineSegment(float3 p0, float3 p1, float r, float4 color, uint i, uint j, v2f o) { o = vert_DrawLineArrow(1, p0, p1, r, color, i, j, o); o.ti.z = drawLineSegment; return o; }
  v2f vert_DrawArrow(float3 p0, float3 p1, float r, float4 color, uint i, uint j, v2f o) { return vert_DrawLineArrow(3, p0, p1, r, color, i, j, o); }
  v2f vert_DrawSignal(float3 p0, float3 p1, float r, uint i, uint j, v2f o) { o.p0 = p0; o.p1 = p1; o.uv = f11 - new float2(wrapJ(j, 1), wrapJ(j, 2)); o.pos = UnityObjectToClipPos(LineArrow_p4(1, p0, p1, _WorldSpaceCameraPos, r, j)); o.ti.z = drawSignal; o.ti.w = r; o.ti.x = i; return o; }
  v2f vert_DrawBoxFrame(float3 c0, float3 c1, float lineRadius, float4 color, uint i, uint j, v2f o) { float3 p0, p1; switch (i) { case 0: p0 = c0; p1 = c0 * f110 + c1 * f001; break; case 1: p0 = c0 * f110 + c1 * f001; p1 = c0 * f100 + c1 * f011; break; case 2: p0 = c0 * f100 + c1 * f011; p1 = c0 * f101 + c1 * f010; break; case 3: p0 = c0 * f101 + c1 * f010; p1 = c0; break; case 4: p0 = c0 * f011 + c1 * f100; p1 = c0 * f010 + c1 * f101; break; case 5: p0 = c0 * f010 + c1 * f101; p1 = c1; break; case 6: p0 = c1; p1 = c0 * f001 + c1 * f110; break; case 7: p0 = c0 * f001 + c1 * f110; p1 = c0 * f011 + c1 * f100; break; case 8: p0 = c0; p1 = c0 * f011 + c1 * f100; break; case 9: p0 = c0 * f101 + c1 * f010; p1 = c0 * f001 + c1 * f110; break; case 10: p0 = c0 * f100 + c1 * f011; p1 = c1; break; default: p0 = c0 * f110 + c1 * f001; p1 = c0 * f010 + c1 * f101; break; } return vert_DrawLine(p0, p1, lineRadius, color, i, j, o); }
  v2f vert_DrawQuad(float3 p0, float3 p1, float3 p2, float3 p3, float4 color, uint i, uint j, v2f o) { float3 p = o.wPos = j % 5 == 0 ? p3 : j == 1 ? p2 : j == 4 ? p0 : p1, n = cross(p1 - p0, p0 - p3); o.color = color; o.pos = UnityObjectToClipPos(p); o.uv = new float2(wrapJ(j, 2), wrapJ(j, 4)); o.normal = n; o.ti.z = drawTexture2D; o.ti.x = i; return o; }
  v2f vert_DrawWebCam(float3 p0, float3 p1, float3 p2, float3 p3, float4 color, uint i, uint j, v2f o) { float3 p = o.wPos = j % 5 == 0 ? p3 : j == 1 ? p2 : j == 4 ? p0 : p1, n = cross(p1 - p0, p0 - p3); o.color = color; o.pos = UnityObjectToClipPos(p); o.uv = new float2(wrapJ(j, 2), wrapJ(j, 4)); o.normal = n; o.ti.z = drawWebCam; o.ti.x = i; return o; }
  v2f vert_DrawCube(float3 p, float3 r, float4 color, uint i, uint j, v2f o) { float3 p0, p1, p2, p3; switch (i % 6) { case 0: p0 = f___; p1 = f1__; p2 = f11_; p3 = f_1_; break; case 1: p0 = f1_1; p1 = f__1; p2 = f_11; p3 = f111; break; case 2: p0 = f__1; p1 = f1_1; p2 = f1__; p3 = f___; break; case 3: p0 = f_1_; p1 = f11_; p2 = f111; p3 = f_11; break; case 4: p0 = f__1; p1 = f___; p2 = f_1_; p3 = f_11; break; default: p0 = f1__; p1 = f1_1; p2 = f111; p3 = f11_; break; } return vert_DrawQuad(p0 * r + p, p1 * r + p, p2 * r + p, p3 * r + p, color, i, j, o); }
  v2f vert_DrawCube(float3 p, float r, float4 color, uint i, uint j, v2f o) { return vert_DrawCube(p, f111 * r, color, i, j, o); }
  public float4 palette(float v) { return paletteColor(_PaletteTex, v); }

  v2f vert
#if (gs_shader)
    (uint i : SV_InstanceID, uint j : SV_VertexID)
#else
    (uint i, uint j)
#endif
  {
    v2f o;
    o.color = f1001;
    o.ti = o.tj = f0000; o.ti.x = i; o.ti.y = j; o.tSIFTo = f0000; o.tSIFTl = f0000;
    o.pos = f0000; o.wPos = f000; o.uv = f00; o.normal = f001; o.p0 = o.p1 = fNegInf3;

    GSIFT_AliS g = G;
    uint n = g.octaves * g.layers;
    //if (i < (n = 1)) o = vert_DrawSphere(f000, 1, palette(0.5f), i, j, o); i -= n; // *********** green sphere test ************
    if (i < n)
    {
      float3 p0 = f000, p1 = f000, p2 = f000, p3 = f000;
      uint currentOct = (uint)(i / g.layers), currentLayer = (uint)(i % g.layers);
      o.tSIFTo.x = currentOct; o.tSIFTl.x = currentLayer;
      uint2 currentSize = g.imageSize / (1 << (int)currentOct);
      float heightControl = 0;
      for (int oc = 0; oc < currentOct; oc++) { heightControl += 0.1f * (uint)(g.imageSize.y / (1 << (int)oc)); }
      float widthControl = currentSize.x * 0.1f; float centerStd = -widthControl * g.layers * 0.5f;
      p0 = new float3(widthControl * currentLayer + centerStd, heightControl, 0);
      p1 = new float3(widthControl * (currentLayer + 1) + centerStd, heightControl, 0);
      p2 = new float3(widthControl * (currentLayer + 1) + centerStd, heightControl + currentSize.y * 0.1f, 0);
      p3 = new float3(widthControl * currentLayer + centerStd, heightControl + currentSize.y * 0.1f, 0);
      o = vert_DrawQuad(p0, p1, p2, p3, f1111, i, j, o);
    }

    return o;
  }

  float4 frag_DrawSphere(v2f i) { float2 uv = i.uv; float r = dot(uv, uv); float4 color = i.color; if (r > 1.0f || color.a == 0) return f0000; float3 n = new float3(uv, r - 1), _LightDir = new float3(0.321f, 0.766f, -0.557f); float lightAmp = max(0.0f, dot(n, _LightDir)); float4 diffuseLight = (lightAmp + UNITY_LIGHTMODEL_AMBIENT) * color; float spec = max(0, (lightAmp - 0.95f) / 0.05f); color = lerp(diffuseLight, f1111, spec / 4); color.a = 1; return color; }
  float4 frag_DrawLine(v2f i) { float3 p0 = i.p0, p1 = i.p1; float lineRadius = i.ti.w; float2 uv = i.uv; float r = dot(uv, uv), r2 = lineRadius * lineRadius; float4 color = i.color; float3 p10 = p1 - p0; float lp10 = length(p10); if (uv.x < 0) r /= r2; else if (uv.x > lp10) { uv.x -= lp10; r = dot(uv, uv) / r2; } else { uv.x = 0; r = dot(uv, uv) / r2; } if (r > 1.0f || color.a == 0) return f0000; float3 n = new float3(uv, r - 1), _LightDir = new float3(0.321f, 0.766f, -0.557f); float lightAmp = max(0.0f, dot(n, _LightDir)); float4 diffuseLight = (lightAmp + UNITY_LIGHTMODEL_AMBIENT) * color; float spec = max(0, (lightAmp - 0.95f) / 0.05f); color = lerp(diffuseLight, f1111, spec / 4); color.a = 1; return color; }
  float4 frag_DrawArrow(v2f i) { float3 p0 = i.p0, p1 = i.p1; float lineRadius = i.ti.w; float2 uv = i.uv; float r = dot(uv, uv), r2 = lineRadius * lineRadius; float4 color = i.color; float3 p10 = p1 - p0; float lp10 = length(p10); if (uv.x < 0) r /= r2; else if (uv.x > lp10 - lineRadius * 3 && abs(uv.y) > lineRadius) { uv.x -= lp10; uv = rotate_sc(uv, -sign(uv.y) * 0.5f, 0.866025404f); uv.x = 0; r = dot(uv, uv) / r2; } else if (uv.x > lp10) { uv.x -= lp10; r = dot(uv, uv) / r2; } else { uv.x = 0; r = dot(uv, uv) / r2; } if (r > 1.0f || color.a == 0) return f0000; float3 n = new float3(uv, r - 1), _LightDir = new float3(0.321f, 0.766f, -0.557f); float lightAmp = max(0.0f, dot(n, _LightDir)); float4 diffuseLight = (lightAmp + UNITY_LIGHTMODEL_AMBIENT) * color; float spec = max(0, (lightAmp - 0.95f) / 0.05f); color = lerp(diffuseLight, f1111, spec / 4); color.a = 1; return color; }
  float4 frag_DrawLineSegment(v2f i) { float3 p0 = i.p0, p1 = i.p1; float lineRadius = i.ti.w; float2 uv = i.uv; float r = dot(uv, uv), r2 = lineRadius * lineRadius; float4 color = i.color; float3 p10 = p1 - p0; float lp10 = length(p10); uv.x = 0; r = dot(uv, uv) / r2; if (r > 1.0f || color.a == 0) return f0000; float3 n = new float3(uv, r - 1), _LightDir = new float3(0.321f, 0.766f, -0.557f); float lightAmp = max(0.0f, dot(n, _LightDir)); float4 diffuseLight = (lightAmp + UNITY_LIGHTMODEL_AMBIENT) * color; float spec = max(0, (lightAmp - 0.95f) / 0.05f); color = lerp(diffuseLight, f1111, spec / 4); color.a = 1; return color; }
  float4 frag_DrawQuad(Texture2D t, v2f i) { return i.color * tex2Dlod(t, new float4(i.uv, f00)); }
  //float4 frag_DrawWebcam(RWStructuredBuffer<Color> v, v2f i) { GWebCam g = G; return (float4)(i.color * v[id_to_i((uint2)(i.uv * (float2)g.wCam), g.wCam)]); }
  float4 frag_DrawQuad(RWStructuredBuffer<Color> v, v2f i, uint2 imageSize) { GSIFT_AliS g = G; return (float4)(i.color * v[id_to_i((uint2)(i.uv * (float2)imageSize), imageSize)]); }
  //************//
  float4 frag_DrawSIFT(uint octave, uint layer, v2f i, uint2 imageSize) { GSIFT_AliS g = G; return i.color * c32_f4(GetColorFromBuffer(octave, layer, id_to_i((uint2)(i.uv * (float2)imageSize), imageSize))); }
  float frag_smp(uint typeI, uint chI, uint smpI) { return 0; }// smps[smpI * g.chN + chI];
  float4 frag_traceColor(uint typeI, uint chI, uint smpI, float r) { return r < 0.5f ? f0000 : YELLOW; }
  float2 frag_traceClamp(uint typeI, float amp, float traceScale, int smpI) { float yScale = 0.95f; return new float2(smpI, yScale * clamp(amp * traceScale, -1, 1)); }
  float4 frag_DrawSignal(float2 uv, uint typeI, uint chI, uint smpN, float2 panelSize, float maxSignalAmplitude, float signalThickness)
  {
    uv.y = 2 * uv.y - 1;
    float2 scale = new float2(panelSize.x / smpN, panelSize.y / 2), p = new float2(uv.x * (smpN - 1), uv.y) * scale;
    signalThickness *= panelSize.y / panelSize.x;
    int dSmpI = ceili(panelSize.x * signalThickness * smpN / 4) + 1, smpI = roundi(uv.x * (smpN - 1)), smpI0 = max(0, smpI - dSmpI), smpI1 = min(smpI + dSmpI, (int)smpN - 1);
    float r = 0, amp = frag_smp(typeI, chI, (uint)smpI0), maxAmp = amp;
    float2 p0 = frag_traceClamp(typeI, amp, maxSignalAmplitude, smpI0) * scale;
    for (int sI = smpI0 + 1; sI <= smpI1; sI++) { amp = frag_smp(typeI, chI, (uint)sI); maxAmp = max(maxAmp, amp); float2 p1 = frag_traceClamp(typeI, amp, maxSignalAmplitude, sI) * scale; r = max(r, signalThickness / LineSegDist(p0, p1, p)); p0 = p1; }
    return frag_traceColor(typeI, chI, (uint)smpI, r);
  }

  float4 frag(v2f i)
#if (gs_shader)
   : COLOR 
#endif
  {
    GSIFT_AliS g = G;
    float4 color = i.color;
    switch (roundi(i.ti.z))
    {
      case -1: Discard(0); break;
      case drawSphere: color = frag_DrawSphere(i); break;
      case drawLine: color = frag_DrawLine(i); break;
      case drawArrow: color = frag_DrawArrow(i); break;
      case drawLineSegment: color = frag_DrawLineSegment(i); break;
      case drawTexture2D: //color = f1101; break;
        uint m = roundu(i.tSIFTo.x), n = roundu(i.tSIFTl.x); color = frag_DrawSIFT(m, n, i, g.imageSize / (1 << (int)m)); break;// *(g.layers/4)); break;
                                                                                                                                //case drawWebCam: color = frag_DrawWebcam(videoBuffer, i); break;
                                                                                                                                //case drawSignal: color = frag_DrawSignal(i.uv, typeI, chI, g.smpN, panelSize, g.signalAmplitude, g.signalThickness); break;
    }
    if (color.a == 0) Discard(0);
    return color;
  }
#endif //!gs_compute //shader code
#if (!gs_compute) && (!gs_shader)
}
#endif

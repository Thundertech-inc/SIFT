using System;
using UnityEngine;
using GSharp;
using Unity.Mathematics;

[AttGS(GS_Class.Name, "Sift", GS_Class.Description, "Sift")]
public class gsSift_ : GS_Settings
{
  [AttGS(GS_Field.gStruct, GS_Field.inspector)] uint2 imageSize, guassMatrixSize, nImageSize;
  [AttGS(GS_Field.gStruct, GS_Field.inspector)] uint octaves, layers, oTracking, lTracking, processCtrl;

  Color32 OriginalImage, BlurredImages;
  float Guassian;
  uint FeaturePntIDs;
  void Step1_ExtremaDetection() { Size = new object[] { new { nImageSize } }; Include = new object[] { new { OriginalImage }, new { BlurredImages }, new { Guassian }, new { FeaturePntIDs } }; }

  Texture2D image;
  [AttGS(GS_Render.Quads)] void OnRenderObject() { Include = new object[] { new { image }, new { OriginalImage }, new { BlurredImages }, new { FeaturePntIDs } }; }
}

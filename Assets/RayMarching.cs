using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RayMarching : MonoBehaviour
{
    private Camera cam;

    public ComputeShader raymarchComputeShader;
    public Voxeliser voxeliser;

    private int kernelIndex;
    private RenderTexture smokeTexture;
    private Material combiningMaterial;

    private int width, height;

    public float length = 10f;

    public float smoothing;
    public float densityMUlt;

    public Vector3 testCase;
    /////////////////////////////////////////////////
    public int textureWidth = 256;
    public int textureHeight = 256;
    public float scale = 10.0f;
    private RenderTexture resultTexture;
    public int kernalNum2;
    ///////////////////////////////////////////
    /// <summary>
 //   RWTexture2D<float4> _NoiseTexture; uint2 _NoiseResolution;  float _Scale;

    /// </summary>
    void Start()
    {
        cam = Camera.main;
        kernelIndex = raymarchComputeShader.FindKernel("CSRaytrace");
     //   kernalNum2 = raymarchComputeShader.FindKernel()
        combiningMaterial = new Material(Shader.Find("Unlit/Combiner"));


       // width = Screen.width;
       // height = Screen.height;

        width = Mathf.CeilToInt(Screen.width / 4);
        height = Mathf.CeilToInt(Screen.height / 4);

        // Initialize and set up the smoke texture
        smokeTexture = new RenderTexture(width,height , 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        smokeTexture.enableRandomWrite = true;
        // Set additional shader properties
        raymarchComputeShader.SetFloat("_RayMaxLength", 50.0f);
        raymarchComputeShader.SetVector("_BoundsExtent", voxeliser.boundsExtent);
        
        raymarchComputeShader.SetInt("_TextureWidth", width);
        raymarchComputeShader.SetInt("_TextureHeight", height);

        raymarchComputeShader.SetInt("_VoxelsX", voxeliser.voxelsX);
        raymarchComputeShader.SetInt("_VoxelsY", voxeliser.voxelsY);
        raymarchComputeShader.SetInt("_VoxelsZ", voxeliser.voxelsZ);


     //   resultTexture = new RenderTexture(256,256, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
     //   resultTexture.enableRandomWrite = true;


     //   raymarchComputeShader.SetTexture(0, "_NoiseTexture", resultTexture);
     //   raymarchComputeShader.SetVector("_NoiseResolution", new Vector2(textureWidth, textureHeight));
    //    raymarchComputeShader.SetFloat("_Scale", scale);
    //    raymarchComputeShader.Dispatch(kernelHandle, textureWidth / 8, textureHeight / 8, 1);


    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Set matrix and buffer data
        raymarchComputeShader.SetMatrix("_CamToWorldMatrix", cam.cameraToWorldMatrix);
        raymarchComputeShader.SetMatrix("_invProjectionMatrix",cam.projectionMatrix.inverse);
        raymarchComputeShader.SetVector("camPosWS", cam.transform.position);
        raymarchComputeShader.SetBuffer(kernelIndex, "_SmokeVoxels", voxeliser.smokeBuffer);

        raymarchComputeShader.SetFloat("_VoxelResolution", voxeliser.voxelSize);

        raymarchComputeShader.SetFloat("_Smoothing", smoothing);
        raymarchComputeShader.SetFloat("_DensityMult", densityMUlt);

        raymarchComputeShader.SetVector("_SmokeOrigin", voxeliser.smokeOrigin);
        raymarchComputeShader.SetFloat("_SmokeRadius", (float)voxeliser.smokeRadius);
        // Dispatch the compute shader
        raymarchComputeShader.SetTexture(kernelIndex, "_ResultTexture", smokeTexture);
        raymarchComputeShader.Dispatch(kernelIndex, smokeTexture.width / 8, smokeTexture.height / 8, 1);

        // Combine textures
        combiningMaterial.SetTexture("_MainTex", source);
        combiningMaterial.SetTexture("_SmokeTex", smokeTexture);

        Graphics.Blit(source, destination, combiningMaterial);


    }
    private void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            float nLen = (testCase - voxeliser.smokeOrigin).magnitude;
            nLen /= voxeliser.smokeRadius;
            nLen = 1 - Mathf.SmoothStep(0, 1, nLen);
            print(nLen);
        }
    }
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cam.transform.position, 1);

        Gizmos.DrawWireSphere(testCase,1);
    }

    private void OnDisable()
    {
        // Release resources
        smokeTexture.Release();
    }
}

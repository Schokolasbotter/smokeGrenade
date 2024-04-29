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

    private ComputeBuffer rayPositionsBuffer;
    public Vector3[] rayPositions;

    private ComputeBuffer rayColBuffer;
    public Vector4[] rayCol;

    private int width, height;

    public float length = 10f;
    void Awake()
    {
        cam = Camera.main;
        kernelIndex = raymarchComputeShader.FindKernel("CSRaytrace");
        combiningMaterial = new Material(Shader.Find("Unlit/Combiner"));


       // width = Screen.width;
       // height = Screen.height;

        width = Mathf.CeilToInt(Screen.width / 16);
        height = Mathf.CeilToInt(Screen.height / 16);

        // Initialize and set up the smoke texture
        smokeTexture = new RenderTexture(width,height , 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        smokeTexture.enableRandomWrite = true;

        // Initialize and set up the compute buffer for ray vectors
        rayPositions = new Vector3[25 * width * height];
        rayPositionsBuffer = new ComputeBuffer(rayPositions.Length, sizeof(float) * 3);
        raymarchComputeShader.SetBuffer(kernelIndex, "_RayPositionsBuffer", rayPositionsBuffer);

        rayCol = new Vector4[25 * width * height];
        rayColBuffer = new ComputeBuffer(rayPositions.Length, sizeof(float) * 4);
        raymarchComputeShader.SetBuffer(kernelIndex, "_RayColBuffer", rayColBuffer);

        // Set additional shader properties
        raymarchComputeShader.SetFloat("_RayMaxLength", 100.0f);
        raymarchComputeShader.SetVector("_BoundsExtent", voxeliser.boundsExtent);
        raymarchComputeShader.SetFloat("_VoxelResolution", voxeliser.voxelSize);
        raymarchComputeShader.SetInt("_TextureWidth", width);
        raymarchComputeShader.SetInt("_TextureHeight", height);

        raymarchComputeShader.SetInt("_VoxelsX", voxeliser.voxelsX);
        raymarchComputeShader.SetInt("_VoxelsY", voxeliser.voxelsY);
        raymarchComputeShader.SetInt("_VoxelsZ", voxeliser.voxelsZ);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Set matrix and buffer data
        raymarchComputeShader.SetMatrix("_CamToWorldMatrix", cam.cameraToWorldMatrix);
        raymarchComputeShader.SetMatrix("_invProjectionMatrix",cam.projectionMatrix.inverse);
        raymarchComputeShader.SetVector("camPosWS", cam.transform.position);
        raymarchComputeShader.SetBuffer(kernelIndex, "_SmokeVoxels", voxeliser.smokeBuffer);

        // Dispatch the compute shader
        raymarchComputeShader.SetTexture(kernelIndex, "_ResultTexture", smokeTexture);
        raymarchComputeShader.Dispatch(kernelIndex, smokeTexture.width / 8, smokeTexture.height / 8, 1);

        // Combine textures
        combiningMaterial.SetTexture("_MainTex", source);
        combiningMaterial.SetTexture("_SmokeTex", smokeTexture);
        Graphics.Blit(source, destination, combiningMaterial);
    }

    private void OnDrawGizmos()
    {

        /*
        // Draw ray vectors as Gizmos
        rayVectorBuffer.GetData(rayVectors);
        Gizmos.color = Color.green;
        for (int i = 0; i < rayVectors.Length; i++)
        {
            Vector3 start = cam.transform.position;
            Vector3 end = start + rayVectors[i] * length;
            Gizmos.DrawLine(start, end);
        }
        */

        rayPositionsBuffer.GetData(rayPositions);
        rayColBuffer.GetData(rayCol);
        
        for (int i = 0; i < rayPositions.Length; i++)
        {
            Gizmos.color = new Color(rayCol[i].x, rayCol[i].y, rayCol[i].z, rayCol[i].w);
            Gizmos.DrawWireSphere(rayPositions[i], 0.3f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cam.transform.position, 1);
    }

    private void OnDisable()
    {
        // Release resources
        smokeTexture.Release();
        rayPositionsBuffer.Release();
    }
}

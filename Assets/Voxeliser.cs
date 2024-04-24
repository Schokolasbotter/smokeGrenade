using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxeliser : MonoBehaviour
{
    [Header("Half the scale of the area")]
    public Vector3 boundsExtent = new Vector3(3, 3, 3);
    
    [Header("")]
    public float voxelSize = 0.5f;
    private ComputeBuffer argBuffer, buffer;

    public int voxelsX, voxelsY, voxelsZ, totalVoxels;

    public Mesh debugMesh;
    private Material debugMaterial;
    private Bounds debugBounds;

    // Start is called before the first frame update
    void OnEnable()
    {
        

        Vector3 boundsSize = boundsExtent * 2;
        debugBounds = new Bounds(new Vector3(0, boundsExtent.y, 0), boundsSize);

        voxelsX = Mathf.CeilToInt(boundsSize.x / voxelSize);
        voxelsY = Mathf.CeilToInt(boundsSize.y / voxelSize);
        voxelsZ = Mathf.CeilToInt(boundsSize.z / voxelSize);
        totalVoxels = voxelsX * voxelsY * voxelsZ;

        //Debug Voxels
        debugMaterial = new Material(Shader.Find("Unlit/DebugVoxels"));

        argBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)debugMesh.GetIndexCount(0);
        args[1] = (uint)totalVoxels;
        args[2] = (uint)debugMesh.GetIndexStart(0);
        args[3] = (uint)debugMesh.GetBaseVertex(0);
        argBuffer.SetData(args);
    }
    private void Update()
    {
        debugMaterial.SetVector("_VoxelResolution", new Vector3(voxelsX, voxelsY, voxelsZ));
        debugMaterial.SetVector("_BoundsExtent", boundsExtent);
        debugMaterial.SetFloat("_VoxelSize", voxelSize);
       // debugMaterial.SetInt("_MaxFillSteps", maxFillSteps);
        Graphics.DrawMeshInstancedIndirect(debugMesh, 0, debugMaterial, debugBounds, argBuffer);
    }

    void OnDestroy()
    {
        if (buffer != null)
            buffer.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(debugBounds.center, debugBounds.extents * 2);
    }
}

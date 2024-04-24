using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public Vector3 boundsExtent = new Vector3(3, 3, 3);

    public float voxelSize = 0.5f;
    private ComputeBuffer argBuffer, buffer;

    public Mesh debugMesh;
    private Material debugMaterial;
    private Bounds debugBounds;

    // Start is called before the first frame update
    void OnEnable()
    {
        private int voxelX = boundsExtent.x;



      //  buffer = new ComputeBuffer(voxelBound, 1);  //creates a new compute buffer containing voxelBound Elements, each having 1 byte of memory allocated

    }
    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(debugMesh, 0, debugMaterial,);

    }   

    void OnDestroy()
    {
        if (buffer != null)
            buffer.Release();
    }
}

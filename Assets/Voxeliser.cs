using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class Voxeliser : MonoBehaviour
{
    [Header("Half the scale of the area")]
    public Vector3 boundsExtent = new Vector3(3, 3, 3);
    
    [Header("")]
    public float voxelSize = 0.5f;
    private ComputeBuffer argBuffer;
    public ComputeBuffer smokeBuffer;
    public int[] smokeArr;
    public int voxelsX, voxelsY, voxelsZ, totalVoxels;

    public float smokeRadius = 4;
    public Vector3 smokeOrigin;
    private float smokeTimer = 10f;
    public float smokeExpansionTime;
    public AnimationCurve easingCurve;

    public Mesh debugMesh;
    private Material debugMaterial;
    private Bounds debugBounds;

    public bool Debugging = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        Vector3 boundsSize = boundsExtent * 2;
        debugBounds = new Bounds(new Vector3(0, boundsExtent.y, 0), boundsSize);

        voxelsX = Mathf.CeilToInt(boundsSize.x / voxelSize);
        voxelsY = Mathf.CeilToInt(boundsSize.y / voxelSize);
        voxelsZ = Mathf.CeilToInt(boundsSize.z / voxelSize);
        totalVoxels = voxelsX * voxelsY * voxelsZ;



        smokeBuffer = new ComputeBuffer(totalVoxels, sizeof(int));
        smokeArr = new int[totalVoxels];
        int[] initialSmokeData = new int[totalVoxels]; // Create an array to hold initial smoke data
        for (int i = 0; i < totalVoxels; i++)
        {
            initialSmokeData[i] = 0; // Set each voxel value to 0
        }
        smokeBuffer.SetData(initialSmokeData);


        //Debug Voxels
        debugMaterial = new Material(Shader.Find("Unlit/DebugVoxels"));

        argBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);  
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)debugMesh.GetIndexCount(0);
        args[1] = (uint)totalVoxels;
        args[2] = (uint)debugMesh.GetIndexStart(0);
        args[3] = (uint)debugMesh.GetBaseVertex(0);
        argBuffer.SetData(args);

        InspectSmokeBuffer();
    }
    private void Update()
    {
        if(Input.GetMouseButton(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                smokeTimer = 0f;
                smokeOrigin = hit.point;
                
            }
        }

        if (Time.frameCount % 30 == 0)
        {
            UpdateSmokeBuffer();
        }

        smokeTimer += Time.deltaTime;


        if (!Debugging) return;
        debugMaterial.SetVector("_VoxelResolution", new Vector3(voxelsX, voxelsY, voxelsZ));
        debugMaterial.SetVector("_BoundsExtent", boundsExtent);
        debugMaterial.SetFloat("_VoxelSize", voxelSize);

        debugMaterial.SetBuffer("_SmokeBuffer", smokeBuffer);

        Graphics.DrawMeshInstancedIndirect(debugMesh, 0, debugMaterial, debugBounds, argBuffer);
    }

    private void UpdateSmokeBuffer()
    {
        // Clear the smoke buffer
        int[] smokeData = new int[totalVoxels];
        smokeBuffer.SetData(smokeData);
        


        // Iterate through each voxel
        for (int x = 0; x < voxelsX; x++)
        {
            for (int y = 0; y < voxelsY; y++)
            {
                for (int z = 0; z < voxelsZ; z++)
                {
                    // Calculate the position of the current voxel
                    Vector3 voxelPos = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);
                    voxelPos -= new Vector3(boundsExtent.x,0,boundsExtent.z);
                    // Calculate the distance between the voxel and the smoke origin
                    float distance = Vector3.Distance(voxelPos, smokeOrigin);
                    //Calculate smokeRadius from timer;
                    float normalValue = Mathf.InverseLerp(0f, smokeExpansionTime, smokeTimer);
                    float currentSmokeRadius = easingCurve.Evaluate(normalValue) * smokeRadius;
                    
                    // If the distance is within the radius, set the voxel value to 1 in the smoke buffer
                    if (distance <= currentSmokeRadius)
                    {
                        int index = x + y * voxelsX + z * voxelsX * voxelsY;
                        smokeData[index] = 1;
                    }
                }
            }
        }

        // Update the smoke buffer with the new data
        smokeBuffer.SetData(smokeData);
        smokeBuffer.GetData(smokeArr);
        InspectSmokeBuffer();
    }


    void InspectSmokeBuffer()
    {
        // Create an array to hold the data retrieved from the buffer
        int[] smokeBufferData = new int[totalVoxels]; // Assuming totalVoxels is the size of your buffer

        // Get the data from the buffer and store it in the array
        smokeBuffer.GetData(smokeBufferData);

        // Now you can inspect the contents of the array, which represents the buffer data
        for (int i = 0; i < totalVoxels; i++)
        {
            // Output each element of the array to the console
           // Debug.Log("SmokeBuffer[" + i + "] = " + smokeBufferData[i]);
        }
    }

    void OnDestroy()
    {
        if (smokeBuffer != null)
            smokeBuffer.Release();
        if (argBuffer != null)
            smokeBuffer.Release();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(debugBounds.center, debugBounds.extents * 2);
    }
}

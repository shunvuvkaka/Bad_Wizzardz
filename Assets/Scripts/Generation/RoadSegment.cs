using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class RoadSegment : MonoBehaviour
{
    private MeshFilter mf;
    private MeshCollider mc;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();

        if (mf.mesh == null)
            mf.mesh = new Mesh();
    }

    public void ApplyMesh(Vector3 leftA, Vector3 rightA, Vector3 leftB, Vector3 rightB, Material mat)
    {
        Mesh mesh = mf.mesh;
        mesh.Clear();

        mesh.vertices = new Vector3[]
        {
            leftA, rightA, leftB, rightB
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            1, 3, 2
        };

        mesh.RecalculateNormals();

        if (mc != null)
            mc.sharedMesh = mesh;

        GetComponent<MeshRenderer>().material = mat;
        GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
    }
}


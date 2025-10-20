using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float meshSize;
    [Header("Plane")]
    [SerializeField] private int nbLignes;
    [SerializeField] private int nbColones;
    [Header("Cylindre")]
    [SerializeField] private float height;
    [SerializeField] private float rayon;
    [SerializeField] private float side;

    private MeshRenderer mRenderer;
    private MeshFilter mFilter;



    private Mesh mesh;
    private List<Vector3> verticices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    void Start()
    {
        Init();
        Plane();
    }


    private void Init()
    {
        mRenderer = transform.AddComponent<MeshRenderer>();
        mRenderer.sharedMaterial = material;
        mFilter = transform.AddComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "CustomMesh";
    }


    private void Plane()
    {
        //CreatPlane();
        CreatCylindre();
        DrawMesh();

    }


    private void CreatCylindre()
    {
        float angle = 0;

        int vUp = CreatVertex(new Vector3(0, height / 2,0));
        int vDown = CreatVertex(new Vector3(0, -height / 2, 0));

        int v1 = CreatVertex(new Vector3(rayon * Mathf.Cos(angle), -height / 2, rayon * Mathf.Sin(angle)));
        int v2 = CreatVertex(new Vector3(rayon * Mathf.Cos(angle), height / 2, rayon * Mathf.Sin(angle)));

        for (int i = 0; i < side; i++)
        {
            angle += 2 * Mathf.PI / side;
            int v3 = CreatVertex(new Vector3(rayon * Mathf.Cos(angle), height / 2, rayon * Mathf.Sin(angle)));
            int v4 = CreatVertex(new Vector3(rayon * Mathf.Cos(angle), -height / 2, rayon * Mathf.Sin(angle)));

            CreatTriangle(v1, v2, v4);
            CreatTriangle(v2, v3, v4);

            CreatTriangle(vUp,v3,v2);
            CreatTriangle(vDown, v1, v4);

            v1 = v4;
            v2 = v3;
        }
    }

    private void CreatPlane()
    {
        for (int i = 0; i < nbLignes; i++)
        {
            for(int j = 0; j < nbColones; j++)
            {

                int v1 = CreatVertex(new Vector3(j* meshSize, i* meshSize, 0));
                int v2 = CreatVertex(new Vector3(j * meshSize, (i+1) * meshSize, 0));
                int v3 = CreatVertex(new Vector3((j+1) * meshSize, (i + 1) * meshSize, 0));
                int v4 = CreatVertex(new Vector3((j+1) * meshSize, i * meshSize, 0));

                CreatTriangle(v1, v2, v4);
                CreatTriangle(v2, v3, v4);
            }
        }
    }


    private int CreatVertex(Vector3 pos)
    {
        if(!verticices.Contains(pos))
        {
            verticices.Add(pos);
        }

        return verticices.IndexOf(pos);

    }

    private void CreatTriangle(int v1,  int v2, int v3)
    {
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
    }


    private void DrawMesh()
    {
        mesh.vertices = verticices.ToArray();
        mesh.triangles = triangles.ToArray();

        foreach(int i in mesh.triangles)
            Debug.Log(i);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        mFilter.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 v in verticices)
        {
            Gizmos.DrawSphere(v, 0.1f);
            Handles.Label(v, "v"+verticices.IndexOf(v));
        }
    }
}

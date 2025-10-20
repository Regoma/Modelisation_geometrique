using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Material material;
    
    [Header("Plane")]
    [SerializeField] private Vector2 planeSize;
    [SerializeField] private int nbLignes;
    [SerializeField] private int nbColones;
    [Header("Cylindre")]
    [SerializeField] private float height;
    [SerializeField] private float rayon;
    [SerializeField] private int side;
    [Header("Sphere")]
    [SerializeField] private float sRayon;
    [SerializeField] private int sSide;
    [SerializeField] private float sLoop;
    [Header("Cone")]
    [SerializeField] private float cRayon;
    [SerializeField] private float cHeight;
    [SerializeField] private int cSide;
    [SerializeField] private float cTronque;

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

    private void Update()
    {
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
        verticices.Clear();
        triangles.Clear();
        mesh.Clear();

        //CreatPlane();
        //CreatCylindre();
        CreatSphere();
        //CreatCone();
        DrawMesh();

    }

    private void CreatCone()
    {
        float angle = 0;

        float y = cHeight / 2 - cTronque;
        float r = cTronque / cHeight * cRayon;
        //float r = Mathf.Sqrt(Mathf.Max(0, cRayon * cRayon - y * y));

        int vUp = CreatVertex(new Vector3(0, y, 0));
        int vDown = CreatVertex(new Vector3(0, -cHeight / 2, 0));

        int v1 = CreatVertex(new Vector3(cRayon * Mathf.Cos(angle), -cHeight / 2, cRayon * Mathf.Sin(angle)));
        int v2 = CreatVertex(new Vector3(r * Mathf.Cos(angle), y, r * Mathf.Sin(angle)));

        for (int i = 0; i < cSide; i++)
        {
            angle += 2 * Mathf.PI / cSide;
            int v3 = CreatVertex(new Vector3(r * Mathf.Cos(angle), y, r * Mathf.Sin(angle)));
            int v4 = CreatVertex(new Vector3(cRayon * Mathf.Cos(angle), -cHeight / 2, cRayon * Mathf.Sin(angle)));

            CreatTriangle(v1, v2, v4);
            CreatTriangle(v2, v3, v4);

            CreatTriangle(vUp, v3, v2);
            CreatTriangle(vDown, v1, v4);

            v1 = v4;
            v2 = v3;
        }
    }

    private void CreatSphere()
    {
        float angle = 0;
        float sHeight = sRayon * 2;

        int vUp = CreatVertex(new Vector3(0, sRayon, 0));
        int vDown = CreatVertex(new Vector3(0, -sRayon, 0));
        float deltaTheta = Mathf.PI / sLoop;

        for (int l = 0; l < sLoop; l++)
        {
            angle = 0;
            float theta1 = deltaTheta * l;
            float theta2 = deltaTheta * (l + 1);

            float y1 = sRayon * Mathf.Cos(theta1);
            float y2 = sRayon * Mathf.Cos(theta2);
            float r1 = sRayon * Mathf.Sin(theta1);
            float r2 = sRayon * Mathf.Sin(theta2);

            int v1 = CreatVertex(new Vector3(r1 * Mathf.Cos(angle), y1, r1 * Mathf.Sin(angle)));
            int v2 = CreatVertex(new Vector3(r2 * Mathf.Cos(angle), y2, r2 * Mathf.Sin(angle)));

            for (int i = 0; i < sSide; i++)
            {
                angle += 2 * Mathf.PI / sSide;
                int v3 = CreatVertex(new Vector3(r2 * Mathf.Cos(angle),y2, r2 * Mathf.Sin(angle)));
                int v4 = CreatVertex(new Vector3(r1 * Mathf.Cos(angle), y1, r1 * Mathf.Sin(angle)));
                CreatTriangle(v2, v1, v4);
                CreatTriangle(v3, v2, v4);

                v1 = v4;
                v2 = v3;

            }
        }
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

                int v1 = CreatVertex(new Vector3(j* planeSize.x, i* planeSize.y, 0));
                int v2 = CreatVertex(new Vector3(j * planeSize.x, (i+1) * planeSize.y, 0));
                int v3 = CreatVertex(new Vector3((j+1) * planeSize.x, (i + 1) * planeSize.y, 0));
                int v4 = CreatVertex(new Vector3((j+1) * planeSize.x, i * planeSize.y, 0));

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

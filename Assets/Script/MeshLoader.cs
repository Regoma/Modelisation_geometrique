using System.Collections.Generic;
using System.IO;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MeshLoder : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private string fileName;
    private MeshRenderer mRenderer;
    private MeshFilter mFilter;

    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    void Start()
    {
        Init();
        LoadMesh();
    }

    private void Init()
    {
        mRenderer = transform.AddComponent<MeshRenderer>();
        mRenderer.sharedMaterial = material;
        mFilter = transform.AddComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "LoadedMesh";
    }

    private void LoadMesh()
    {
        string path = "Assets/Off_Meshes/" + fileName;
        StreamReader reader = new StreamReader(path);
        string extention = reader.ReadLine();
        //if (extention != "OFF")
          //  return;
        string[] headers = reader.ReadLine().Split(" ");

        Vector3 center = Vector3.zero;
        float maxSize = 0;
        for (int vertex = 0; vertex < int.Parse(headers[0]); vertex++)
        {
            string[] coord = reader.ReadLine().Replace(".", ",").Split(" ");
            float x = float.Parse(coord[0]);
            float y = float.Parse(coord[1]);
            float z = float.Parse(coord[2]);
            CreatVertex(new Vector3(x, y, z));
            center += new Vector3(x, y, z);
            maxSize = Mathf.Max(maxSize, Mathf.Abs(new Vector3(x, y, z).magnitude));
        }
        center = center / int.Parse(headers[0]);       
        for(int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = (vertices[i] - center) / maxSize;
        }
            

        for (int triangle = 0; triangle < int.Parse(headers[1]); triangle++)
        {
            string[] index = reader.ReadLine().Split(" ");
            int v1 = int.Parse(index[1]);
            int v2 = int.Parse(index[2]);
            int v3 = int.Parse(index[3]);
            CreatTriangle(v1, v2, v3);
        }

        reader.Close();
        DrawMesh();
    }

    private int CreatVertex(Vector3 pos)
    {
        if (!vertices.Contains(pos))
        {
            vertices.Add(pos);
        }

        return vertices.IndexOf(pos);

    }

    private void CreatTriangle(int v1, int v2, int v3)
    {
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
    }

    private void CratNormal(Vector3 v)
    {
        int index = vertices.IndexOf(v);
        Vector3 normal = Vector3.zero;
        int nbNormal = 0;
        for (int i = 0; i < triangles.Count; i +=3)
        {

            if (triangles[i] == index || triangles[i+1] == index || triangles[i+2] == index)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i+1]];
                Vector3 v2 = vertices[triangles[i+2]];
                Vector3 e0 = v1 - v0;
                Vector3 e1 = v2 - v0;
                Vector3 faceNormal = Vector3.Cross(e0, e1);
                faceNormal.Normalize();
                normal += faceNormal ;
                nbNormal++;
            }
        }
        normal = (normal / nbNormal);
        normal.Normalize();
    }


    private void DrawMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mFilter.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 v in vertices)
        {
            Gizmos.DrawSphere(v, 0.1f);
            Handles.Label(v, "v_" + vertices.IndexOf(v));
        }
    }
}

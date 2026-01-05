using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Loop : MonoBehaviour
{
    [SerializeField][Range(0, 3)] private int iteration = 1;
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

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
            LoadMesh();
    }

    private void Init()
    {
        mRenderer = transform.AddComponent<MeshRenderer>();
        mRenderer.sharedMaterial = material;
        mFilter = transform.AddComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "Subdivide Mesh";
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
        for (int i = 0; i < vertices.Count; i++)
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

        SubdivideMesh();
    }

    private void SubdivideMesh()
    {
        for (int subdivide = 0; subdivide < iteration; subdivide++)
        {
            List<int> oldTriangles = new List<int>(triangles);
            triangles.Clear();
            for (int i = 0; i < oldTriangles.Count; i += 3)
            {
                Vector3 v1 = vertices[oldTriangles[i]];
                Vector3 v2 = vertices[oldTriangles[i + 1]];
                Vector3 v3 = vertices[oldTriangles[i + 2]];

                Vector3 e1 = v2 - v1;
                Vector3 e2 = v3 - v2;
                Vector3 e3 = v1 - v3;

                int _v1 = CreatVertex(e1 * 0.5f + v1);
                int _v2 = CreatVertex(e2 * 0.5f + v2);
                int _v3 = CreatVertex(e3 * 0.5f + v3);

                CreatTriangle(oldTriangles[i], _v1, _v3);
                CreatTriangle(_v1, oldTriangles[i + 1], _v2);
                CreatTriangle(_v3, _v2, oldTriangles[i + 2]);
                CreatTriangle(_v1, _v2, _v3);

            }

            List<Vector3> oldVertices = new List<Vector3>(vertices);
            for (int j = 0; j < vertices.Count; j++)
            {
                List<int> neighbor = GetNeighborVertices(j);
                Debug.Log("V_" + j + ": ");
                Vector3 pos = oldVertices[j];
                foreach (int i in neighbor)
                {
                    pos += oldVertices[i];
                    Debug.Log(i);
                }

                pos /= neighbor.Count + 1;

                vertices[j] = pos;
            }
        }
        DrawMesh();
    }

     private List<int> GetNeighborVertices(int index)
    {
        List<int> vertices = new List<int>();
        for (int i = 0; i < triangles.Count; i += 3)
        {
            if(triangles[i] == index)
            {
                if(!vertices.Contains(triangles[i+1]))
                    vertices.Add(triangles[i+1]);
                if(!vertices.Contains(triangles[i+2]))
                    vertices.Add(triangles[i+2]);
            }
            if (triangles[i+1] == index)
            {
                if(!vertices.Contains(triangles[i+2]))
                    vertices.Add(triangles[i+2]);
                if(!vertices.Contains(triangles[i]))
                    vertices.Add(triangles[i]);
            }
            if(triangles[i+2] == index)
            {
                if(!vertices.Contains(triangles[i + 1]))
                vertices.Add(triangles[i+1]);
                if (!vertices.Contains(triangles[i]))
                    vertices.Add(triangles[i]);
            }
        }
        return vertices;
    }

    private int CreatVertex(Vector3 pos)
    {
        if (!ContainsPos(pos))
        {
            vertices.Add(pos);
        }

        return IndexOfPos(pos);

    }

    private bool ContainsPos(Vector3 pos)
    {
        foreach(Vector3 v in vertices)
        {
            if(Vector3.Distance(pos, v) <0.01f)
                return true;
        }

        return false;
    }

    private int IndexOfPos(Vector3 pos)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if(Vector3.Distance(pos, vertices[i]) < 0.01f)
                return i;
        }
        return 0;
    }

    private void CreatTriangle(int v1, int v2, int v3)
    {
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
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
            //Gizmos.DrawSphere(v, 0.1f);
            Handles.Label(v, "v_" + vertices.IndexOf(v));
        }

    }


}

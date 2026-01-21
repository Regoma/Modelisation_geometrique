using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Loop : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private string fileName;
    private MeshRenderer mRenderer;
    private MeshFilter mFilter;

    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    List<int> oldTriangles = new List<int>();
    List<Vector3> oldVertices = new List<Vector3>();
    Dictionary<(int, int), int> edgeCache = new Dictionary<(int, int), int>();
    void Start()
    {
        Init();
        LoadMesh();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
            SubdivideMesh();
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

        DrawMesh();
    }
    
    private void SubdivideMesh()
    {
            edgeCache = new Dictionary<(int, int), int>();
            oldTriangles = new List<int>(triangles);
            oldVertices = new List<Vector3>(vertices);
            int originalVertexCount = oldVertices.Count;
            triangles.Clear();
            for (int i = 0; i < oldTriangles.Count; i += 3)
            {
                int a = oldTriangles[i];
                int b = oldTriangles[i + 1];
                int c = oldTriangles[i + 2];

                int ab = GetEdgePoint(a, b);
                int bc = GetEdgePoint(b, c);
                int ca = GetEdgePoint(c, a);

                CreatTriangle(a, ab, ca);
                CreatTriangle(b, bc, ab);
                CreatTriangle(c, ca, bc);
                CreatTriangle(ab, bc, ca);
            }

            for (int j = 0; j < originalVertexCount; j++)
            {
                List<int> neighbors = GetNeighborVertices(j);
                int n = neighbors.Count;

                float beta = (n == 3) ? 3f / 16f : 3f / (8f * n);

                Vector3 newPos = (1 - n * beta) * oldVertices[j];
                foreach (int v in neighbors)
                    newPos += beta * oldVertices[v];

                vertices[j] = newPos;
            }
        
        DrawMesh();
    }

    private List<int> GetNeighborVertices(int index)
    {
        List<int> neighbors = new List<int>();

        for (int i = 0; i < oldTriangles.Count; i += 3)
        {
            int a = oldTriangles[i];
            int b = oldTriangles[i + 1];
            int c = oldTriangles[i + 2];

            if (a == index)
            {
                if (b < oldVertices.Count) neighbors.Add(b);
                if (c < oldVertices.Count) neighbors.Add(c);
            }
            else if (b == index)
            {
                if (a < oldVertices.Count) neighbors.Add(a);
                if (c < oldVertices.Count) neighbors.Add(c);
            }
            else if (c == index)
            {
                if (a < oldVertices.Count) neighbors.Add(a);
                if (b < oldVertices.Count) neighbors.Add(b);
            }
        }

        return neighbors.Distinct().ToList();
    }
    int GetEdgePoint(int a, int b)
    {
        if (a > b) (a, b) = (b, a);

        if (edgeCache.TryGetValue((a, b), out int index))
            return index;

        Vector3 mid = (oldVertices[a] + oldVertices[b]) * 0.5f;
        index = CreatVertex(mid);
        edgeCache[(a, b)] = index;
        return index;
    }


    private int CreatVertex(Vector3 pos)
    {
        Vector3 roundPos = RoundVector(pos);

        if (!vertices.Contains(pos))
        {
            vertices.Add(pos);
        }

        return vertices.IndexOf(pos);

    }

    private Vector3 RoundVector(Vector3 pos)
    {
        Vector3 v = pos * 1000;
        Vector3 roundPos = new Vector3(
        v.x = Mathf.Round(v.x),
        v.y = Mathf.Round(v.y),
        v.z = Mathf.Round(v.z));
        roundPos /= 1000;
        return roundPos;

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

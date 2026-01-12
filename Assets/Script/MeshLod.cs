using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;


public class MeshLod : MonoBehaviour
{
    [SerializeField][Range(0.025f,0.25f)] private float epsilone;
    [SerializeField] private Material material;
    [SerializeField] private string fileName;
    private MeshRenderer mRenderer;
    private MeshFilter mFilter;

    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector3> normals = new List<Vector3>();


    private Mesh lodMesh;
    private List <Cell> grid = new List<Cell>();
    private List<Vector3> lodVertices = new List<Vector3>();
    private List<int> lodTriangles = new List<int>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        LoadMesh();
        CreatGrid();
        FillGride();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Remesh();

        }
        //CreatGrid();
    }

    private void OnValidate()
    {
        CreatGrid();
        FillGride();
    }

    private void CreatGrid()
    {
        if (mesh == null)
            return;

        grid.Clear();

        Vector3 grideOrigine = transform.position + mesh.bounds.center;
        int x = Mathf.CeilToInt(mesh.bounds.size.x / epsilone);
        int y = Mathf.CeilToInt(mesh.bounds.size.y / epsilone);
        int z = Mathf.CeilToInt(mesh.bounds.size.z / epsilone);
        Vector3Int grideSize = new Vector3Int(x, y, z);

        for (x = -grideSize.x / 2; x <= grideSize.x / 2; x++)
            for (y = -grideSize.y / 2; y <= grideSize.y / 2; y++)
                for (z = -grideSize.z / 2; z <= grideSize.z / 2; z++)
                    grid.Add(new(grideOrigine + new Vector3(x * epsilone, y * epsilone, z * epsilone)));

        
    }

    private void FillGride()
    {
        float half =epsilone  * 0.5f;

        foreach (Cell c in grid)
        {
            c.cellVertices.Clear();
            c.avregeVertex = Vector3.zero;
            foreach (Vector3 v in vertices)
            {
                if(v.x >= c.pos.x - half && v.x <= c.pos.x + half &&
                v.y >= c.pos.y - half && v.y <= c.pos.y + half &&
                v.z >= c.pos.z - half && v.z <= c.pos.z + half)
                {
                    c.cellVertices.Add(v);
                    c.avregeVertex += v;
                }
            }
            if(c.cellVertices.Count > 0)
                c.avregeVertex /= c.cellVertices.Count;
        }

        
    }

    private void Remesh()
    {
        lodVertices.Clear();
        lodTriangles.Clear();

        foreach (Cell c in grid)
        {
            if(c.cellVertices.Count > 0)
            {
                lodVertices.Add(c.avregeVertex);
                c.index = lodVertices.IndexOf(c.avregeVertex);
            }
                
        }

        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            Cell c1 = GetCell(v1);
            Cell c2 = GetCell(v2);
            Cell c3 = GetCell(v3);

            if (c1 == null || c2 == null || c3 == null) 
                continue;
            if (c1 == c2 || c2 == c3 || c1 == c3) 
                continue;

            lodTriangles.Add(c1.index);
            lodTriangles.Add(c2.index);
            lodTriangles.Add(c3.index);
        }
        
        lodMesh = new Mesh();
        lodMesh.vertices = lodVertices.ToArray();
        lodMesh.triangles = lodTriangles.ToArray();
        lodMesh.RecalculateBounds();
        lodMesh.RecalculateNormals();
        lodMesh.RecalculateTangents();
        mFilter.sharedMesh = lodMesh;
        

        //DrawMesh();


    }

    private Cell GetCell(Vector3 vertex)
    {
        foreach (Cell c in grid)
        {
            if(c.cellVertices.Contains(vertex))
                return c;
        }
        return null;
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
        vertices.Clear();
        triangles.Clear();
        normals.Clear();

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

        /*
        foreach (Vector3 v in vertices)
        {
            normals.Add(CreatNormal(v));
        }
        */
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

    private Vector3 CreatNormal(Vector3 v)
    {
        

        int index = vertices.IndexOf(v);
        Vector3 normal = Vector3.zero;
        int nbNormal = 0;
        for (int i = 0; i < triangles.Count; i += 3)
        {

            if (triangles[i] == index || triangles[i + 1] == index || triangles[i + 2] == index)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];
                Vector3 e0 = v1 - v0;
                Vector3 e1 = v2 - v0;
                Vector3 faceNormal = Vector3.Cross(e0, e1);
                faceNormal.Normalize();
                normal += faceNormal;
                nbNormal++;
            }
        }
        normal = (normal / nbNormal);
        normal.Normalize();

        return normal;
    }



    private void DrawMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mFilter.sharedMesh = mesh;
    }

    private void OnDrawGizmosSelected()
    {
        foreach(Cell c in grid)
        {
            Gizmos.DrawWireCube(c.pos + transform.position, Vector3.one * epsilone);
            Gizmos.DrawSphere(c.avregeVertex + transform.position, 0.005f);
        }
        
    }
}

[SerializeField]
public class Cell
{
    public Vector3 pos;
    public List<Vector3> cellVertices;
    public Vector3 avregeVertex;
    public int index;
    public Cell(Vector3 _pos)
    {
        pos = _pos;
        cellVertices = new List<Vector3>();
        avregeVertex = Vector3.zero;
    }

}
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MeshVolumiques : MonoBehaviour
{
    public GameObject voxelPrefab;
    public int resolution;
    public List<Sphere> meshs;
    private Node tree;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Sphere sphere in meshs) 
            sphere.center += transform.position;

        tree = new Node(transform.position,20,0);
        SphereNode(tree);
        NodeDisplay(tree);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void NodeDisplay(Node node)
    {
        if(node.voxel)
        {
            GameObject voxel = Instantiate(voxelPrefab, node.position, new Quaternion(0, 0, 0, 0),transform);
            voxel.transform.localScale = node.size * Vector3.one;
            return;
        }

        if (node.leaf)
            return;

        foreach (Node child in node.childs)
            NodeDisplay(child);

    }

    private void SphereNode(Node node)
    {
        foreach(Sphere sphere in meshs)
        {
            float sphereDist = Vector3.Distance(node.position, sphere.center);

            //interieur
            if (sphereDist + node.size * Mathf.Sqrt(3f) * 0.5f < sphere.radius)
            {
                node.leaf = true;
                node.voxel = true;
                node.childs = null;
                return;
            }
        }

        foreach (Sphere sphere in meshs)
        {
            float sphereDist = Vector3.Distance(node.position, sphere.center);
            //exterieur
            if (sphereDist - node.size * Mathf.Sqrt(3f) * 0.5f > sphere.radius)
            {
                node.leaf = true;
                node.voxel = false;
                node.childs = null;
                return;
            }
        }

        if (node.depth>= resolution)
            return;

        NodeSubdivide(node);

        foreach(Node child in node.childs)
            SphereNode(child);
    }

    private void NodeSubdivide(Node node)
    {
        node.leaf = false;
        node.childs = new Node[8];
        float childSize = node.size / 2;

        int childindex = 0;
        for(int x =0; x<2; x++)
            for (int y = 0; y < 2; y++)
                for (int z= 0; z < 2; z++)
                {
                    Vector3 childPos = node.position + new Vector3((x-0.5f)*childSize, (y - 0.5f) * childSize, (z - 0.5f) * childSize);
                    node.childs[childindex] = new Node(childPos, childSize,node.depth+1);
                    
                    childindex++;
                }

    }
    private void OnDrawGizmosSelected()
    {
        if (tree == null)
            return;

        DrawGizmosNode(tree);
    }

    private void DrawGizmosNode(Node node)
    {
        if (node == null)
            return;

        if (node.leaf)
        {
            if (!node.voxel)
                Gizmos.DrawWireCube(node.position, Vector3.one * node.size);
           
            return;
        }
        else
            foreach (var child in node.childs)
                DrawGizmosNode(child);

    }
}

[System.Serializable]
public class Node
{
    public bool leaf;
    public bool voxel;
    public Vector3 position;
    public float size;
    public int depth;
    public Node[] childs;



    public Node(Vector3 pos, float s, int d)
    {
        position = pos;
        size = s;
        depth = d;
        leaf = true;
    }

}

[System.Serializable]
public class Sphere
{
    public Vector3 center;
    public float radius;
}

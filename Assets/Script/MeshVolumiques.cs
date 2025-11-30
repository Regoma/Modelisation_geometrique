using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum Operation
{
    Union,
    Intersection
}

public class MeshVolumiques : MonoBehaviour
{
    [SerializeField] private Operation operation;
    [SerializeField] private float size;
    [SerializeField] private float cubesize;
    [SerializeField] private GameObject voxelPrefab;
    [SerializeField] private int resolution;
    [SerializeField] private List<Sphere> meshs;
    [SerializeField] private Transform brush;
    [SerializeField] private float brushWeight;
    private Node tree;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        foreach (Sphere sphere in meshs) 
            sphere.center += transform.position;

        tree = new Node(transform.position,size,0);
        SphereNode(tree);
        NodeDisplay(tree);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Transform c in GetComponentInChildren<Transform>())
            {
                Destroy(c.gameObject);
            }
            foreach (Sphere sphere in meshs)
                sphere.center += transform.position;

            tree = new Node(transform.position, size, 0);
            SphereNode(tree);
            NodeDisplay(tree);
        }

        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(r,out RaycastHit hit, 50f))
        {
            Debug.Log(hit.point);
            brush.transform.position = hit.point;

            if (Input.GetMouseButton(0))  // Clique pour peindre
            {
                PaintWithBrush(tree);

                // Regénère la display
                foreach (Transform c in transform)
                    Destroy(c.gameObject);

                NodeDisplay(tree);
            }
        }
    }


    private void NodeDisplay(Node node)
    {
        if (node.voxel)
        {
            GameObject voxel = Instantiate(voxelPrefab, node.position, new Quaternion(0, 0, 0, 0),transform);
            voxel.transform.localScale = node.size * Vector3.one * cubesize;
            return;
        }

        if (node.leaf)
            return;

        foreach (Node child in node.childs)
            NodeDisplay(child);

    }
    private void SphereNode(Node node)
    {
        float halfDiag = node.size * Mathf.Sqrt(3f) * 0.5f;

        bool insideAtLeastOne = false;
        bool outsideAll = true;
        bool insideAll = true;
        bool outsideAny = false;

        if(operation == Operation.Union)
        {
            foreach (Sphere sphere in meshs)
            {
                float dist = Vector3.Distance(node.position, sphere.center);


                if (dist + halfDiag < sphere.radius)
                    insideAtLeastOne = true;


                if (!(dist - halfDiag > sphere.radius))
                    outsideAll = false;

            }
            if (insideAtLeastOne)
            {
                node.leaf = true;

                node.voxel = true;
                node.childs = null;
                return;
            }
            if (outsideAll)
            {
                node.leaf = true;
                node.voxel = false;
                node.childs = null;
                return;
            }
        }
        else if (operation == Operation.Intersection)
        {
            foreach(Sphere sphere in meshs)
            {
                float dist = Vector3.Distance(node.position, sphere.center);

                if (!(dist + halfDiag < sphere.radius))
                    insideAll = false;


                if (dist - halfDiag > sphere.radius)
                    outsideAny=true;

            }

            if (outsideAny)
            {
                node.leaf = true;
                node.voxel = false;
                node.childs = null;
                return;
            }

            if (insideAll)
            {
                node.leaf = true;
                node.voxel = true;
                node.childs = null;
                return;
            }

        }     
        if (node.depth >= resolution)
            return;

        NodeSubdivide(node);

        foreach (Node child in node.childs)
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

    public void PaintWithBrush(Node node)
    {
        float halfDiag = node.size * Mathf.Sqrt(3f) * 0.5f;
        float brushRadius = brush.localScale.x * 0.5f;
        float dist = Vector3.Distance(node.position, brush.position);

        if (dist > brushRadius + halfDiag)
            return;

        if (node.leaf && node.depth < resolution)
        {
            NodeSubdivide(node);
        }


        if (node.leaf && node.depth >= resolution)
        {

            float attenu = 1- (dist / brushRadius);
            node.weight += (brushWeight * attenu);

            if (node.weight > 1f)
                node.voxel = true;
            return;
        }




        foreach (Node child in node.childs)
            PaintWithBrush(child);
    }


    private void OnDrawGizmosSelected()
    {
        if (tree == null)
            return;

        DrawGizmosNode(tree);

        foreach (Sphere sphere in meshs)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sphere.center,sphere.radius);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color= Color.red;
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Gizmos.DrawLine(pos, pos+ Camera.main.transform.forward *50f);
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
            foreach (Node child in node.childs)
                DrawGizmosNode(child);

    }
}

public class Node
{
    public bool leaf;
    public bool voxel;
    public Vector3 position;
    public float size;
    public int depth;
    public Node[] childs;
    public float weight = 0;


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

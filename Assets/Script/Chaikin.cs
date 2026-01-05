using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Chaikin : MonoBehaviour
{
    [SerializeField][Range(0,10)] private int iteration = 1;
    [Space]
    [SerializeField] private float verticesGizmosSize = 0.05f;
    [SerializeField] private List<Vector3> vertices = new List<Vector3>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        if (transform.childCount <= 2)
            return;

        vertices.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            vertices.Add(transform.GetChild(i).position);
        }

        for (int sub = 0; sub < iteration; sub++)
        {
            List<Vector3> oldVertices = new List<Vector3>(vertices);
            vertices.Clear();
            for (int i = 0; i < oldVertices.Count - 1; i++)
            {
                Vector3 edge = oldVertices[i + 1] - oldVertices[i];
                Vector3 v1 = edge * 0.25f + oldVertices[i];
                Vector3 v2 = edge * 0.75f + oldVertices[i];
                vertices.Add(v1);
                vertices.Add(v2);
            }
            Vector3 _edge = oldVertices[0] - oldVertices[oldVertices.Count - 1];
            Vector3 _v1 = _edge * 0.25f + oldVertices[oldVertices.Count - 1];
            Vector3 _v2 = _edge * 0.75f + oldVertices[oldVertices.Count - 1];
            vertices.Add(_v1);
            vertices.Add(_v2);
        }


    }

    private void OnDrawGizmos()
    {
        if (transform.childCount <= 2)
            return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < transform.childCount-1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i +1).position);
            Gizmos.DrawCube(transform.GetChild(i).position, Vector3.one * verticesGizmosSize);
        }
        Gizmos.DrawLine(transform.GetChild(transform.childCount -1).position, transform.GetChild(0).position);
        Gizmos.DrawCube(transform.GetChild(transform.childCount - 1).position, Vector3.one * verticesGizmosSize);

        if (vertices.Count <= 2)
            return;
        Gizmos.color = Color.blue;
        for (int j = 0; j < vertices.Count - 1; j++)
        {
            Gizmos.DrawLine(vertices[j], vertices[j+1]);
            Gizmos.DrawCube(vertices[j], Vector3.one * verticesGizmosSize);
        }
        Gizmos.DrawLine(vertices[0], vertices[vertices.Count-1]);
        Gizmos.DrawCube(vertices[vertices.Count - 1], Vector3.one * verticesGizmosSize);
    }
}

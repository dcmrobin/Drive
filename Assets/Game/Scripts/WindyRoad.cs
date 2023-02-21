using UnityEngine;
using System.Collections.Generic;

public class WindyRoad : MonoBehaviour
{
    public Material roadMaterial;
    public int roadWidth = 10;
    public float minTurnRadius = 20f;
    public float maxTurnRadius = 50f;
    public int numTurns = 10;

    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        GenerateRoad();
    }
    /*private void OnGUI() {
        if(GUILayout.Button("Generate"))
        {
            GenerateRoad();
        }
    }*/

    void GenerateRoad()
    {
        // Start at the origin
        Vector3 currentPosition = Vector3.zero;
        points.Add(currentPosition);

        // Generate a series of turns
        for (int i = 0; i < numTurns; i++)
        {
            // Determine the angle of the turn
            float turnAngle = Random.Range(-90f, 90f);

            // Determine the radius of the turn based on the road width
            float minRadius = roadWidth * 2f;
            float turnRadius = Random.Range(minRadius, maxTurnRadius);
            float minTurnRadius = Mathf.Max(minRadius, turnRadius / 2f);

            // Calculate the center of the turn
            Vector3 center;
            if (points.Count < 2)
            {
                center = currentPosition + Quaternion.AngleAxis(turnAngle, Vector3.up) * Vector3.forward * turnRadius;
            }
            else
            {
                bool intersects;
                do
                {
                    center = currentPosition + Quaternion.AngleAxis(turnAngle, Vector3.up) * (currentPosition - points[points.Count - 2]).normalized * turnRadius;
                    intersects = false;
                    for (int j = 1; j < points.Count - 1; j++)
                    {
                        if (Vector3.Distance(center, points[j]) < turnRadius)
                        {
                            intersects = true;
                            break;
                        }
                    }
                    if (intersects)
                    {
                        turnAngle = Random.Range(-90f, 90f);
                        turnRadius = Random.Range(minTurnRadius, maxTurnRadius);
                    }
                } while (intersects);
            }

            // Generate points along the turn
            int numSegments = (int)Mathf.Ceil(Mathf.Abs(turnAngle) / 5f);
            float angleStep = turnAngle / numSegments;
            for (int j = 1; j <= numSegments; j++)
            {
                float angle = j * angleStep;
                Vector3 point = center + Quaternion.AngleAxis(angle, Vector3.up) * (currentPosition - center).normalized * turnRadius;
                points.Add(point);
            }

            // Update current position
            currentPosition = points[points.Count - 1];
        }

        // Generate mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Count * 2];
        int[] triangles = new int[(points.Count - 1) * 6];

        for (int i = 0; i < points.Count; i++)
        {
            // Calculate the position of the vertices
            Vector3 offset = Quaternion.AngleAxis(90f, Vector3.up) * (points[i] - currentPosition).normalized * roadWidth / 2f;
            vertices[i * 2] = points[i] + offset;
            vertices[i * 2 + 1] = points[i] - offset;

            // Generate triangles
            if (i > 0)
            {
                int triIndex = (i - 1) * 6;
                triangles[triIndex] = i * 2 - 2;
                triangles[triIndex + 1] = i * 2 - 1;
                triangles[triIndex + 2] = i * 2;
                triangles[triIndex + 3] = i * 2;
                triangles[triIndex + 4        ] = i * 2 - 1;
                triangles[triIndex + 5] = i * 2 + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Set up game object
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.material = roadMaterial;
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

    }
}

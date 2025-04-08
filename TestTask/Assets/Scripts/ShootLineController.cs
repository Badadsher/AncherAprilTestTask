using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLineController : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float maxLineLength = 10f;
    [SerializeField] private int linePoints = 20;

    void Update()
    {
        UpdateAimLine();
    }

    void UpdateAimLine()
    {
        Vector3[] points = new Vector3[linePoints];
        Vector2 direction = playerTransform.right; 

        for (int i = 0; i < linePoints; i++)
        {
            float time = i / (float)linePoints;
            Vector2 point = (Vector2)playerTransform.position + direction * time * maxLineLength;
            points[i] = point;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}

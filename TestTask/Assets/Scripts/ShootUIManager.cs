using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootUIManager : MonoBehaviour
{
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private int numberOfPoints = 30;
    [SerializeField] private float timeStep = 0.1f;
    [SerializeField] private Transform player;

    private List<GameObject> points = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            GameObject dot = Instantiate(pointPrefab);
            dot.SetActive(false);
            points.Add(dot);
        }
    }

    public void UpdateTrajectory(Vector2 startPos, Vector2 velocity)
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i * timeStep;
            Vector2 pos = startPos + velocity * t + 0.5f * Physics2D.gravity * t * t;

            points[i].transform.position = pos;
            points[i].SetActive(true);
        }
    }

    public void ClearTrajectory()
    {
        foreach (var point in points)
            point.SetActive(false);
    }

    public void RotatePlayer(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        player.rotation = Quaternion.Euler(0, 0, angle);

        // Тут можешь триггерить анимации Spine или Animator
        // skeletonAnimation.skeleton.SetBoneRotation("Spine", angle); // если Spine
    }

    public void PlayShootAnimation()
    {
        // animator.SetTrigger("Shoot"); или через Spine
    }
}

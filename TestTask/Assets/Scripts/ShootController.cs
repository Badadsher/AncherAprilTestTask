using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour
{
     [SerializeField] private Transform shootPoint;
     [SerializeField] private GameObject arrowPrefab;
     [SerializeField] private float power = 10f;
 
     private Vector2 startPoint;
     private bool isAiming = false;
     private ShootUIManager uiManager;
 
     void Start()
     {
         uiManager = FindObjectOfType<ShootUIManager>();
     }
 
     void Update()
     {
         if (Input.GetMouseButtonDown(0))
         {
             startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
             isAiming = true;
         }
 
         if (Input.GetMouseButton(0) && isAiming)
         {
             Vector2 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
             Vector2 direction = startPoint - currentPoint;
 
             uiManager.UpdateTrajectory(shootPoint.position, direction * power);
             uiManager.RotatePlayer(direction);
         }
 
         if (Input.GetMouseButtonUp(0) && isAiming)
         {
             Vector2 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
             Vector2 force = (startPoint - endPoint) * power;
 
             Shoot(force);
             uiManager.ClearTrajectory();
             isAiming = false;
         }
     }
 
     void Shoot(Vector2 force)
     {
         GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
         Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
         rb.velocity = force;
 
         uiManager.PlayShootAnimation();
     }
}

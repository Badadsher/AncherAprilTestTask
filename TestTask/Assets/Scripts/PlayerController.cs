using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;
    
    [Header("Bones")]
    [SerializeField] private string headBoneName = "head";
    [SerializeField] private string bodyBoneName = "body";
    
    [Header("Arrow Settings")]
    [SerializeField] private float maxPullDistance = 5f;
    [SerializeField] private float arrowRotationOffset = -90f; 
    
    [Header("Rotation Limits")]
    [SerializeField] private float headMaxRotation = 40f;
    [SerializeField] private float bodyMaxRotation = 35f;

    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothness = 8f;

    [Header("Trajectory Settings")]

    [SerializeField] private int trajectoryResolution = 10;
    [SerializeField] private float maxTrajectoryTime = 3f; 
    [SerializeField] private float pullForceMultiplier = 3f;
    [SerializeField] private float maxTrajectoryHeight = 5f; 
    [SerializeField] private float gravityScale = 0.8f;     
    [SerializeField] private float minPower = 15f;         
    [SerializeField] private float maxPower = 50f;        
    [SerializeField] private float powerCurve = 1.5f;   
    
    [Header("Vertical Aim Settings")]
    [SerializeField] private float minVerticalAngle = -0.2f;
    [SerializeField] private float maxVerticalAngle = 0.5f; 
    [SerializeField] private float verticalAimSensitivity = 0.2f;
    
    [Header("Shot Line")]
    [SerializeField] private GameObject dotPrefab; 
    [SerializeField] private int dotsCount = 10;
    [SerializeField] private float dotSpacing = 0.5f; 
    [SerializeField] private Transform firePoint; 
    [SerializeField] private float maxLineLength = 5f; 
    [SerializeField] private float pullMultiplier = 2f; 
    
    [Header("Arrow")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed = 10f;
    
    [Header("Animations")]
    [SerializeField] private string aimAnimation = "attack_target";
    [SerializeField] private string shootAnimation = "attack_finish";
    [SerializeField] private string idleAnimation = "idle";
    [SerializeField] private float animationTransitionSpeed = 0.1f;

    private bool isAiming = false;
    
    private Vector3 initialMousePosition;
    private float currentPullDistance;
    private float currentPullDistanceNormalized;
    
    private Bone headBone;
    private Bone bodyBone;
    
    private List<GameObject> dots = new List<GameObject>();
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    
    private bool isMouseDown = false;
    private Vector3 lastAimDirection;
    private float lastPower;
    
    private Vector2 currentShootDirection; 
    private float currentShootPower;       

    private void Start()
    {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        headBone = skeletonAnimation.Skeleton.FindBone(headBoneName);
        bodyBone = skeletonAnimation.Skeleton.FindBone(bodyBoneName);
        
        skeletonAnimation.Skeleton.ScaleX = 1;
        bodyBone.Rotation = 0;
        
        for (int i = 0; i < dotsCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, firePoint.position, Quaternion.identity);
            dot.SetActive(false);
            dots.Add(dot);
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateRotation();
        
        if (isMouseDown)
        {
            UpdateDots();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            isAiming = true;
            initialMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PlayAnimation(aimAnimation, true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isMouseDown)
            {
                isAiming = false;
                PlayAnimation(shootAnimation, false);
                ShootArrow();
                HideDots();
                skeletonAnimation.AnimationState.SetEmptyAnimation(0, 0);
            }
            isMouseDown = false;
        }
    }

    private void PlayAnimation(string animationName, bool loop)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
    }
    

    private void UpdateRotation()
    {
        if (headBone == null) return;
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 direction = (mousePosition - transform.position).normalized;
        direction.y = -direction.y;
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float headTargetAngle = Mathf.Clamp(targetAngle, -headMaxRotation, headMaxRotation);
        float bodyTargetAngle = Mathf.Clamp(targetAngle, -bodyMaxRotation, bodyMaxRotation);

        ApplyBoneRotation(headBone, headTargetAngle);
        ApplyBoneRotation(bodyBone, bodyTargetAngle);
        
        skeletonAnimation.Skeleton.UpdateWorldTransform();
    }

    private void UpdateDots()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector2 pullVector = initialMousePosition - mousePos;
        
        currentShootDirection = Vector2.right;

        float pullDistance = Mathf.Clamp(pullVector.magnitude, 0, maxPullDistance);
        currentPullDistanceNormalized = pullDistance / maxPullDistance;
        
        currentShootDirection.y = Mathf.Clamp(pullVector.y * verticalAimSensitivity, minVerticalAngle, maxVerticalAngle);
        currentShootDirection.Normalize();
        
        currentShootPower = Mathf.Lerp(minPower, maxPower, Mathf.Pow(currentPullDistanceNormalized, powerCurve));
        
        Vector2 gravity = Physics2D.gravity * gravityScale;

        for (int i = 0; i < dotsCount; i++)
        {
            float t = i / (float)(dotsCount - 1) * maxTrajectoryTime;
            Vector2 point = (Vector2)firePoint.position +
                            currentShootDirection * currentShootPower * t +
                            0.5f * gravity * t * t;

            if (i < dots.Count)
            {
                dots[i].SetActive(true);
                dots[i].transform.position = point;
            }
        }
    }

    private void ShootArrow()
    {
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.gravityScale = 0;
            StartCoroutine(EnableArrowGravity(rb, 0.2f));
            
            rb.velocity = currentShootDirection * currentShootPower;
            
            float angle = Mathf.Atan2(currentShootDirection.y, currentShootDirection.x) * Mathf.Rad2Deg + arrowRotationOffset;
            arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private IEnumerator EnableArrowGravity(Rigidbody2D rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            rb.gravityScale = gravityScale;
        }
    }

   
    private void ApplyBoneRotation(Bone bone, float targetAngle)
    {
        float smoothedAngle = Mathf.LerpAngle(
            bone.Rotation,
            targetAngle,
            Time.deltaTime * rotationSmoothness
        );
        bone.Rotation = smoothedAngle;
    }

    private void HideDots()
    {
        foreach (GameObject dot in dots)
        {
            dot.SetActive(false);
        }
    }
}
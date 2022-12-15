using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using static UnityEditor.PlayerSettings;

public class Brandon_PlayerMovement : MonoBehaviour
{
    private Transform playerModel;

    [Header("Settings")]
    public bool joystick = true;

    [Space]

    [Header("Parameters")]
    public float xySpeed = 18;
    public float lookSpeed = 340;
    public float forwardSpeed = 100;

    // Ability
    public float boostTimer; // Timer for boosting
    public float boostCD = 10; // Boost Cool Down 
    private bool boosting = false;
    private bool canBoost = false;

    // Waypoints
    public Transform destination;
    private Rigidbody rb;
    public Camera shipCam;
    public float turningSpeed = 1f;
    public int forwardForce = 1; // Forward Force
    public int movementForce = 1; // Movement Force



    [Space]

    [Header("Public References")]
    public Transform aimTarget;
    public CinemachineDollyCart dolly;
    public Transform cameraParent;

    [Space]

    [Header("Particles")]
    public ParticleSystem trail;
    public ParticleSystem circle;
    public ParticleSystem barrel;
    public ParticleSystem stars;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerModel = transform.GetChild(0);
        SetSpeed(forwardSpeed);
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * forwardForce);
    }

    void Update()
    {
        // Add Force Movement using control physics
        /*if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(transform.up * movementForce);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-transform.right * movementForce);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(-transform.up * movementForce);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(transform.right * movementForce);
        }*/
        // Waypoint Movement Start
        /*Vector3 lookPos = destination.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turningSpeed);*/

        // Waypoint Movement End

        float h = joystick ? Input.GetAxis("Horizontal") : Input.GetAxis("Mouse X");
        float v = joystick ? Input.GetAxis("Vertical") : Input.GetAxis("Mouse Y");

        LocalMove(h, v, xySpeed);
        RotationLook(h,v, lookSpeed);
        HorizontalLean(playerModel, h, 80, .1f);

        //Boost Cool Down
        boostCD -= Time.deltaTime;
        if (boostCD <= 0)
        {
            canBoost = true;
        }
        
        // Press button to boost
        if (canBoost && Input.GetButtonDown("Action"))
        { 
            boostTimer += Time.deltaTime;
            boosting = true;
            StartBoost();
        }
        // hold button to boost
        if (Input.GetButton("Action") && boostCD <= 0)
        {
            boostTimer += Time.deltaTime;
            // used if player is holding down boost button to boost
            if (boostTimer >= 5)
            {
                boosting = false;
                Boost(false);
            }
        }

        // This can be used if we want the player to be able to hold down for boost
        if (Input.GetButtonUp("Action"))
             Boost(false);

        if (Input.GetButtonDown("Fire3"))
            Break(true);

        if (Input.GetButtonUp("Fire3"))
            Break(false);

        if (Input.GetButtonDown("TriggerL") || Input.GetButtonDown("TriggerR"))
        {
            int dir = Input.GetButtonDown("TriggerL") ? -1 : 1;
            QuickSpin(dir);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        // Add Force to the player if triggered
        if (collider.CompareTag("object"))
        {
            movementForce = 50000;
            rb.AddForce(-transform.forward * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.D))
        {
            movementForce = 50000;
            rb.AddForce(-transform.right * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.W))
        {
            movementForce = 50000;
            rb.AddForce(-transform.up * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.A))
        {
            movementForce = 50000;
            rb.AddForce(transform.right * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.S))
        {
            movementForce = 50000;
            rb.AddForce(transform.up * movementForce);
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("object"))
        {
            rb.velocity = Vector3.zero;
            movementForce = 0;
        }
        if (collider.CompareTag("PlayerPos"))
        {
            playerModel.transform.position = destination.transform.position;
        }
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float p)
    {
        Vector3 result = new Vector3();

        result.x = Lerp(a.x, b.x, p);
        result.y = Lerp(a.y, b.y, p);
        result.z = Lerp(a.z, b.z, p);

        return result;
    }

    public static float Lerp(float a, float b, float p)
    {
        return (b - a) * p + a;
    }

    void LocalMove(float x, float y, float speed)
    {
        transform.localPosition += new Vector3(x, y, 0) * speed * Time.deltaTime;
        ClampPosition();
    }

    void ClampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Lerp(transform.position, Camera.main.ViewportToWorldPoint(pos), .05f);
    }

    void RotationLook(float h, float v, float speed)
    {
        aimTarget.parent.position = Vector3.zero;
        aimTarget.localPosition = new Vector3(h, v, 1);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(aimTarget.position), Mathf.Deg2Rad * speed * Time.deltaTime);
    }

    void HorizontalLean(Transform target, float axis, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngels = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngels.x, targetEulerAngels.y, Mathf.LerpAngle(targetEulerAngels.z, -axis * leanLimit, lerpTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(aimTarget.position, .5f);
        Gizmos.DrawSphere(aimTarget.position, .15f);

    }

    public void QuickSpin(int dir)
    {
        if (!DOTween.IsTweening(playerModel))
        {
            playerModel.DOLocalRotate(new Vector3(playerModel.localEulerAngles.x, playerModel.localEulerAngles.y, 360 * -dir), .4f, RotateMode.LocalAxisAdd).SetEase(Ease.OutSine);
            barrel.Play();
        }
    }

    void SetSpeed(float x)
    {
        dolly.m_Speed = x;
    }

    void SetCameraZoom(float zoom, float duration)
    {
        cameraParent.DOLocalMove(new Vector3(0, 0, zoom), duration);
    }

    void DistortionAmount(float x)
    {
        Camera.main.GetComponent<PostProcessVolume>().profile.GetSetting<LensDistortion>().intensity.value = x;
    }

    void FieldOfView(float fov)
    {
        cameraParent.GetComponentInChildren<CinemachineVirtualCamera>().m_Lens.FieldOfView = fov;
    }

    void Chromatic(float x)
    {
        Camera.main.GetComponent<PostProcessVolume>().profile.GetSetting<ChromaticAberration>().intensity.value = x;
    }

    // Help keep track of boost this will start the Boost method and set a timer to stop the boost
    void StartBoost()
    {
        Boost(true);

        Invoke("EndBoost", 5);
    }

    // This will call after the timer is up and stop the boost and reset timer
    void EndBoost()
    {
        Boost(false);
        boostTimer = 0;
        boostCD = 10;
        boosting = false;
        canBoost = false;
    }
    void Boost(bool state)
    {
        if (boosting && boostCD <= 0)
        {

            if (state)
            {
                cameraParent.GetComponentInChildren<CinemachineImpulseSource>().GenerateImpulse();
                trail.Play();
                circle.Play();
            }
            else
            {
                trail.Stop();
                circle.Stop();
            }

            trail.GetComponent<TrailRenderer>().emitting = state;

            float origFov = state ? 40 : 55;
            float endFov = state ? 55 : 40;
            float origChrom = state ? 0 : 1;
            float endChrom = state ? 1 : 0;
            float origDistortion = state ? 0 : -30;
            float endDistorton = state ? -30 : 0;
            float starsVel = state ? -20 : -1;
            float speed = state ? forwardSpeed * 2f : forwardSpeed; // Speed of boost
            float zoom = state ? -7 : 0; // how far camera zooms

            DOVirtual.Float(origChrom, endChrom, .5f, Chromatic);
            DOVirtual.Float(origFov, endFov, .5f, FieldOfView);
            DOVirtual.Float(origDistortion, endDistorton, .5f, DistortionAmount);
            var pvel = stars.velocityOverLifetime;
            pvel.z = starsVel;

            DOVirtual.Float(dolly.m_Speed, speed, .15f, SetSpeed);
            SetCameraZoom(zoom, .4f);
        }

        // This setting is used if the player will be holding down the boost button
        /*if (boostTimer >= 5)
        {
            boostTimer = 0;
            boostCD = 10;
            boosting = false;
            canBoost = false;
        }*/
    }

    void Break(bool state)
    {
        float speed = state ? forwardSpeed / 10 : forwardSpeed;
        float zoom = state ? 3 : 0;

        DOVirtual.Float(dolly.m_Speed, speed, .15f, SetSpeed);
        SetCameraZoom(zoom, .4f);
    }
}

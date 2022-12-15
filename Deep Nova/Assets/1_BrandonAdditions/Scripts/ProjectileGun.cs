using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ProjectileGun : MonoBehaviour
{

    // Bullet
    public GameObject bullet;

    // Bullet Force
    public float shootForce, upwardForce;

    // Gun Stats
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;

    // Bools
    bool shooting, readyToShoot, reloading;

    // Reference
    public Camera shipCam;
    public Transform attackPoint;
    public LayerMask layers;

    // Graphics
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    public bool allowInvoke = true;

    // Getting Camera for locating mouse position
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        // Make Sure Magazine is Full
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    void Update()
    {
        MyInput();

        // Set ammo display (if it exists)
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
    }

    private void MyInput()
    {
        // Check if allowed to hold down button and take corresponding input
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        // Reload automatically when trying to shoot without ammo
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        // Shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            // Set Bullets Shot to 0
            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot()
    {
        // Test for bullet hitting different layers
        Collider[] hitLayers = Physics.OverlapSphere(attackPoint.position, 5f, layers);

        readyToShoot = false;

        // Find the exact hit position using a raycast
        //Ray ray = shipCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f)); // Just a ray through the middle of your screen
        //RaycastHit hit;

        // locating mouse position in the 3D world sence
        //Ray ray = mainCamera.ScreenPointToRay(attackPoint.position);
        //if (Physics.Raycast(ray, out RaycastHit hit))
        //{
        //  transform.position = hit.point;
        //}

        // Check if Ray hits something
        //Physics.Raycast(ray, out hit, 100000, layers);

        //if (Physics.Raycast(ray, out hit)){
        //    targetPoint = hit.point;
        //}
        //else
        //    targetPoint = ray.GetPoint(1000);

        // Calculate direction from attackPoint to targetPoint
        Vector3 targetPoint;
        RaycastHit hit;

        if (Physics.Raycast(muzzleFlash.transform.position, muzzleFlash.transform.forward, out hit, 100000))
            Debug.Log(hit.transform.name);

        {
            //Target target = hit.transform.GetComponent<Target>();
        }

        targetPoint = hit.point;
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;
        // Calculate Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate new direction with spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0); // Just add spread to last direction

        // Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        // Rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread.normalized;

        // Add forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(attackPoint.transform.up * upwardForce, ForceMode.Impulse);

        // Instantiate muzzle flash (if there is one)
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

        // Invoke reset function (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        // if more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }

    }

    private void ResetShot()
    {
        // Allow shooting and invoking again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private void OnCollisionEnter(Collision col)
    {
        
    }

}

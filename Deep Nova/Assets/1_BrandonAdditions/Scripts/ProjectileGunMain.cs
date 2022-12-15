using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProjectileGunMain : MonoBehaviour
{

    // Bullet
    public GameObject bullet;

    // Bullet Force
    public float shootForce, upwardForce;

    // Gun Stats
    //public float damage = 10f,
    public float range = 100f;
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;


    int bulletsLeft, bulletsShot;

    // Bools
    bool shooting, readyToShoot, reloading;

    // Reference
    public Camera playerCam;
    public Transform attackPoint;
    public Transform aimPoint;
    public RaycastHit rayHit;
    public LayerMask layers;

    // Graphics
    public GameObject muzzleFlash, lazerBurn;
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
        Vector3 direction = Vector3.forward;
        new Ray(transform.position, transform.TransformDirection(direction * range));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction * range));
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
        // Reload automatically when player runs out of ammo
        if (readyToShoot && !shooting && !reloading && bulletsLeft <= 0) Reload();
        // Reload automatically when player is shooting and out of ammo
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

        readyToShoot = false;

        // Mouse position with ray through the middle of your screen
        // Find the exact hit position using a raycast
        Vector3 direction = Vector3.forward;
        Ray ray = new Ray(transform.position, transform.TransformDirection(direction * range));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction * range));
        RaycastHit rayHit;

        // Check if Ray hits something
        Vector3 targetPoint;

        // For center screen Mouse
        if (Physics.Raycast(ray, out rayHit, range))
        {
            Debug.Log(rayHit.transform.name);
            targetPoint = rayHit.point;
        }
        else
            targetPoint = ray.GetPoint(75);

        // Calculate direction from attackPoint to targetPoint
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        // Calculate Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);

        // Calculate new direction with spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, z); // Just add spread to last direction

        // Instantiate bullet/projectile
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, bullet.transform.rotation);
        // Rotate bullet to shoot direction
        currentBullet.transform.forward = directionWithSpread.normalized;

        // Add forces to bullet
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * upwardForce, ForceMode.Impulse);

        // Test for bullet hitting different layers
        // This will damage enemies that are inside the attack point for the bullet instead of raycasting.
        //Collider[] hitLayers = Physics.OverlapSphere(attackPoint.transform.position, 5f, layers);
        /*foreach(Collider enemy in hitLayers)
        {
            B_FattStacks target = enemy.GetComponent<B_FattStacks>();
            if (target != null){
                target.TakeDamage(damage);
            }
        }*/

        // Instantiate muzzle flash (if there is one)
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        if (lazerBurn != null)     
            Instantiate(lazerBurn, rayHit.point, Quaternion.Euler(0,180,0));

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

        //Destroy(currentBullet, 2f);

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

using UnityEngine;

public class LazerBeam : MonoBehaviour
{
    // Assignables
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemy;

    // Stats
    [Range (0f, 1f)]
    public float bounciness;
    public bool useGravity;

    // Damage
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    // Lifetime
    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    int collisions;
    PhysicMaterial physic_mat;

    private void Start() 
    {
        Setup();
    }

    private void Update()
    {
        // When to Explode
        if (collisions > maxCollisions) Explode();

        // Count down lifetime
        maxLifetime = Time.deltaTime;
        if (maxLifetime <= 0) Explode();
    }

    private void Explode()
    {
        // Instantiate explosion
        if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);

        // Check for enemies
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemy);
        for (int i = 0; i < enemies.Length; i++)
        {
            // Get the component of the enemy and call take Damage
            //enemies[i].GetComponent<B_FattStacks>().TakeDamage(explosionDamage);
            if (enemies[i].GetComponent<Rigidbody>())
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
        }
        // Add a delay in case of bugs
        Invoke("Delay", 0.05f);
    }
    
    private void Delay()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Don't count collisions with other bullets
        //if (collision.collider.CompareTag("Bullet")) return;

        // Count up collisions
        collisions++;

        // Explode if bullethits an enemy directly and explodeOnTouch is activated
        //if (collision.collider.CompareTag("Enemy") && explodeOnTouch) Explode();
        {

        }
    }

    private void Setup()
    {
        // Create a new Physics material
        physic_mat = new PhysicMaterial();
        physic_mat.bounciness = bounciness;
        physic_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physic_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        // Assign material to collider
        GetComponent<SphereCollider>().material = physic_mat;

        // Set Gravity
        rb.useGravity = useGravity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, explosionRange);
    }
}
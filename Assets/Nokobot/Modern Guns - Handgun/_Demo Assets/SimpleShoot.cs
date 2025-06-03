using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Nokobot/Modern Guns/Simple Shoot")]
public class SimpleShoot : MonoBehaviour
{   
    public int maxAmmo = 10; // Maximum ammo in the gun
    private int currentAmmo; // Current ammo in the gun

    [Header("Prefab Refrences")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location Refrences")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [Tooltip("Specify time to destory the casing object")] [SerializeField] private float destroyTimer = 2f;
    [Tooltip("Bullet Speed")] [SerializeField] private float shotPower = 500f;
    [Tooltip("Casing Ejection Speed")] [SerializeField] private float ejectPower = 150f;
    [Tooltip("Line width")] [SerializeField] private float lineWidth = 0.5f;
    [Tooltip("Line duration")] [SerializeField] private float lineDuration = 0.5f;
    [Tooltip("Line color")] [SerializeField] private Color lineColor = Color.yellow;

    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;

        if (gunAnimator == null)
            gunAnimator = GetComponentInChildren<Animator>();

        Reload();
    }

    void Reload()
    {
        // Reset current ammo to max ammo
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        //If you want a different input, change it here
        
        if (Vector3.Angle(transform.up, Vector3.up) > 100 && currentAmmo < maxAmmo)
        {
            // If the gun is tilted too much, reload automatically
            Reload();
        }
        if (Input.GetButtonDown("Fire1") && Vector3.Angle(transform.up, Vector3.up) < 100){
            if (currentAmmo > 0)
            {
                gunAnimator.SetTrigger("Fire");
            }
            else
            {
                Debug.Log("Out of ammo!");
            }
        }
    }

    //This function creates the bullet behavior
    void Shoot()
    {
        //cancels if there's no bullet prefeb
        if (!bulletPrefab || currentAmmo <= 0)
        return;

        currentAmmo--; // Reduce ammo here
        Debug.Log("Ammo remaining: " + currentAmmo);

        // Create and fire the bullet
        Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation).GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);

        if (muzzleFlashPrefab)
        {
            //Create the muzzle flash
            GameObject tempFlash;
            tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);
            
            //Destroy the muzzle flash effect
            Destroy(tempFlash, destroyTimer);
        }

        // Create tracer line effect dynamically
        CreateTracerLine();
    }

    void CreateTracerLine()
    {
        // Raycast to detect hit
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(barrelLocation.position, barrelLocation.forward, out hitInfo, 100f);
        
        // Create line dynamically
        GameObject liner = new GameObject("TracerLine");
        LineRenderer lineRenderer = liner.AddComponent<LineRenderer>();
        
        // Use a simpler, more reliable shader
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Set colors using startColor and endColor
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        
        // Configure other properties with better visibility settings
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        // Ensure it renders properly
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.allowOcclusionWhenDynamic = false;
        
        Vector3 startPoint = barrelLocation.position;
        Vector3 endPoint = hasHit ? hitInfo.point : barrelLocation.position + barrelLocation.forward * 100f;
        
        // Add slight offset to start point to avoid clipping
        startPoint += barrelLocation.forward * 0.1f;
        
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        
        // Debug to check line length
        Debug.Log($"Tracer line from {startPoint} to {endPoint}, distance: {Vector3.Distance(startPoint, endPoint)}");
        
        // Destroy the line after specified duration
        Destroy(liner, lineDuration);
    }

    //This function creates a casing at the ejection slot
    void CasingRelease()
    {
        //Cancels function if ejection slot hasn't been set or there's no casing
        if (!casingExitLocation || !casingPrefab)
        { return; }

        //Create the casing
        GameObject tempCasing;
        tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;

        //Add force on casing to push it out
        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);

        //Add torque to make casing spin in random direction
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

        //Destroy casing after X seconds
        Destroy(tempCasing, destroyTimer);
        
    }
}
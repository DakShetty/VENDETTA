using UnityEngine;

public class LightDrop : MonoBehaviour
{
    public float rotateSpeed = 120f;
    public float bobSpeed = 3.5f;
    public float bobHeight = 0.15f;
    public float magnetRadius = 6.0f;
    public float initialFlySpeed = 5.0f;
    public float maxFlySpeed = 18.0f;
    public float absorbDistance = 1.2f;
    public int lightValue = 1;

    private Transform playerTransform;
    private PlayerLight playerLight;
    private Vector3 initialPosition;
    private float flySpeed;
    private bool isAbsorbed = false;
    private float seed;

    void Start()
    {
        initialPosition = transform.position;
        flySpeed = initialFlySpeed;
        seed = Random.Range(0f, 100f);

        FindPlayer();
        SetupGlowingVisualsAndParticles();
    }

    void FindPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerLight = playerObj.GetComponent<PlayerLight>();
        }
        else
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                playerTransform = pm.transform;
                playerLight = pm.GetComponent<PlayerLight>();
            }
        }
    }

    void SetupGlowingVisualsAndParticles()
    {
        // 1. Golden Point Light for radiant glow
        Light pLight = GetComponent<Light>();
        if (pLight == null) pLight = gameObject.AddComponent<Light>();
        pLight.type = LightType.Point;
        pLight.color = new Color(1.0f, 0.85f, 0.25f);
        pLight.intensity = 3.5f;
        pLight.range = 6.5f;

        // 2. Emissive gold material on core sphere
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            if (sh != null)
            {
                Material mat = new Material(sh);
                mat.color = new Color(1.0f, 0.88f, 0.25f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(2.0f, 1.6f, 0.4f));
                mr.material = mat;
            }
        }

        // 3. Glowing yellow particle system
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor = new Color(1.0f, 0.88f, 0.30f, 0.95f);
        main.startSize = 0.22f;
        main.startLifetime = 1.0f;
        main.startSpeed = 0.6f;
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 20f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        Shader partShader = Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
        if (partShader != null)
        {
            psRenderer.material = new Material(partShader);
        }
    }

    void Update()
    {
        if (isAbsorbed) return;

        // Gentle floating bob & rotation
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        Vector3 targetPoint = playerTransform.position + Vector3.up * 1.0f;
        float dist = Vector3.Distance(transform.position, targetPoint);

        // Magnetic radius absorption (pulls to player even if in boss spawn area)
        if (dist <= magnetRadius)
        {
            flySpeed = Mathf.Min(flySpeed + 20f * Time.deltaTime, maxFlySpeed);
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, flySpeed * Time.deltaTime);

            if (dist <= absorbDistance)
            {
                Absorb();
            }
        }
        else
        {
            // Hover in place with slight vertical bobbing
            float hoverY = initialPosition.y + Mathf.Sin((Time.time + seed) * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, hoverY, transform.position.z);
            flySpeed = initialFlySpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAbsorbed) return;

        if (other.CompareTag("Player"))
        {
            if (playerLight == null) playerLight = other.GetComponent<PlayerLight>();
            Absorb();
        }
    }

    public void Absorb()
    {
        if (isAbsorbed) return;
        isAbsorbed = true;

        if (playerLight != null)
        {
            playerLight.AbsorbLight(lightValue);
            VendettaAudioManager.Instance?.PlayPickupChime();
        }

        Destroy(gameObject);
    }
}

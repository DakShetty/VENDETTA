using UnityEngine;

public class LightAbsorption : MonoBehaviour
{
    public int lightValue = 1;
    public float magnetRadius = 6.0f;
    private bool isAbsorbed = false;
    private Transform playerTransform;
    private PlayerLight playerLight;

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
            playerLight = p.GetComponent<PlayerLight>();
        }
    }

    void Update()
    {
        if (isAbsorbed) return;

        // Backup magnetic collection check
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        Vector3 target = playerTransform.position + Vector3.up * 1.0f;
        float dist = Vector3.Distance(transform.position, target);

        if (dist <= 1.2f)
        {
            Absorb();
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

        var drop = GetComponent<LightDrop>();
        if (drop != null)
        {
            drop.Absorb();
            return;
        }

        if (playerLight != null)
        {
            playerLight.AbsorbLight(lightValue);
            VendettaAudioManager.Instance?.PlayPickupChime();
        }

        Destroy(gameObject);
    }
}

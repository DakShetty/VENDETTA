using UnityEngine;

public class LightAbsorption : MonoBehaviour
{
    public int lightValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLight playerLight = other.GetComponent<PlayerLight>();

            if (playerLight != null)
            {
                playerLight.AbsorbLight(lightValue);
                VendettaAudioManager.Instance?.PlayPickupChime();
            }

            Destroy(gameObject);
        }
    }
}


using UnityEngine;

public class LightAbsorption : MonoBehaviour
{
    public int lightValue = 1;
    private bool isAbsorbed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isAbsorbed) return;

        if (other.CompareTag("Player"))
        {
            PlayerLight playerLight = other.GetComponent<PlayerLight>();

            if (playerLight != null)
            {
                isAbsorbed = true;
                playerLight.AbsorbLight(lightValue);
                VendettaAudioManager.Instance?.PlayPickupChime();
                Destroy(gameObject);
            }
        }
    }
}

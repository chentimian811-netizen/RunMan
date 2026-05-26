using UnityEngine;
using Mirror;

public class ExtractionZone : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        Debug.Log("[ExtractionZone] Trigger entered: " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[ExtractionZone] Not a player, ignored");
            return;
        }
        if (GameManager.instance == null)
        {
            Debug.Log("[ExtractionZone] GameManager is null!");
            return;
        }
        if (GameManager.instance.gameIsOver)
        {
            Debug.Log("[ExtractionZone] Game already over");
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            int count = player.collectibles;
            Debug.Log("[ExtractionZone] Player extracted with " + count + " collectibles");
            player.Die();
            GameManager.instance.PlayerExtracted(count);
        }
    }
}

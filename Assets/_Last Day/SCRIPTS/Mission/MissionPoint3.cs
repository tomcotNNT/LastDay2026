using System.Collections;
using UnityEngine;

public class MissionPoint3 : MonoBehaviour
{
   
    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;

            Debug.Log("Explore Triggered (MissionPoint3)");

            if (Mission.Instance != null)
            {
                Mission.Instance.missionSystem.DisableMaker();
                Debug.Log("Marker Disabled (MissionPoint3)");
            }

            if (Mission.Instance != null)
                Mission.Instance.UIMission(3);

            HunterDialogueZone dialogue = FindObjectOfType<HunterDialogueZone>();

            if (dialogue != null)
                dialogue.UnlockSecondDialogue();    

            gameObject.SetActive(false);
        }
    }

}

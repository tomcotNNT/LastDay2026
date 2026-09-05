    using UnityEngine;
    using System.Collections;

    public class MutantIntro : MonoBehaviour
    {
        public GameObject mainCam;
        public GameObject introCam;

        public IEnumerator ShowMutant(Transform mutant, float holdDuration = 11f)
        {
            if (mainCam == null || introCam == null || mutant == null)
                yield break;

            mainCam.SetActive(false);
            introCam.SetActive(true);


            yield return new WaitForSeconds(holdDuration);

            introCam.SetActive(false);
            mainCam.SetActive(true);
        }
    }
using TMPro;
using UnityEngine;

public class TextPopup : MonoBehaviour
{
    public GameObject successText;
    public TMP_Text resultText;
    public Animator animator;

    
    public void ShowSuccess()
    {   
        successText.SetActive(true);

        resultText.text = "UPGRADE SUCCESS";
        resultText.color = Color.yellow;

        animator.Play("Popup", 0, 0);
    }

    public void ShowFail()
    {
        successText.SetActive(true);
        resultText.text = "UPGRADE FAILED";
        resultText.color = Color.red;

        animator.Play("Popup", 0, 0);
    }

    // gọi bằng Animation Event cuối clip
    public void Hide()
    {
        successText.SetActive(false);
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Class not used.
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(Button))]
public class NotificationController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text content = null;

    [SerializeField]
    private Button button = null;
    private TMP_Text buttonTMPText = null;

    public NotificationType notificationType;

    private void Awake()
    {
        buttonTMPText = button.GetComponentInChildren<TMP_Text>();
    }


    public void SetNofitication(string notificationText, string buttonText)
    {
        content.text = notificationText;
        
        if(buttonText != "")
        {
            buttonTMPText.text = buttonText;
        }
        else
        {
            button.gameObject.SetActive(false);
        }
    }


}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// This class must be attached to a Button game object to trigger an action after button has been clicked.
[RequireComponent (typeof(Button))]
public class CapturePositionWithDelay : MonoBehaviour
{
    private readonly KeyCode debugKeyPress = KeyCode.A;

    [SerializeField] int delayInSeconds = 0;
    [SerializeField] TMP_Text timerText = null;

    [Tooltip("Attach the OVRGazePointer if it should be hidden during countdown.")]
    [SerializeField] OVRGazePointer gazePointer = null;

    [Tooltip("List of buttons to disable and reenable when countdown starts and stop accordingly.")]
    [SerializeField] List<Button> buttonsToDisable = new List<Button>();

    [SerializeField] Button.ButtonClickedEvent onClick = null;

    private Coroutine delayRoutine = null;
    private Button countdownButton = null;
    private string defaultTimerText = "";

    private void Awake()
    {
        defaultTimerText = timerText.text;
        countdownButton = GetComponent<Button>();
        countdownButton.onClick.AddListener(() => StartAction());

    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(debugKeyPress))
        {
            StartAction();
        }
#endif
    }

    private void StartAction()
    {
        if (delayRoutine != null)
            StopCoroutine(delayRoutine);

        delayRoutine = StartCoroutine(TriggerActionWithDelay());
    }


    public IEnumerator TriggerActionWithDelay()
    {
        // resume global time to 1 to count seconds in real time when not in session
        Time.timeScale = 1;

        int secondsLeft = delayInSeconds;

        if(gazePointer != null)
        {
            gazePointer.gameObject.SetActive(false);
        }

        foreach (Button b in buttonsToDisable)
        {
            b.interactable = false;
        }

        while (secondsLeft > 0){
            timerText.text = secondsLeft.ToString();
            yield return new WaitForSeconds(1);
            secondsLeft--;
        }

        onClick.Invoke();
        timerText.text = defaultTimerText;

        if (gazePointer != null)
        {
            gazePointer.gameObject.SetActive(true);
        }

        foreach (Button b in buttonsToDisable)
        {
            b.interactable = true;
        }

        // pause time again as we are not in session
        Time.timeScale = 0;
    }
}

using UnityEngine;
using TMPro;

public class DebugShowHandPose : MonoBehaviour
{
    [SerializeField] TMP_Text content = null;
    [SerializeField] HandController handController= null;

    private void Awake() => handController.OnHandPoseChanged += UpdateText;

    private void OnDisable() => handController.OnHandPoseChanged -= UpdateText;

    private void UpdateText(HandPose pose) => content.text = pose.ToString();

}

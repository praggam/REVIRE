using Assets.Scripts.Gameplay;
using System.Collections.Generic;
using UnityEngine;

// Used to hide 'Next Task' button in Task Menu if currently in last task.
public class ButtonEnablerPerTask : MonoBehaviour
{
    [System.Serializable]
    protected class PerTaskObjectEnabler
    {
        [SerializeField]
        public GameObject enabledObject = null;

        [SerializeField]
        public List<TaskType> tasks = new List<TaskType>();
    }

    [SerializeField]
    List<PerTaskObjectEnabler> enablers = new List<PerTaskObjectEnabler>();

    private void OnEnable()
    {
        foreach(PerTaskObjectEnabler enabler in enablers)
        {
            bool activated = GameManager.Instance && 
                ((GameManager.Instance.IsSessionActive() && enabler.tasks.Contains(GameManager.Instance.CurrentTask.Type)) ||
                (!GameManager.Instance.IsSessionActive() && enabler.tasks.Contains(TaskType.None)));

            if (GameManager.Instance &&
                GameManager.Instance.IsLastTask() &&
                enabler.enabledObject.name == "Next Task")
            {
                activated = false;
            }
             
            if (GameManager.Instance &&
               GameManager.Instance.CurrentTask == null &&
               enabler.enabledObject.name == "Back to Menu")
            {
                activated = true;
            }

            enabler.enabledObject.SetActive(activated);
        }
        

    }
}
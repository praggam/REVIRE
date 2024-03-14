using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Generic class which can be attached to any game object with colliders to trigger actions on collision with specified colliders to e.g. change color or material. The script can also be attached to a parent with Rigidbody that has a collider in child.
[RequireComponent(typeof(Renderer))]
public class OnCollisionBehaviour: MonoBehaviour
{
    #region GENERAL
    [SerializeField]
    [Tooltip("Add all game object tags which should trigger actions on collision. The string must exactly match an existing game object tag.")]
    private List<string> collideWithTags = new List<string>();

    private Renderer rend = null;
    private Coroutine collisionExitRoutine;
    #endregion

    #region COLOR PROPERTIES
    [SerializeField]
    private bool changeColor = false;

    [SerializeField]
    [Tooltip("Gradually changes between the two colors on trigger enter and exit.")]
    private bool colorLerping = false;

    [SerializeField]
    private Color colorOnCollision = Color.red;
    private Color colorOnCollisionEnd = Color.blue;
    #endregion

    private void Awake()
    {
        rend = gameObject.GetComponent<Renderer>();
        colorOnCollisionEnd = rend.material.color;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collideWithTags.Contains(collision.collider.tag))
            return;

        if(collisionExitRoutine != null) 
            StopCoroutine(collisionExitRoutine);

        if(changeColor)
            ChangeColor(colorOnCollisionEnd, colorOnCollision);
        Debug.LogWarning("Colllision entered!!!");
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collideWithTags.Contains(collision.collider.tag))
            return;

        collisionExitRoutine = StartCoroutine(DelayedExit(0.3f));
    }

    // Delay the exit routine, avoids glitching if in the meantime the object will collide again
    private IEnumerator DelayedExit(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        if (changeColor)
            ChangeColor(colorOnCollision, colorOnCollisionEnd);
    }

    private void ChangeColor(Color colorFrom, Color colorTo)
    {
        if (colorLerping)
        {
            // TODO lerping should be in routine so it's not working
            rend.material.color = Color.Lerp(colorFrom, colorTo, 0.5f);
        }
        else
        {
            rend.material.color = colorTo;
        }
    }
}

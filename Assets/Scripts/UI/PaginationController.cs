using System.Collections.Generic;
using UnityEngine;

// Controls group of pages/tabs. Allows to switch between previous and next page by enabling and disabling UI elements. Implements unity editor methods that can be accessed in inspector menu to speed up development.
public class PaginationController : MonoBehaviour
{
    [Tooltip("UI Panel steps will be activated from the list in sequential order.")]
    [SerializeField] List<GameObject> panelPages = new List<GameObject>();

    [Tooltip("Panel page index which will open when GameObject enabled.")]
    [Range(0, 10)]
    [SerializeField] int currentPage = 0;

    private void Start()
    {
        if(panelPages.Count == 0)
        {
            Debug.LogError("Pagination error: Panel Pages List empty.");
            return;
        }

        //ActivateCurrentPage();
    }

    private void OnEnable()
    {
        ActivateCurrentPage();
    }


    private void OnDisable()
    {
        Reset();
    }

    // deactivates currently open page, if any, and activates next page, if any
    public void NextOrClose()
    {
        if (currentPage < panelPages.Count)
        {
            panelPages[currentPage].SetActive(false);
        }

        ++currentPage;

        if (currentPage < panelPages.Count)
        {
            panelPages[currentPage].SetActive(true);
        }
        else if(currentPage == panelPages.Count)
        {
            // DO SOMETHING LIKE CLOSE ALL WINDOWS
        }
        else
        {
            Debug.LogError(string.Format("Pagination error: Next() called on invalid page index: {0}.", currentPage));
            currentPage = 0;
        }
    }

    // deactivates currently open page, if any, and activates previous page, if any
    public void Previous()
    {
        if (currentPage >= 0 && currentPage < panelPages.Count)
        {
            panelPages[currentPage].SetActive(false);
        }

        if (--currentPage < panelPages.Count)
        {
            panelPages[currentPage].SetActive(true);
        }
        else
        {
            Debug.LogError(string.Format("Pagination error: Previous() called on invalid page index: {0}.", currentPage));
            currentPage = 0;
        }
    }

    public void Reset()
    {
        currentPage = 0;
    }


    [ContextMenu("Activate Current Page")]
    public void ActivateCurrentPage()
    {
        panelPages.ForEach(page => page.SetActive(false));
        panelPages[currentPage].SetActive(true);
    }

#if UNITY_EDITOR
    [ContextMenu("Autofill Pages From Children")]
    public void AddAllGestures()
    {
        List<GameObject> pages = new List<GameObject>();

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            pages.Add(gameObject.transform.GetChild(i).gameObject);
        }

        panelPages = pages;
    }
#endif
}

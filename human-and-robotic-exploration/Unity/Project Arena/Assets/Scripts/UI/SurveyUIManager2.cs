using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurveyUIManager2 : MonoBehaviour {

    [Header("Pages of survay")] 
    [SerializeField] private GameObject[] pages;

    public Text pageText;

    private int currentPage = 0;

    public void Start()
    {
        InitializePageText();
    }

    // initialize the text which contain the current page that user is reading and total pages of survey
    private void InitializePageText()
    {
        pageText.text = (currentPage + 1) + " / " + pages.Length;
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            pages[currentPage].SetActive(false);
            currentPage++;
            pages[currentPage].SetActive(true);

            pageText.text = (currentPage + 1) + " / " + pages.Length;
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            pages[currentPage].SetActive(false);
            currentPage--;
            pages[currentPage].SetActive(true);

            pageText.text = (currentPage + 1) + " / " + pages.Length;
        }
    }
}

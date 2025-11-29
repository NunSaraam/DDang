using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideManager : MonoBehaviour
{
    public PanelSliede pS;
    public RectTransform mainmenu;
    public RectTransform tutorial;
    public RectTransform awards;

    [SerializeField] private RectTransform current;

    private void Start()
    {
        current = mainmenu;

    }

    public void GoToMainmenu()
    {
        if (current == mainmenu) return;

        pS.Slide(current, mainmenu);
        current = mainmenu;
    }

    public void GoToTutorial()
    {
        if (current == tutorial) return;

        pS.Slide(current, tutorial);
        current = tutorial;
    }

    public void GoToAwards()
    {
        if (current == awards) return;

        pS.Slide(current, awards);
        current = awards;
    }
}

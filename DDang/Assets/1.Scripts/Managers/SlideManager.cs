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

        if (pS.Slide(current, mainmenu)) current = mainmenu;   // 실제로 넘어갔을 때만 바꿈
    }

    public void GoToTutorial()
    {
        if (current == tutorial) return;

        if (pS.Slide(current, tutorial)) current = tutorial;
    }

    public void GoToAwards()
    {
        if (current == awards) return;

        if (pS.Slide(current, awards)) current = awards;
    }
}

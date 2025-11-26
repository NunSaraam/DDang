using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    public Image player1Panel;
    public Image player2Panel;

    private float pulseDuration = 3f;
    private float pulseSpeed = 1f;


    private void Update()
    {
        if (RoundManager.Instance.currentState == RoundState.Result)
        {
            StartCoroutine(Result());
        }
    }

    public void StartPulseEffect(PlayerType winner)
    {
        StartCoroutine(PulseEffect(winner));
    }

    IEnumerator Result()
    {
        StartPulseEffect(RoundManager.Instance.gameWinner);

        yield return new WaitForSeconds(5f);

        SceneLoadManager.Instance.LoadScene("Mainmenu");
            
    }

    IEnumerator PulseEffect(PlayerType winner)
    {
        float timer = 0f;

        while (timer < pulseDuration)
        {
            float alpha = Mathf.Lerp(.5f, 1f, Mathf.PingPong(Time.time * pulseSpeed, 1f));

            SetImageAlpha(player1Panel, alpha);
            SetImageAlpha(player2Panel, alpha);

            timer += Time.deltaTime;
            yield return null;
        }
        
        if (winner == PlayerType.Player1)
        {
            SetImageAlpha(player1Panel, 1f);
            SetImageAlpha(player2Panel, .5f);
        }
        else if (winner == PlayerType.Player2)
        {
            SetImageAlpha(player1Panel, .5f);
            SetImageAlpha(player2Panel, 1f);
        }
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }
}

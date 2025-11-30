using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    public Image player1Panel;
    public GameObject p1Win;
    public GameObject p1Lose;
    
    public Image player2Panel;
    public GameObject p2Win;
    public GameObject p2Lose;

    private float pulseDuration = 3f;
    private float pulseSpeed = 1f;

    private void Start()
    {
        p1Win.SetActive(false);
        p1Lose.SetActive(false);
        p2Win.SetActive(false);
        p2Lose.SetActive(false);
    }

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

        switch (winner)
        {
            case PlayerType.Player1:
                p1Win.SetActive(true);
                p2Lose.SetActive(true);
                break;

            case PlayerType.Player2:
                p2Win.SetActive(true);
                p1Lose.SetActive(true);
                break ;
        }
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

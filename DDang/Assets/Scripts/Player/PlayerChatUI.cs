using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChatUI : MonoBehaviour
{
    public Image stunImage;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI storeText;

    public PlayerMovement pM;
    public PlayerType playerType;

    public Transform targetCamera;

    private void Start()
    {
        if (pM == null) pM = GetComponentInParent<PlayerMovement>();

        if (targetCamera == null) targetCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + targetCamera.forward);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (pM == null || ScoreUIManager.Instance == null)  return;

        if (RoundManager.Instance != null && RoundManager.Instance.currentState == RoundState.Playing)
        {
            bool isStunned = pM.state == PlayerState.Stunned;
            stunImage.gameObject.SetActive(isStunned);
            scoreText.gameObject.SetActive(!isStunned);
        }
        else if (RoundManager.Instance != null && RoundManager.Instance.currentState == RoundState.Store)
        {
            stunImage.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
            storeText.gameObject.SetActive(true);
        }


        switch (playerType)
        {
            case PlayerType.Player1:
                if (ScoreUIManager.Instance.player1ScoreText != null)
                {
                    scoreText.text = ScoreUIManager.Instance.player1ScoreText.text;
                }
                break;

            case PlayerType.Player2:
                if (ScoreUIManager.Instance.player2ScoreText != null)
                {
                    scoreText.text = ScoreUIManager.Instance.player2ScoreText.text;
                }
                break;
        }
    }
}

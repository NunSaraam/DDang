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

    public GridGenerator grid;

    public Transform targetCamera;

    private void Start()
    {
        if (pM == null) pM = GetComponentInParent<PlayerMovement>();

        if (grid == null) grid = FindObjectOfType<GridGenerator>();

        if (targetCamera == null) targetCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + targetCamera.forward);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (grid == null) return;

        int p1Score = grid.CountScore(PlayerType.Player1);
        int p2Score = grid.CountScore(PlayerType.Player2);

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

                scoreText.text = $"{p1Score}";
               
                break;

            case PlayerType.Player2:

                scoreText.text = $"{p2Score}";

                break;
        }
    }
}

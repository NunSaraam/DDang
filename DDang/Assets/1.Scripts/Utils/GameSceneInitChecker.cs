using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;

public class GameSceneInitChecker : MonoBehaviour
{
    private IEnumerator Start()
    {
        UIManager.Instance?.ReconnectUI();

        Debug.Log("SceneUIManager 체킹 시작");
        while (UIManager.Instance == null || !UIManager.Instance.IsUIReady())
            yield return null;
        Debug.Log("SceneUIManager 완료");

        while (RoundManager.Instance == null)
            yield return null;
        Debug.Log("모든 게임 준비 완료");

        if (GameSession.Instance.needFullReset || RoundManager.Instance.Match == null)
        {
            RoundManager.Instance.StartMatch();
            QuestManager.Instance?.ResetAllQuest();
            CoinManager.Instance?.ResetCoins();
            UIManager.Instance?.ResetAllUI();
            foreach (var player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
                player.ResetStats();
            GameSession.Instance.needFullReset = false;
        }

        RoundManager.Instance.BeginRound();
    }
}

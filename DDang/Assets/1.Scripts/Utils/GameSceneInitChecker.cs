using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;
using Stats;
using Store;

public class GameSceneInitChecker : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 라운드 HUD는 씬 안의 UI.RoundHudInstaller가 스스로 준비하므로 UI를 기다리지 않음
        while (RoundManager.Instance == null)
            yield return null;
        Debug.Log("모든 게임 준비 완료");

        if (GameSession.Instance.needFullReset || RoundManager.Instance.Match == null)
        {
            RoundManager.Instance.StartMatch();
            QuestManager.Instance?.ResetAllQuest();
            CoinManager.Instance?.ResetCoins();
            PlayerDataManager.Instance.ResetMatch();                // 증강·디버프 초기화
            GameSession.Instance.needFullReset = false;
        }

        RoundManager.Instance.BeginRound();
    }
}

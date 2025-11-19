using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneInitChecker : MonoBehaviour
{
    private IEnumerator Start()
    {
        ScoreUIManager.Instance?.ReconnectUI();

        Debug.Log("SceneUIManager 체킹 시작");
        while (ScoreUIManager.Instance == null || !ScoreUIManager.Instance.IsUIReady())
            yield return null;
        Debug.Log("SceneUIManager 완료");

        while (RoundManager.Instance == null)
            yield return null;
        Debug.Log("모든 게임 준비 완료");


        RoundManager.Instance.Begin();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static event System.Action<string> OnSceneChanged;           // 씬 전환 이벤트
    public static event System.Action<GameState> OnGameStarteChanged;   // 상태 전환 이벤트
    public static System.Action<float> OnVolumeChanged;                 // 볼륨 변경 이벤트
    public static System.Action<int> OnScoreChanged;                    // 점수 변경 이벤트
    public static event System.Action OnGamePaused;                     // 게임 정지 이벤트
    public static event System.Action OnGameResumed;                    // 게임 재시작 이벤트
    public static event System.Action OnGameLoad;                       // 게임 데이터 로드 이벤트
    public static event System.Action<string> OnItemPurchased;          // 구매한 아이템 이벤트
    public static event System.Action<string> OnItemSelected;           // 아이템 선택 이벤트
    public static event System.Action<int> OnCoinsChanged;              // 골드 변경 이벤트

    public static void SceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);

    public static void GameStateChanged(GameState gameState) => OnGameStarteChanged?.Invoke(gameState);

    public static void VolumeChanged(float volume) => OnVolumeChanged?.Invoke(volume);

    public static void ScoreChanged(int score) => OnScoreChanged?.Invoke(score);

    public static void GamePaused() => OnGamePaused?.Invoke();

    public static void GameResumed() => OnGameResumed?.Invoke();

    public static void GameLoad() => OnGameLoad?.Invoke();

    public static void CoinChanged(int coin) => OnCoinsChanged?.Invoke(coin);


    //설정 관련 이벤트 정의
    public static System.Action<int> OnResolutionChanged;                   //해상도 변경
    public static System.Action<bool> OnFullscreenChanged;                  //전체화면 변경
    public static System.Action<int> OnQualityChanged;                      //그래픽 품질 변경

    public static void ResolutionChanged(int resolutionIndex) => OnResolutionChanged?.Invoke(resolutionIndex);

    public static void FullscreenChanged(bool isFullscreen) => OnFullscreenChanged?.Invoke(isFullscreen);

    public static void QualityChanged(int qualitylevel) => OnQualityChanged?.Invoke(qualitylevel);


}
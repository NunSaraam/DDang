using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static event System.Action<string> OnSceneChanged;           // �� ��ȯ �̺�Ʈ
    public static event System.Action<GameState> OnGameStarteChanged;   // ���� ��ȯ �̺�Ʈ
    public static System.Action<float> OnVolumeChanged;                 // ���� ���� �̺�Ʈ
    public static System.Action<int> OnScoreChanged;                    // ���� ���� �̺�Ʈ
    public static event System.Action OnGamePaused;                     // ���� ���� �̺�Ʈ
    public static event System.Action OnGameResumed;                    // ���� ����� �̺�Ʈ
    public static event System.Action OnGameLoad;                       // ���� ������ �ε� �̺�Ʈ
    public static event System.Action<string> OnItemPurchased;          // ������ ������ �̺�Ʈ
    public static event System.Action<string> OnItemSelected;           // ������ ���� �̺�Ʈ
    public static event System.Action<int> OnCoinsChanged;              // ��� ���� �̺�Ʈ

    public static void SceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);

    public static void GameStateChanged(GameState gameState) => OnGameStarteChanged?.Invoke(gameState);

    public static void VolumeChanged(float volume) => OnVolumeChanged?.Invoke(volume);

    public static void ScoreChanged(int score) => OnScoreChanged?.Invoke(score);

    public static void GamePaused() => OnGamePaused?.Invoke();

    public static void GameResumed() => OnGameResumed?.Invoke();

    public static void GameLoad() => OnGameLoad?.Invoke();

    public static void CoinChanged(int coin) => OnCoinsChanged?.Invoke(coin);


    //���� ���� �̺�Ʈ ����
    public static System.Action<int> OnResolutionChanged;                   //�ػ� ����
    public static System.Action<bool> OnFullscreenChanged;                  //��üȭ�� ����
    public static System.Action<int> OnQualityChanged;                      //�׷��� ǰ�� ����

    public static void ResolutionChanged(int resolutionIndex) => OnResolutionChanged?.Invoke(resolutionIndex);

    public static void FullscreenChanged(bool isFullscreen) => OnFullscreenChanged?.Invoke(isFullscreen);

    public static void QualityChanged(int qualitylevel) => OnQualityChanged?.Invoke(qualitylevel);


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,           //메인 메뉴
    Playing,        //게임 플레이 중
    Paused,         //일시정지 상태
    Store,          //상점
    GameResult      //게임 결과
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState {  get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


}

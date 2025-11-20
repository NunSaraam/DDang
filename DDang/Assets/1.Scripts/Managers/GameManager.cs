using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,           //메인 메뉴
    Gmae,           //게임
    Store,          //상점
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

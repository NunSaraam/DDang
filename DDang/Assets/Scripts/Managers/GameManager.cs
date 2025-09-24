using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,           //���� �޴�
    Playing,        //���� �÷��� ��
    Paused,         //�Ͻ����� ����
    Store,          //����
    GameResult      //���� ���
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
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


}

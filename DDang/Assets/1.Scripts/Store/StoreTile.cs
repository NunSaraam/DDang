using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StoreType
{
    RandomStat,
    MoveSpeed,
    StunReduce
}

public class StoreTile : MonoBehaviour
{
    public StoreType storeType;
    public int randCost = 2;
    public int speedCost = 5;
    public int stunCost = 5;

    public Image tileImage;

    [Range(0, 99)] public int randFail = 69;
    [Range(0, 99)] public int randSpeed = 84;
    [Range(0, 99)] public int randStun = 99;

    private Image image;


    public void SetImage(Sprite icon)
    {
        if (tileImage != null)
        {
            GameObject.Find("TileImage");
        }
        tileImage.sprite = icon;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && player.state == PlayerState.Attacking)
        {
            TryPurchase(player);
        }
    }

    public void TryPurchase(PlayerMovement player)
    {
        if (RoundManager.Instance.currentState != RoundState.Store)     //현재 라운드가 상점이 아니면 리턴
        {
            return;
        }

        PlayerType playerType = player.playerType;
        int coins = CoinManager.Instance.GetCoins(playerType);

        int price = 0;

        switch (storeType)
        {
            case StoreType.RandomStat:      //랜덤 스탯

                price = randCost;

                break;

            case StoreType.MoveSpeed:       //속도 증가

                price = speedCost;

                break;

            case StoreType.StunReduce:      //스턴 시간 감소

                price = stunCost;

                break;
        }

        if (coins < price)
        {
            Debug.Log($"{playerType} 코인 부족");
            return;
        }

        CoinManager.Instance.SpendCoins(playerType, price);

        switch (storeType)
        {
            case StoreType.RandomStat:      //랜덤 스탯
                ApplyRandomStat(player);
                break;

            case StoreType.MoveSpeed:       //속도 증가
                if (playerType == PlayerType.Player1)
                {
                    PlayerDataManager.Instance.p1MoveSpeed += 1;

                    Debug.Log($"이동속도 +{1}");
                }
                else if (playerType == PlayerType.Player2)
                {
                    PlayerDataManager.Instance.p2MoveSpeed += 1;

                    Debug.Log($"이동속도 +{1}");
                }
                break;

            case StoreType.StunReduce:      //스턴 시간 감소
                if (playerType == PlayerType.Player1)
                {
                    PlayerDataManager.Instance.p1StunTime -= .5f;

                    Debug.Log($"랜덤 강화 스턴 시간 감소 -{0.5}");
                }
                else if (playerType == PlayerType.Player2)
                {
                    PlayerDataManager.Instance.p2StunTime -= .5f;

                    Debug.Log($"랜덤 강화 스턴 시간 감소 -{0.5}");
                }
                break;
        }
    }

    private void ApplyRandomStat(PlayerMovement player)
    {
        PlayerType playerType = player.playerType;

        int random = Random.Range(0, 100);

        int fail = randFail;
        int speed = fail + randSpeed;
        int stun = speed + randStun;

        if (random < fail)
        {
            Debug.Log("꽝");
        }
        else if (random < speed)
        {
            int cost = Random.Range(1, 3);

            if (playerType == PlayerType.Player1)
            {
                PlayerDataManager.Instance.p1MoveSpeed += cost;

                Debug.Log($"이동속도 +{cost}");
            }
            else if (playerType == PlayerType.Player2)
            {
                PlayerDataManager.Instance.p2MoveSpeed += cost;

                Debug.Log($"이동속도 +{cost}");
            }
        }
        else if (random < stun)
        {
            float cost = Random.Range(0.5f, 1f);

            if (playerType == PlayerType.Player1)
            {
                PlayerDataManager.Instance.p1StunTime -= cost;

                Debug.Log($"랜덤 강화 스턴 시간 감소 -{cost:F0}");
            }
            else if (playerType == PlayerType.Player2)
            {
                PlayerDataManager.Instance.p2StunTime -= cost;

                Debug.Log($"랜덤 강화 스턴 시간 감소 -{cost:F0}");
            }

        }
    }
}

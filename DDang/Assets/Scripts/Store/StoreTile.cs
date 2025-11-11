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
    public int cost;

    public Image tileImage;

    [Range(0, 99)] public int randFail = 49;
    [Range(0, 99)] public int randSpeed = 74;
    [Range(0, 99)] public int randStun = 100;

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

        if (coins < cost)           //코인 부족 시 리턴
        {
            return;
        }

        CoinManager.Instance.SpendCoins(playerType, cost);

        switch (storeType)
        {
            case StoreType.RandomStat:      //랜덤 스탯
                ApplyRandomStat(player);
                break;

            case StoreType.MoveSpeed:       //속도 증가
                player.moveSpeed += 1f;
                Debug.Log($"이동속도 +{1}");
                break;

            case StoreType.StunReduce:      //스턴 시간 감소
                player.stunTime = Mathf.Max(0.5f, player.stunTime - 0.5f);
                Debug.Log($"랜덤 강화 스턴 시간 감소 -{0.5}");
                break;
        }
    }

    private void ApplyRandomStat(PlayerMovement player)
    {
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

            player.moveSpeed += cost;
            Debug.Log($"랜덤 강화 이동속도 +{cost}");
        }
        else if (random < stun)
        {
            float cost = Random.Range(0.5f, 1f);

            player.stunTime = Mathf.Max(0f, player.stunTime -  cost);
            Debug.Log($"랜덤 강화 스턴 시간 감소 -{cost:F0}");
        }
    }
}

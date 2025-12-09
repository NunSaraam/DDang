using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
    [SerializeField] PlayerChatUI player1CchatUI;
    [SerializeField] PlayerChatUI player2CchatUI;

    public TextMeshProUGUI shopingTimeText;

    private float currentShopingTime;

    private void Start()
    {
        player1CchatUI.storeText.enabled = true;
        player2CchatUI.storeText.enabled = true;

        player1CchatUI.storeText.text = "";
        player2CchatUI.storeText.text = "";
    }

    
    public void UpdateStoreUI(PlayerMovement pM)
    {
        switch (pM.playerType)
        {
            case PlayerType.Player1:

                if (pM.currentStoreTile.storeType == StoreType.MoveSpeed && player1CchatUI.playerType == PlayerType.Player1)
                {
                    player1CchatUI.storeText.text = $"{pM.currentStoreTile.name}\n이동속도를 1 증가 시킴";
                }
                else if (pM.currentStoreTile.storeType == StoreType.StunReduce && player1CchatUI.playerType == PlayerType.Player1)
                {
                    player1CchatUI.storeText.text = $"{pM.currentStoreTile.name}\n상대 기절시간을 증가 시킴";
                }
                else if (pM.currentStoreTile.storeType == StoreType.RandomStat && player1CchatUI.playerType == PlayerType.Player1)
                {
                    player1CchatUI.storeText.text = $"{pM.currentStoreTile.name}\n 랜덤 스탯 적용";
                }
                break;

            case PlayerType.Player2:

                if (pM.currentStoreTile.storeType == StoreType.MoveSpeed && player2CchatUI.playerType == PlayerType.Player2)
                {
                    player2CchatUI.storeText.text = $"{pM.currentStoreTile.name}\n이동속도를 1 증가 시킴";
                }
                else if (pM.currentStoreTile.storeType == StoreType.StunReduce && player2CchatUI.playerType == PlayerType.Player2)
                {
                    player2CchatUI.storeText.text = $"{pM.currentStoreTile.name}\n상대 기절시간을 증가 시킴";
                }
                else if (pM.currentStoreTile.storeType == StoreType.RandomStat && player2CchatUI.playerType == PlayerType.Player2)
                {
                    player2CchatUI.storeText.text = $"{pM.currentStoreTile.name}\n 랜덤 스탯 적용";
                }
                break;
        }
    }

    private void UpdateRoundUI()
    {
        shopingTimeText.text = $"{currentShopingTime:F2}";
    }

    public void ShopingTime(float shopingTime)
    {
        currentShopingTime = shopingTime;
        UpdateRoundUI();
    }
    public void ExitTileUI(PlayerMovement pM)
    {
        switch (pM.playerType)
        {
            case PlayerType.Player1:

                player1CchatUI.storeText.text = "";
                break;

            case PlayerType.Player2:

                player2CchatUI.storeText.text = "";
                break;
        }
    }
}

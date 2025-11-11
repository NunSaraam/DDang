using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreUIManager : MonoBehaviour
{
    [SerializeField] PlayerChatUI player1CchatUI;
    [SerializeField] PlayerChatUI player2CchatUI;

    private void Start()
    {
        player1CchatUI.storeText.enabled = true;
        player2CchatUI.storeText.enabled = true;
            
    }

    
}

using Initialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static MyUtility.Utility;
using UnityEngine.UI;

public class MainHUD : Entity {

    [SerializeField] private Color LocalMessageColor = Color.blue;
    [SerializeField] private Color Player2MessageColor = Color.red;
    [SerializeField] private float chatMessageReceivedPopupDuration = 3.0f;
    [SerializeField] private float chatMessageSentPopupDuration = 2.0f;
    [SerializeField] private int chatMessagesRecycleLimit = 50;

    private bool chatOpened = false;
    private float chatMessagePopupTimer = 0.0f;

    private Image player1HealthBarImage;
    private Image player2HealthBarImage;

    private GameObject chat;
    private GameObject chatLogContent;
    private GameObject chatMessageReference;
    private TMP_InputField chatInputFieldComp;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        SetupReferences();
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;


        CheckInput();
        UpdateChatPopupTimer();
    }
    private void SetupReferences() {

        SetupPlayer1HUDReferences();
        SetupPlayer2HUDReferences();
        SetupChatHUDReferences();
    }


    private void SetupChatHUDReferences() {
        //Chat
        Transform chatTransform = transform.Find("Chat");
        Validate(chatTransform, "Failed to find chat reference", ValidationLevel.ERROR, true);
        chat = chatTransform.gameObject;

        //Chat Input Field
        Transform chatInputFieldTransform = chatTransform.Find("ChatInput").transform;
        Validate(chatInputFieldTransform, "Failed to find chat input field reference", ValidationLevel.ERROR, true);
        chatInputFieldComp = chatInputFieldTransform.GetComponent<TMP_InputField>();
        Validate(chatInputFieldComp, "Failed to find chat input field component reference", ValidationLevel.ERROR, true);

        //ChatLog Content
        Transform chatLogTransform = chatTransform.Find("ChatLog").transform;
        Validate(chatLogTransform, "Failed to find chat Log reference", ValidationLevel.ERROR, true);

        Transform chatLogViewportTransform = chatLogTransform.Find("LogViewport").transform;
        Validate(chatLogViewportTransform, "Failed to find Log Viewport reference", ValidationLevel.ERROR, true);

        Transform chatLogContentTransform = chatLogViewportTransform.Find("LogContent").transform;
        Validate(chatLogContentTransform, "Failed to find chat Log Content reference", ValidationLevel.ERROR, true);
        chatLogContent = chatLogContentTransform.gameObject;

        //ChatLog Message Reference
        Transform chatLogMessageReferenceTransform = chatLogTransform.Find("ChatMessageReference").transform;
        Validate(chatLogMessageReferenceTransform, "Failed to find Log Viewport reference", ValidationLevel.ERROR, true);
        chatMessageReference = chatLogMessageReferenceTransform.gameObject;
        chatMessageReference.SetActive(false);

        chatInputFieldComp.SetTextWithoutNotify("");
        SetChatState(false, false);
    }
    private void SetupPlayer1HUDReferences() {

        //Transform player1HUDElementsTransform = transform.Find("Player1HUDElements");
        //Validate(player1HUDElementsTransform, "Failed to find Player1HUDElements reference", ValidationLevel.ERROR, true);
        //player1HUDElements = player1HUDElementsTransform.gameObject;
    }
    private void SetupPlayer2HUDReferences() {

        //Transform player2HUDElementsTransform = transform.Find("Player2HUDElements");
        //Validate(player2HUDElementsTransform, "Failed to find Player2HUDElements reference", ValidationLevel.ERROR, true);
        //player2HUDElements = player2HUDElementsTransform.gameObject;
    }
    private void CheckInput() {
        if (Input.GetKeyDown(KeyCode.Return) && !chatOpened) {
            SetChatState(true, true, true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && chatOpened) {
            SetChatState(false, false);
        }
    }


    //Chat
    private void SetChatState(bool state, bool cursorState, bool select = false) {
        if (!chat)
            return;

        if (state != chat.activeInHierarchy) {
            chatOpened = state;
            chat.SetActive(chatOpened);
        }

        gameInstanceRef.SetCursorState(cursorState);
        if (chatOpened && select)
            chatInputFieldComp.Select();
    }
    public void AddReceivedChatMessage(string message) {
        if (string.IsNullOrEmpty(message))
            return;

        AddMessageToLog(message, false);
        if (!chatOpened)
            ActivateChatPopup(chatMessageReceivedPopupDuration);
    }
    private void AddMessageToLog(string message, bool local = true) {

        GameObject newMessage;
        int currentMessagesCount = chatLogContent.transform.childCount;
        if (currentMessagesCount == chatMessagesRecycleLimit) {
            newMessage = chatLogContent.transform.GetChild(0).gameObject; //Oldest message
            newMessage.transform.SetParent(null);
        }
        else
            newMessage = Instantiate(chatMessageReference);


        TMP_Text textComponent = newMessage.GetComponent<TMP_Text>();
        if (local) {
            textComponent.text = "You: " + message;
            textComponent.color = LocalMessageColor;
        }
        else {
            textComponent.text = "Enemy: " + message;
            textComponent.color = Player2MessageColor;
        }

        newMessage.transform.SetParent(chatLogContent.transform);
        newMessage.SetActive(true);
    }
    private void ActivateChatPopup(float duration) {
        if (duration <= 0.0f || chatOpened)
            return;
        
        SetChatState(true, false);
        chatMessagePopupTimer = duration;
    }
    private void UpdateChatPopupTimer() {
        if (chatMessagePopupTimer <= 0.0f)
            return;

        chatMessagePopupTimer -= Time.deltaTime;
        if (chatMessagePopupTimer <= 0.0f) {
            chatMessagePopupTimer = 0.0f;
            if (chatOpened)
                SetChatState(false, false);
        }
    }
    public void ConfirmChatMessage() {
        string message = chatInputFieldComp.text;
        if (string.IsNullOrEmpty(message))
            return;

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.SendChatMessageServerRpc(message, Netcode.GetClientID());
        AddMessageToLog(message);
        chatInputFieldComp.SetTextWithoutNotify("");
        ActivateChatPopup(chatMessageSentPopupDuration);
    }


    //PlayerHUDElements
    public void UpdatePlayerHealth(float percentage, Player.PlayerID id) {
        if (id == Player.PlayerID.NONE)
            return;

        float value = percentage;
        if (value < 0.0f)
            value *= -1;

        if (id == Player.PlayerID.PLAYER_1)
            player1HealthBarImage.fillAmount = value;
        else if (id == Player.PlayerID.PLAYER_2)
            player2HealthBarImage.fillAmount = value;
    }
}

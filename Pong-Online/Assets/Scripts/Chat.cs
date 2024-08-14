using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chat : NetworkBehaviour
{
    private GameManager gameManager;
    
    [Header("Settings")]
    [SerializeField] private int maxMessages = 25;
    [SerializeField] private List<Message> chatMessages;

    [Header("UI objects")]
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private GameObject chat;
    [SerializeField] private GameObject textObject;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void ClearMessages()
    {
        while (chatMessages.Count > 0)
        {
            Destroy(chatMessages[^1].textObject.gameObject);
            chatMessages.Remove(chatMessages[^1]);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SendMessageClientRPC(FixedString128Bytes message, FixedString128Bytes owner)
    {
        if (chatMessages.Count >= maxMessages)
        {
            Destroy(chatMessages[0].textObject.gameObject);
            chatMessages.Remove(chatMessages[0]);
        }
        
        Message newMessage = new Message(owner, message);

        GameObject chatObject = Instantiate(textObject, chat.transform);
        newMessage.textObject = chatObject.GetComponent<TMP_Text>();
        newMessage.textObject.text = newMessage.owner + ": " + newMessage.message;
        chatMessages.Add(newMessage);
    }

    [Rpc(SendTo.Server)]
    public void SendMessageServerRPC(FixedString128Bytes message, FixedString128Bytes owner)
    {
        SendMessageClientRPC(message, owner);
    }

    public void CheckInputToSend(InputAction.CallbackContext context)
    {
        if (chatInput.isFocused && !chatInput.text.Equals(""))
        {
            FixedString128Bytes owner = gameManager.GetName();
            FixedString128Bytes message = chatInput.text;
            SendMessageServerRPC(message, owner);
            chatInput.text = "";
        }
    }
}

[Serializable]
public class Message
{
    public FixedString128Bytes owner;
    public FixedString128Bytes message;
    public TMP_Text textObject;

    public Message(FixedString128Bytes owner, FixedString128Bytes message)
    {
        this.owner = owner;
        this.message = message;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles communication with the AI backend, maintaining configurable settings and persisting conversation memory.
/// Use <see cref="SendMessageToAI"/> to dispatch player prompts and subscribe to the OnResponseReceived event for UI updates.
/// </summary>
public class AICompanionManager : MonoBehaviour
{
    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public List<Message> messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public List<Choice> choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [Header("Configuration")]
    [Tooltip("Base URL of the AI service endpoint. Example: http://localhost:11434/api/chat for Ollama.")]
    public string apiUrl = "http://localhost:11434/api/chat";

    [Tooltip("Model identifier to request from the AI service (e.g., mistral, gpt-4o, custom model name).")]
    public string modelName = "mistral";

    [Tooltip("Optional authorization token for hosted APIs (leave empty for local services like Ollama).")]
    public string apiKey;

    [Tooltip("Reference to the conversation memory asset that stores chat history between sessions.")]
    public ConversationMemory conversationMemory;

    [Tooltip("Raised whenever the AI returns a new message.")]
    public System.Action<string> OnResponseReceived;

    [Tooltip("Raised when a request starts, useful for showing loading indicators.")]
    public System.Action OnRequestStarted;

    [Tooltip("Raised when a request finishes, whether successful or not.")]
    public System.Action OnRequestCompleted;

    /// <summary>
    /// Dispatches the player's message to the AI service and stores the interaction in memory.
    /// </summary>
    public void SendMessageToAI(string playerMessage)
    {
        if (string.IsNullOrWhiteSpace(playerMessage))
        {
            return;
        }

        if (conversationMemory == null)
        {
            Debug.LogWarning("AICompanionManager requires a ConversationMemory asset to function correctly.");
            return;
        }

        conversationMemory.AddEntry("Player", playerMessage);
        StartCoroutine(SendChatCoroutine(playerMessage));
    }

    private IEnumerator SendChatCoroutine(string playerMessage)
    {
        OnRequestStarted?.Invoke();

        ChatRequest requestPayload = new ChatRequest
        {
            model = modelName,
            messages = BuildMessageList()
        };

        string json = JsonUtility.ToJson(requestPayload);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"AI request failed: {request.error}");
                conversationMemory.AddEntry("Companion", "I tried to answer but something glitched in the ether. Let's try that again!");
                OnResponseReceived?.Invoke("I tried to answer but something glitched in the ether. Let's try that again!");
            }
            else
            {
                string responseText = ParseResponse(request.downloadHandler.text);
                conversationMemory.AddEntry("Companion", responseText);
                OnResponseReceived?.Invoke(responseText);
            }
        }

        OnRequestCompleted?.Invoke();
    }

    /// <summary>
    /// Builds the list of messages expected by common chat completion APIs.
    /// </summary>
    private List<Message> BuildMessageList()
    {
        List<Message> messages = new List<Message>();

        messages.Add(new Message
        {
            role = "system",
            content = "You are an overly enthusiastic but helpful guide in a mysterious new world. Keep replies playful and a little cringe, but still helpful."
        });

        if (conversationMemory != null)
        {
            foreach (ConversationMemory.ConversationEntry entry in conversationMemory.entries)
            {
                messages.Add(new Message
                {
                    role = entry.speaker == "Player" ? "user" : "assistant",
                    content = entry.message
                });
            }
        }

        return messages;
    }

    /// <summary>
    /// Parses the JSON response from the AI service. Adjust this method if your backend returns a different structure.
    /// </summary>
    private string ParseResponse(string json)
    {
        try
        {
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(json);
            if (response != null && response.choices != null && response.choices.Count > 0)
            {
                return response.choices[0].message.content;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse AI response: {ex.Message}");
        }

        return "Uh-oh, my brain fog rolled in. Let's give that another go!";
    }
}

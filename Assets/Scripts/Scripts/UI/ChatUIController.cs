using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game chat interface, wiring up the AI companion manager to Unity UI elements.
/// Attach this script to a Canvas object that contains the chat panel, then assign references in the inspector.
/// </summary>
public class ChatUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Scrollable area used to show the conversation history.")]
    public ScrollRect chatScrollRect;

    [Tooltip("Text component used to display the aggregated conversation.")]
    public Text chatHistoryText;

    [Tooltip("Input field where the player types their message.")]
    public InputField chatInputField;

    [Tooltip("Button that triggers message sending when clicked.")]
    public Button sendButton;

    [Header("Services")]
    [Tooltip("Reference to the AI companion manager responsible for network communication.")]
    public AICompanionManager aiCompanionManager;

    private void Awake()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(HandleSendButtonClicked);
        }
    }

    private void OnEnable()
    {
        if (aiCompanionManager != null)
        {
            aiCompanionManager.OnResponseReceived += HandleAIResponse;
            aiCompanionManager.OnRequestStarted += HandleRequestStarted;
            aiCompanionManager.OnRequestCompleted += HandleRequestCompleted;
        }
    }

    private void OnDisable()
    {
        if (aiCompanionManager != null)
        {
            aiCompanionManager.OnResponseReceived -= HandleAIResponse;
            aiCompanionManager.OnRequestStarted -= HandleRequestStarted;
            aiCompanionManager.OnRequestCompleted -= HandleRequestCompleted;
        }
    }

    /// <summary>
    /// Sends the player's message to the AI and updates the UI to show the new entry.
    /// </summary>
    private void HandleSendButtonClicked()
    {
        if (chatInputField == null || aiCompanionManager == null)
        {
            return;
        }

        string message = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        AppendLineToHistory($"Player: {message}");
        aiCompanionManager.SendMessageToAI(message);
        chatInputField.text = string.Empty;
        chatInputField.ActivateInputField();
    }

    private void HandleAIResponse(string response)
    {
        AppendLineToHistory($"Companion: {response}");
    }

    private void HandleRequestStarted()
    {
        AppendLineToHistory("Companion is thinking...");
    }

    private void HandleRequestCompleted()
    {
        ForceScrollToBottom();
    }

    /// <summary>
    /// Updates the scrollable chat history and keeps the scrollbar at the bottom.
    /// </summary>
    private void AppendLineToHistory(string line)
    {
        if (chatHistoryText == null)
        {
            Debug.LogWarning("ChatUIController requires a Text component to display chat history.");
            return;
        }

        if (chatHistoryText.text.Length > 0)
        {
            chatHistoryText.text += "\n";
        }

        chatHistoryText.text += line;
        ForceScrollToBottom();
    }

    /// <summary>
    /// Forces the ScrollRect to stay scrolled to the bottom when new messages arrive.
    /// </summary>
    private void ForceScrollToBottom()
    {
        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ChatUIAnchorsReset : MonoBehaviour
{
    public RectTransform chatPanel;     // assign ChatPanel (optional)
    public RectTransform scrollView;    // assign Scroll View (optional)
    public RectTransform inputField;    // assign ChatInputField (optional)
    public RectTransform sendButton;    // assign SendButton (optional)

    void Start()
    {
        // Canvas should scale nicely on any resolution
        var canvas = FindObjectOfType<Canvas>();
        if (canvas)
        {
            var scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Auto-find if you didn't assign in Inspector
        chatPanel = chatPanel ? chatPanel : GameObject.Find("ChatPanel")?.GetComponent<RectTransform>();
        scrollView = scrollView ? scrollView : GameObject.Find("Scroll View")?.GetComponent<RectTransform>();
        inputField = inputField ? inputField : GameObject.Find("ChatInputField")?.GetComponent<RectTransform>();
        sendButton = sendButton ? sendButton : GameObject.Find("SendButton")?.GetComponent<RectTransform>();

        // Remove layout components that might fight our manual positions
        if (chatPanel)
        {
            foreach (var lg in chatPanel.GetComponents<UnityEngine.UI.LayoutGroup>()) Destroy(lg);
            var fitter = chatPanel.GetComponent<ContentSizeFitter>(); if (fitter) Destroy(fitter);
        }

        // Snap panel to bottom-left, fixed size
        SetFixed(chatPanel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(16, 16), new Vector2(520, 260));
        // Make Scroll View fill the panel with 8px margins and 52px bottom gap
        SetStretch(scrollView, new Vector2(0, 0), new Vector2(1, 1), left: 8, top: 8, bottom: 52, right: 8);
        // Input + Button along the bottom
        SetFixed(inputField, new Vector2(0, 0), new Vector2(0, 0), new Vector2(8, 8), new Vector2(400, 32));
        SetFixed(sendButton, new Vector2(0, 0), new Vector2(0, 0), new Vector2(416, 8), new Vector2(88, 32));
    }

    void SetFixed(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        if (!rt) return;
        rt.anchorMin = min; rt.anchorMax = max; rt.pivot = new Vector2(0, 0);
        rt.sizeDelta = size; rt.anchoredPosition = pos;
    }

    void SetStretch(RectTransform rt, Vector2 min, Vector2 max, float left, float top, float bottom, float right)
    {
        if (!rt) return;
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = new Vector2(left, bottom);     // left, bottom
        rt.offsetMax = new Vector2(-right, -top);     // -right, -top
    }
}

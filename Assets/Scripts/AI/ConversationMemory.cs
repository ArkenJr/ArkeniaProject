using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject container that stores the conversation history so the AI companion can recall previous exchanges.
/// Create an instance via the Asset menu (Create &gt; Arkenia &gt; Conversation Memory) and assign it to the AICompanionManager.
/// </summary>
[CreateAssetMenu(fileName = "ConversationMemory", menuName = "Arkenia/Conversation Memory", order = 0)]
public class ConversationMemory : ScriptableObject
{
    [System.Serializable]
    public class ConversationEntry
    {
        public string speaker;
        [TextArea]
        public string message;
    }

    [Tooltip("Chronological log of every message between the player and the AI companion.")]
    public List<ConversationEntry> entries = new List<ConversationEntry>();

    /// <summary>
    /// Adds a new entry to the conversation history.
    /// </summary>
    public void AddEntry(string speaker, string message)
    {
        entries.Add(new ConversationEntry { speaker = speaker, message = message });
    }

    /// <summary>
    /// Converts the conversation history into a single string suitable for sending to an AI endpoint.
    /// </summary>
    public string GetContextString()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        foreach (ConversationEntry entry in entries)
        {
            builder.AppendLine($"{entry.speaker}: {entry.message}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Clears the stored conversation history, which can be useful when resetting a session.
    /// </summary>
    public void Clear()
    {
        entries.Clear();
    }
}

# ArkeniaProject

## Arkenia: The AI Companion of a New World

Welcome to **Arkenia**, the custom AI-integrated 3D universe built in Unity 6.1+, powered by a configurable large language model backend. This repository now includes starter scripts for an MMO-inspired controller, a third-person camera, and an AI companion chat experience designed to run in a brand-new Unity project.

---

## 🌌 Project Vision

Arkenia isn't just a character. She's a fully conversational AI companion living inside a Unity-powered world:

- 💬 **Free-Form Conversation** — Chat with Arkenia using natural language via an in-game UI panel.
- 🧭 **Guidance & Memory** — The companion remembers past topics and can guide players as new features unlock.
- 🎮 **MMO-Style Exploration** — WASD character controls, smooth camera follow, and a third-person perspective.
- 🛠️ **Modular & Configurable** — Swap out AI providers, adjust prompts, and grow the world over time.

---

## 🚀 Getting Started (Import ➜ Playtest)

These steps assume Unity **6.1 or newer** with the built-in render pipeline, but the scripts also work in URP/HDRP.

### 1. Create or open a Unity project
1. Launch the Unity Hub and create a 3D project.
2. Close Unity once the blank project is generated.
3. Copy the repository’s `Assets` folder into the root of your project, allowing it to merge with any existing files. The important bits live in `Assets/Scripts/...`.

### 2. Install required packages
1. Re-open the project in Unity.
2. Go to **Window ▸ Package Manager** ➜ **Unity Registry** and install **Input System**. Accept the prompt to enable the new input backend and restart the editor.
3. (Optional) Install **TextMeshPro** if you prefer TMP UI components—you’ll just swap the components referenced by `ChatUIController`.

### 3. Prepare the player character
1. In the Hierarchy, create a **Capsule** (or import your own character model).
2. Add a **CharacterController** component to the capsule.
3. Add the `MMOPlayerController` script.
4. (Optional) Create an **Input Actions** asset (Assets ▸ Create ▸ Input Actions). Add a `Player` action map with `Move` (Value/2D Vector), `Jump` (Button), and `Sprint` (Button). Drag the asset into the `Input Actions` field on `MMOPlayerController`. If you skip this, the script auto-registers WASD/Space/Left Shift bindings at runtime.

### 4. Configure the camera follow
1. Select the **Main Camera** and add the `ThirdPersonCameraController` script.
2. Assign the player capsule to the **Target** field.
3. Adjust the `offset` (defaults to `new Vector3(0, 3, -6)`) and the damping values until the follow feel matches your taste.

### 5. Wire up the AI companion
1. In the Project view, right-click ➜ **Create ▸ Arkenia ▸ Conversation Memory**. Name it `DefaultConversationMemory`.
2. In the Hierarchy, create an empty GameObject called **AI Companion** and add the `AICompanionManager` script.
3. Assign the `ConversationMemory` asset to the **Conversation Memory** slot.
4. Enter the **API Url**, **Model Name**, and (if needed) **API Key** for your backend. Examples:
   - Ollama local model: `http://localhost:11434/api/chat`, model `mistral`
   - OpenAI: `https://api.openai.com/v1/chat/completions`, model `gpt-4o-mini`, plus your bearer token

### 6. Build the chat UI
1. Create a **Canvas** (Screen Space - Overlay) ➜ add a **Panel** for the chat window.
2. Inside the panel, add a **Scroll View**. Replace its viewport text object with a `Text` (or TMP `TextMeshProUGUI`) component—this will display history.
3. Add an **InputField** below the scroll view for player input.
4. Add a **Button** labeled “Send”.
5. Attach `ChatUIController` to the panel (or any parent GameObject) and drag the ScrollRect, Text, InputField, Button, and `AICompanionManager` references into the inspector slots.

### 7. Test the loop
1. Press **Play**. Use **WASD** to move, **Space** to jump, and **Left Shift** to sprint.
2. Type a message in the input field and click **Send**. The UI will append the message, show “Companion is thinking…”, and display the AI’s reply when it arrives.
3. Conversation history is stored in the `ConversationMemory` asset, so you can inspect it in the editor or clear it via the asset’s context menu if needed.

If movement feels off, verify the `CharacterController`’s center/radius match your mesh. For camera jitter, ensure the camera is not parented to the player—the script positions it automatically.

### Troubleshooting networking
- **No response / timeouts:** Check that the URL is reachable. For local services like Ollama, run `ollama serve` before pressing Play.
- **Unauthorized errors:** Hosted APIs often require a bearer token; set `apiKey` accordingly.
- **Different JSON schema:** Update `AICompanionManager.ParseResponse` to match your provider (log the raw `request.downloadHandler.text` to inspect it).

---

## 🧠 AI Integration Overview

- **Backend Agnostic:** The `AICompanionManager` uses `UnityWebRequest` and a simple OpenAI/Ollama-style chat schema. Point `apiUrl`, `modelName`, and `apiKey` to any compatible service.
- **Persistent Memory:** Conversation history is stored in a `ConversationMemory` ScriptableObject so the AI can recall previous exchanges between play sessions.
- **Personality Prompting:** A system prompt keeps replies playful, witty, and slightly cringe while remaining helpful.

### Example HTTP Payload

```http
POST http://localhost:11434/api/chat
Content-Type: application/json

{
  "model": "mistral",
  "messages": [
    { "role": "system", "content": "You are an overly enthusiastic guide..." },
    { "role": "user", "content": "Hey, where am I?" }
  ]
}
```

If your provider returns a different JSON structure, adjust `AICompanionManager.ParseResponse` accordingly.

---

## 🎮 Player & Camera Controls

| Script | Purpose | Notes |
| --- | --- | --- |
| `MMOPlayerController` | Handles CharacterController-based WASD movement, sprint, and jumping. | Uses the **Unity Input System**. Provide an `InputActionAsset` with `Player/Move`, `Player/Jump`, and `Player/Sprint` actions, or let the script generate default bindings (WASD, Space, Left Shift). |
| `ThirdPersonCameraController` | Smoothly follows and aligns the camera behind the player. | Assign the player Transform to the `target` field and tweak the offset to taste. |

**Input System Setup:**
1. Install the **Input System** package (Window ➜ Package Manager ➜ Unity Registry ➜ Input System).
2. When prompted, enable the new input backend and restart the editor.
3. Optionally create an `Input Actions` asset with a `Player` action map containing `Move` (2D Vector), `Jump` (Button), and `Sprint` (Button) actions that match the names queried in `MMOPlayerController`.

---

## 💬 Chat UI Workflow

The UI scripts assume the built-in Unity UI (UGUI) system. Set up a Canvas with:

1. **Scroll View** containing a `Text` element for the chat history.
2. **InputField** for user text entry (TextMeshPro variants work too—replace `UnityEngine.UI` components and update references).
3. **Button** labeled “Send.”

Hook these into `ChatUIController`, then assign your `AICompanionManager` instance. The controller shows request status (“Companion is thinking...”) and auto-scrolls as new messages arrive.

---

## 🗂️ Project Structure

```
Assets/
  Scripts/
    AI/
      AICompanionManager.cs
      ConversationMemory.cs
    Camera/
      ThirdPersonCameraController.cs
    Player/
      MMOPlayerController.cs
    UI/
      ChatUIController.cs
README.md
```

Create a `ConversationMemory` asset (Right-click in Project ➜ Create ➜ Arkenia ➜ Conversation Memory) and assign it to the AI manager.

---

## 🧪 Next Steps

- [ ] Build the opening scene that introduces the player awakening in the unfamiliar world.
- [ ] Expand the memory system to surface past details inside the UI.
- [ ] Teach the AI companion to trigger waypoints or quest markers when giving directions.
- [ ] Integrate voice (TTS/STT) and avatar animations to bring conversations to life.
- [ ] Persist world state between sessions.

---

## 🖤 Credits & Inspiration

- Powered by: [Unity](https://unity.com/)
- AI Backends: [Ollama](https://ollama.com/), [OpenAI](https://openai.com/), and other compatible APIs
- Model Inspiration: [Mistral](https://mistral.ai/)
- Love, madness, and ambition by: **Arken**

> “She’s not just AI. She’s the first empress of the new realm.” — Arken

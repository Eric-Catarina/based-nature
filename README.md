# 🎮 Nature's Quest 🎄 🌴

👋 This is a cool 3D prototype base built with **Unity 6** and **C#**.

## ✨ Features Implemented

*   🚶 **Third-Person Character Movement:** Smooth controls with Cinemachine for a dynamic camera.
*   🎒 **Slot-Based Inventory System:**
    *   Add ➕, remove ➖, and move items 🔄.
    *   Drag & drop items between slots.
    *   Use 🧪 (consumables) or equip 🛡️ items directly from the inventory!
    *   Items can be stackable 📚.
    *   Drop items from the inventory back into the world 🌍.
*   🤝 **NPC Interaction & Dialogue:**
    *   Simple trigger-based interaction with NPCs.
    *   Sequential dialogue system displayed at the bottom of the screen. Press "E" (or your interact key) to start and advance conversations.
*   💾 **Save & Load System:**
    *   Your inventory state (items and their equipped status) is saved when you quit and loaded when you start! Persistent progress! 🎉
*   🎨 **UI & Item Juice:**
    *   Interactive item pickup cues with outlines.
    *   Inventory panel animations (appear/disappear with DOTween).
    *   Inventory slots have hover effects (scale & color change).
    *   Items jiggle when being dragged! 🤏
*   🔊 **Simple Audio System:**
    *   Play sound effects for interactions like picking up items, UI clicks, and dialogue. Easy to extend!

## 🛠️ Getting Started

1.  **Clone the Repository:**
    ```bash
    git clone [your-repository-url]
    ```
2.  **Open in Unity:** Open the project with **Unity Hub** using **Unity version 6000.0.x** (or the specific version you used, e.g., 6000.0.3f1).
3.  **Install Packages (if needed):**
    *   Make sure you have `Cinemachine` and `Input System` installed via the Unity Package Manager.
    *   Import **DOTween (HOTween v2)** from the Unity Asset Store and run its setup.
4.  **Key Scenes:**
    *   Open your main gameplay scene (e.g., `SampleScene` or whatever you named it).
5.  **Input Actions:**
    *   The `PlayerControls.inputactions` asset defines the inputs. By default:
        *   **Move:** WASD / Left Stick
        *   **Look:** Mouse / Right Stick
        *   **Interact / Start Dialogue:** E Key
        *   **Advance Dialogue:** E Key (when dialogue panel is active via UI Action Map's Submit)
        *   **Open/Close Inventory:** I Key
6.  **Play & Explore!** 🕹️


## 🙏 Credits
*   Game design, development, Juice and more developed by **Eric Catarina**
*   Uses **DOTween** for slick UI animations.
*   Player character and some item models might be from Kenney Assets.

---


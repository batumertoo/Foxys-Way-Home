<div align="center">

# 🦊 Foxy's Way Home — SunnyLand

**A charming 2D platformer where a brave little fox dashes, jumps, and fights its way home through a vibrant pixel-art world.**

![Unity](https://img.shields.io/badge/Unity-2020.3.9f1-000000?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Web-blue)
![License](https://img.shields.io/badge/License-MIT-green)

<img src="Screenshots/screenshot_1.png" alt="Gameplay Screenshot 1" width="45%"/>
&nbsp;&nbsp;
<img src="Screenshots/screenshot_2.png" alt="Gameplay Screenshot 2" width="45%"/>

</div>

---

## 📖 Project Overview

**Foxy's Way Home** is a feature-rich 2D side-scrolling platformer built with **Unity**. Developed as part of the *Introduction to Game Programming (BIM449)* course, the project goes well beyond a basic platformer — it features a full progression system with a shop, an inventory of unlockable abilities, multiple enemy types with distinct AI behaviors, breakable tiles, checkpoints, and a persistent cross-scene economy.

Players guide a fox through hand-crafted levels filled with coins, cherries, and hostile creatures. Coins fuel a shop where permanent upgrades like **Double Jump** and **Dash** can be unlocked, while consumable power-ups like the **Magnet** pull collectibles toward the player. Each level ends at an exit trigger, carrying progress forward into the next stage.

> **Art Assets:** [Sunnyland Asset Pack](https://assetstore.unity.com/packages/2d/characters/sunny-land-103349) by Ansimuz  
> **Tutorial Reference:** [Platformer Tutorial Series](https://www.youtube.com/playlist?list=PLpj8TZGNIBNy51EtRuyix-NYGmcfkNAuH)

---

## 🛠️ Tech Stack

| Technology | Details |
|---|---|
| **Engine** | Unity 2020.3.9f1 |
| **Language** | C# |
| **Physics** | Unity Physics 2D (Rigidbody2D, Collider2D, Raycasts) |
| **Camera** | Cinemachine 2.5.0 (smooth follow) |
| **UI** | TextMesh Pro 3.0.6, Unity UGUI |
| **2D Toolkit** | 2D Animation, 2D Sprite, 2D SpriteShape, 2D Tilemap, 2D Pixel Perfect |
| **Level Design** | Unity Tilemap with Tile Palette |
| **Input** | Unity Legacy Input Manager |
| **IDE Support** | Visual Studio, Rider, VS Code |

---

## ✨ Key Features

### 🏃 Player Mechanics
- **Fluid Movement** — Responsive run, jump, and air-control physics (base speed `9`, air control `0.8×`)
- **Sprint & Stamina** — Hold `Shift` to sprint at `2.5×` speed; stamina drains at 65/sec and regenerates at 10/sec
- **Dash** — Unlockable ability with invulnerability frames, cooldown system, and directional control
- **Double Jump** — Unlockable permanent upgrade for reaching higher platforms
- **Wall Slide & Wall Jump** — Cling to walls and leap off with distinct wall-jump forces

### 🪙 Economy & Progression
- **Coin System** — Collect coins from pickups and enemy defeats; persists across levels
- **Shop** — Spend coins on permanent upgrades (Double Jump, Dash) and consumables (Magnet)
- **Inventory** — Manage unlocked abilities and consumable counts via a dedicated UI panel
- **Magnet Power-Up** — Pulls all nearby collectibles within a 10-unit radius toward the player for 10 seconds

### 👾 Enemy AI
- **Frog** — Ground patrol enemy that jumps between boundary points; defeated by stomping
- **Eagle** — Ranged flying enemy that fires homing projectiles when the player is within range
- **Bat** — Aerial patrol enemy that can only be defeated from above; rewards bonus coins with floating text

### ❤️ Health & Damage
- **Health Bar UI** — Persistent HP system (0–100) with visual bar and floating damage/heal numbers
- **Invulnerability Frames** — 1.5-second damage cooldown prevents rapid hits
- **Non-Lethal Fall Zones** — Bottomless pits deal damage and respawn the player instead of instant death
- **Cherry Pickups** — Restore 5 HP on collection

### 🗺️ Level Design
- **Breakable Tiles** — Platforms that crumble underfoot and respawn after a delay
- **Checkpoints** — Mid-level save points with audio/visual activation feedback
- **Scene Transitions** — Trigger-based level exits with a Level Complete screen
- **Tilemap-Based Levels** — Built with Unity's Tilemap system for efficient, tile-based world design

### 🎵 Audio & Polish
- **Footstep sounds** triggered by walk animation events
- **Jump, coin, explosion, and checkpoint SFX**
- **Floating damage text** — Color-coded numbers (🟡 coins, 🔴 damage, 🟢 healing) at impact location

---

## 🏗️ Technical Architecture

The project employs several well-known design patterns to manage complexity across scenes and systems:

### 🔷 Singleton Pattern
Core managers use `DontDestroyOnLoad` singletons to persist state across scene transitions:

```
PermanentUI.perm     →  Cross-scene coin tracking & UI
HealthSystem         →  Persistent HP across levels
MenuManager          →  Pause, level-complete, and menu screens
InventoryManager     →  Powerup unlocks & consumable counts
```

### 🔷 State Machine (Player Controller)
The player uses an enum-based state machine (`idle → running → jumping → falling → hurt`) synchronized with the Animator via integer parameters. Physics queries (grounded checks, velocity) drive automatic state transitions each frame.

### 🔷 Inheritance (Enemy System)
All enemies inherit from the `Enemy` base class, which provides shared references (`Animator`, `Rigidbody2D`, `AudioSource`) and a common `JumpedOn()` death method. Subclasses (`Frog`, `Eagle`, `Bat`) override `Start()` and implement unique movement and attack behaviors.

### 🔷 Component-Based Design
Unity's component architecture is used throughout — each behavior (movement, health, camera, shop, checkpoint) lives in its own `MonoBehaviour` script attached to the relevant GameObject.

### System Interaction Diagram

```
┌─────────────┐    coins     ┌──────────────┐    purchase    ┌───────────────┐
│   Player    │─────────────▶│ PermanentUI  │◀──────────────│  ShopManager  │
│ Controller  │              │  (Singleton) │               │               │
└──────┬──────┘              └──────────────┘               └───────────────┘
       │                                                            │
       │ damage/heal                                         unlock │
       ▼                                                            ▼
┌──────────────┐                                        ┌───────────────────┐
│ HealthSystem │                                        │ InventoryManager  │
│ (Singleton)  │                                        │   (Singleton)     │
└──────────────┘                                        └───────────────────┘
       │                                                            │
       │ death                                        apply powerup │
       ▼                                                            ▼
┌──────────────┐         scene load          ┌──────────────────────┐
│ MenuManager  │◀───────────────────────────▶│   PlayerController   │
│ (Singleton)  │                             │  (dash, magnet, dblj)│
└──────────────┘                             └──────────────────────┘
```

---

## 🚀 Installation & Setup

### Prerequisites

- **Unity Hub** — [Download here](https://unity.com/download)
- **Unity 2020.3.9f1** (LTS) — Install via Unity Hub
- **Git** — [Download here](https://git-scm.com/)

### Steps

```bash
# 1. Clone the repository
git clone https://github.com/batumertoo/Foxys-Way-Home.git

# 2. Open the project
#    Launch Unity Hub → Click "Open" → Select the cloned folder

# 3. Wait for Unity to import all assets and compile scripts

# 4. Open the main scene
#    In the Project window, navigate to Assets/Scenes/ and double-click the first level scene

# 5. Press the ▶ Play button in the Unity Editor to run the game
```

> **Note:** If prompted to upgrade the project, select "Continue" to allow Unity to upgrade project settings. The game was built with Unity 2020.3.9f1 but is compatible with newer 2020.3.x LTS releases.

---

## 🎮 Controls & Input

| Action | Key(s) | Notes |
|---|---|---|
| **Move Left / Right** | `A` / `D` or `←` / `→` | Axis-based with acceleration |
| **Jump** | `Space` | Hold for higher jump; double jump if unlocked |
| **Sprint** | `Left Shift` (hold) | 2.5× speed; drains stamina bar |
| **Dash** | `Left Ctrl` | Requires unlock from Shop; has cooldown |
| **Pause / Resume** | `Escape` | Opens pause menu overlay |
| **Inventory** | UI Button | Toggle powerup & unlock panel |
| **Shop** | UI Button | Browse and purchase upgrades |

---

## 📁 Project Structure

```
Assets/
├── Scripts/                  # All C# game logic (19 scripts)
│   ├── PlayerController.cs   # State machine, movement, combat, powerups
│   ├── Enemy.cs              # Base enemy class with shared death logic
│   ├── Frog.cs               # Patrol-jumping ground enemy
│   ├── Eagle.cs              # Ranged flying enemy with projectiles
│   ├── EagleShot.cs          # Homing projectile behavior
│   ├── Bat.cs                # Aerial patrol enemy (bonus coins)
│   ├── HealthSystem.cs       # Persistent HP, damage, healing, floating text
│   ├── MenuManager.cs        # Main menu, pause, level complete UI
│   ├── ShopManager.cs        # In-game shop for upgrades
│   ├── InventoryManager.cs   # Powerup management & persistence
│   ├── SceneChange.cs        # Level transitions & scene loading
│   ├── CameraController.cs   # Smooth player-following camera
│   ├── Fall.cs               # Non-lethal fall zone trigger
│   ├── Checkpoint.cs         # Mid-level respawn points
│   ├── BreakableTile.cs      # Destructible & respawning platforms
│   └── PermanentUI.cs        # Singleton coin tracker & display
├── Prefabs/                  # Reusable GameObjects (Player, enemies, UI)
├── Scenes/                   # Game levels and menus
├── Audio/                    # Sound effects & background music
├── Sunnyland/                # Sprite sheets, animations, tilesets
├── Materials/                # Physics materials
├── HealthBar/                # Health bar UI assets
├── Fonts/                    # UI typography
├── TextMesh Pro/             # TMP resources & shaders
└── Tile Palette/             # Tilemap brush assets
```

---

## 🗺️ Future Roadmap

Based on the current architecture and codebase, the following enhancements would build naturally on the existing systems:

| Priority | Feature | Description |
|---|---|---|
| 🟢 | **Save & Load System** | Persist coins, unlocks, and level progress to disk using `PlayerPrefs` or JSON serialization so players can resume across sessions. |
| 🟡 | **New Enemy Types** | Leverage the `Enemy` base class to add boss fights or enemies with patrol + projectile hybrid behavior. |
| 🔵 | **Mobile Touch Controls** | The project already supports auto-rotation; add on-screen virtual joystick and action buttons for full mobile gameplay. |
| 🟣 | **Level Select & Star Rating** | Add a level selection screen with a 1–3 star rating based on coins collected, time, and HP remaining for replayability. |

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to fork the repository and open a pull request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📜 Credits

- **Art Assets:** [Sunnyland Asset Pack](https://assetstore.unity.com/packages/2d/characters/sunny-land-103349) by Ansimuz
- **Tutorial Reference:** [Platformer Tutorial Series](https://www.youtube.com/playlist?list=PLpj8TZGNIBNy51EtRuyix-NYGmcfkNAuH)
- **Engine:** [Unity Technologies](https://unity.com/)

---

<div align="center">

*Built with ❤️ using Unity*

</div>

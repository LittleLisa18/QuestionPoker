# Question Poker

Question Poker is a narrative-driven 2D card game built with Unity. It combines poker-inspired hand comparison and betting with dialogue, boss encounters, collectible tool cards, and animated effects.

## Features

- card dealing, dragging, selection, flipping, and hand comparison;
- player and enemy betting with score and chip management;
- tool cards for peeking, swapping, protecting, changing suits, doubling or halving chips, and adding or removing cards;
- dialogue-driven stage transitions and story sequences;
- scripted enemy behavior and smart card selection;
- sound, music, particle, coin, and victory effects.

## Built with

- Unity `2022.3.58f1c1`
- C#
- Unity UI and TextMesh Pro
- DOTween
- Unity Timeline and Video

## Scene flow

The enabled build scenes are:

1. `Login`
2. `StoryIntro`
3. `Room J`
4. `Table_J`
5. `J_Joker`
6. `Table_Joker`
7. `Win`

`Assets/Scenes/Login.unity` is the entry scene.

## Repository structure

| Path | Description |
| --- | --- |
| `Assets/Scenes/` | Menus, story rooms, card tables, and ending scenes |
| `Assets/Scripts/Cards/` | Dealing, collection, selection, drag-and-drop, and comparison |
| `Assets/Scripts/Table/` | Game loop, betting, scoring, AI, questions, and table audio |
| `Assets/Scripts/Button/` | UI controls and tool-card behavior |
| `Assets/Scripts/Dialogue/` | Story progression and stage transitions |
| `Assets/Scripts/Effect/` | Interface and reward animations |
| `Assets/Resources/` | Runtime-loaded art, audio, fonts, and game resources |
| `Packages/` | Unity package manifest and lockfile |
| `ProjectSettings/` | Versioned Unity configuration |

## Getting started

1. Clone the repository:

   ```bash
   git clone https://github.com/LittleLisa18/QuestionPoker.git
   ```

2. Add the repository folder in Unity Hub.
3. Open it with Unity `2022.3.58f1c1`.
4. Allow Unity to restore packages and import assets.
5. Open `Assets/Scenes/Login.unity` and press Play.

For a standalone build, confirm that all seven scenes are enabled and ordered correctly under **File → Build Settings**.

## Large local assets

GitHub's regular per-file limit prevents this repository from including:

- `Assets/Resources/Video/WinEnd.mp4`
- `Assets/Resources/Video/开头 1.mp4`
- `Assets/Resources/Video/StoryIntro.mp4`
- `Assets/Fonts/games/MSYH SDF.asset`

Copy these files from the original local project when a complete cinematic build is required. The font atlas can also be regenerated from its source font through TextMesh Pro.

## Credits

Question Poker was developed during Tencent's Spark Program. The project includes third-party Unity assets and packages; original notices and license files remain alongside the corresponding assets.

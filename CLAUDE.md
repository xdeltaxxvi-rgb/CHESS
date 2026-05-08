# CHESS — Project Brain

> Read this entire file before touching any code. Every agent session starts here.

---

## What This Project Is

A 3D story-driven chess game for Android and iOS. The core mechanic is standard chess wrapped in a narrative about two brothers.

**The Story:**
- **White** = the younger brother (the player). Rules a small kingdom. Earnest, determined.
- **Black** = the older brother (the antagonist). Once grew up with White. Now controls the world — for reasons revealed through the story.
- The game is White's quest to expand his kingdom and ultimately confront his older brother.
- Each level = a different enemy faction (goblins, orcs, elves, monsters…) en route to the older brother.
- Chess rules never change. Only the visual skin of the enemy pieces changes per faction.

**Platform:** Android (primary), iOS (secondary). PC is post-launch.
**Orientation:** Portrait only. This is non-negotiable.
**Style:** 3D low-poly stylized. Isometric orthographic camera.

---

## Tech Stack

| Tool | Version | Purpose |
|---|---|---|
| Unity | **6000.4.6f1 (Unity 6)** | Game engine |
| IDE | **Visual Studio 2026** | C# development |
| Render Pipeline | URP (Universal) | Mobile-optimised rendering |
| Language | C# (.NET Standard 2.1) | All game logic |
| Dialogue | Yarn Spinner (free) | Story narration, pre/post-match dialogue |
| Cutscenes | Unity Timeline | Animated story beats |
| AI | Custom C# Minimax + Alpha-Beta | Chess opponent |
| Save | JSON → Application.persistentDataPath | Game progress |
| IAP | Unity IAP | Chapter unlock purchases |
| Analytics | Firebase Analytics | Player behaviour tracking |
| Version Control | Git (GitHub) | xdeltaxxvi-rgb/CHESS |
| Build targets | Android ARM64+ARMv7 (IL2CPP) API 26+, iOS arm64 (IL2CPP) iOS 16.0+ | Release builds |

---

## Repository Layout

```
CHESS/                          ← Git repo root
├── CHESS/                      ← Unity project (subfolder created by Unity Hub)
│   ├── Assets/
│   │   ├── Scripts/
│   │   │   ├── Core/
│   │   │   │   ├── Board/          ← BoardManager.cs, BoardConstants.cs, BoardVisualizer.cs, Square.cs, SquareColor.cs
│   │   │   │   ├── Pieces/         ← Piece.cs (base), PieceType.cs, PieceColor.cs, PieceView.cs, King.cs, Queen.cs, Rook.cs, Bishop.cs, Knight.cs, Pawn.cs
│   │   │   │   └── GameManager.cs  ← Central authority: turn system, game state, win/loss
│   │   │   ├── AI/
│   │   │   │   └── ChessAI.cs      ← IChessAI interface + minimax implementation
│   │   │   ├── Input/
│   │   │   │   ├── TileSelector.cs ← Raycasting, tile/piece hit detection
│   │   │   │   └── MoveSelector.cs ← Valid move highlighting, move execution
│   │   │   ├── Story/
│   │   │   │   └── NarrativeController.cs ← Story state, Yarn triggers, faction swaps
│   │   │   ├── UI/
│   │   │   │   └── HUDManager.cs   ← In-game HUD, menus, dialogue overlay
│   │   │   └── Save/
│   │   │       └── SaveManager.cs  ← Serialise/deserialise game progress to JSON
│   │   ├── Prefabs/
│   │   │   ├── Pieces/             ← One prefab per piece type per faction
│   │   │   └── UI/
│   │   ├── Scenes/
│   │   │   ├── MainMenu.unity
│   │   │   ├── ChapterSelect.unity
│   │   │   └── Game.unity          ← Single game scene; faction/chapter loaded at runtime
│   │   ├── Art/
│   │   │   ├── Models/             ← FBX files, organised by faction
│   │   │   ├── Materials/          ← One material per faction team
│   │   │   └── Textures/           ← 1024x1024 atlases per faction
│   │   ├── Audio/
│   │   │   ├── Music/              ← OGG loop tracks per chapter
│   │   │   └── SFX/                ← Piece movement, capture, check, UI sounds
│   │   └── Dialogue/               ← .yarn scripts per chapter (Ch1.yarn, Ch2.yarn, …)
│   ├── Packages/               ← Unity package manifest (URP etc.)
│   └── ProjectSettings/        ← Platform settings (Android/iOS, quality, graphics)
├── docs/
│   └── TEST_PLAN.md            ← Created in Phase 8
├── scripts/
│   └── create-issues.sh        ← One-time setup script (already run, do not re-run)
├── .github/
│   ├── ISSUE_TEMPLATE/         ← task.yml, bug_report.yml
│   ├── pull_request_template.md
│   └── workflows/
│       └── project-automation.yml ← Auto-moves project board cards
├── CLAUDE.md                   ← THIS FILE
├── .gitignore                  ← Repo-level ignores (build artefacts, IDE files)
└── .gitattributes              ← Unity binary files + LFS rules for assets
```

> **Note:** All Unity file paths (Assets/, ProjectSettings/, etc.) are relative to `CHESS/CHESS/`, not the repo root.

---

## Architecture Decisions (Do Not Change Without Good Reason)

### Board & Pieces
- Board is a **2D array `Square[8,8]`** — file 0–7 (A–H), rank 0–7 (1–8).
- `Square` is a **struct**, not a MonoBehaviour. It holds data only.
- `Piece` is an **abstract C# class** (not MonoBehaviour). Each piece type is a subclass.
- `GetValidMoves(Square[,] board)` is abstract on `Piece` — every subclass implements its own movement rules. The raw `Square[8,8]` array is passed directly; there is no `Board` wrapper class.
- **`Chess.Input` namespace**: Any code inside this namespace must qualify Unity's input class as `UnityEngine.Input.` (e.g. `UnityEngine.Input.GetMouseButtonDown(0)`) to avoid ambiguity with the namespace name itself.
- **`Square` struct mutation**: To modify a field on a Square, always copy→modify→write back: `var sq = _board[f,r]; sq.Piece = p; _board[f,r] = sq;`. Direct field assignment on an array element of a value type does not compile.
- The board **data** (C# classes) is completely separate from the **visual** (GameObjects/prefabs). `BoardManager` owns data; a separate `BoardVisualizer` syncs GameObjects to data.

### Game Flow
- `GameManager` is the single source of truth for: whose turn it is, whether game is over, current board state.
- Turn order: `White → Black → White`. White always goes first.
- After every move, `GameManager` checks: Is the opponent in check? Is it checkmate? Is it stalemate?
- Illegal moves are **impossible to execute** — they are filtered out before being offered to the player.

### AI
- AI lives behind `IChessAI` interface. `GameManager` calls `GetBestMove()` — never the concrete class directly.
- Difficulty = minimax search depth. Chapter maps to depth: Ch1→2, Ch2-3→3, Ch4-5→4, Ch6-7→5, Ch8→6.
- AI computation runs on a **background thread** (C# `Task`). Main thread shows "Thinking…" and waits.
- AI must **never** return an illegal move. If no moves available, return null (checkmate/stalemate handled by GameManager).

### Story & Factions
- `NarrativeController` reads the current chapter from `SaveManager` and:
  1. Swaps the enemy piece prefabs to the correct faction set
  2. Loads the correct `.yarn` dialogue file
  3. Triggers pre/post-match Yarn dialogue via `DialogueRunner`
- **Faction = visual skin only.** Chess rules never change between factions.
- Story flags are a `Dictionary<string, bool>` serialised into the save file.

### Save System
- Single JSON file at `Application.persistentDataPath/save.json`.
- `SaveData` class: `int currentChapter`, `List<int> completedChapters`, `Dictionary<string, bool> storyFlags`, `List<string> iapUnlocks`.
- Auto-save on: chapter complete, chapter start, app pause (`OnApplicationPause`).

### Camera
- **Orthographic** camera. Position `(0, 18, 0)`, rotation `(30°, 45°, 0°)`.
- Orthographic size is calculated at runtime from screen aspect ratio to keep the board in the upper ~70% of portrait screen.
- Do not use perspective cameras for the board scene.

### Performance Targets
| Metric | Target |
|---|---|
| Draw calls (board scene) | ≤ 20 |
| Triangles (board scene) | ≤ 30k |
| Texture memory | ≤ 150 MB |
| FPS (mid-range Android) | 60 during gameplay, 30 during dialogue |
| AI move time (depth 6) | < 3 seconds |
| App size | < 150 MB |

---

## Coding Conventions

- **Namespaces:** `Chess.Core`, `Chess.AI`, `Chess.Input`, `Chess.Story`, `Chess.UI`, `Chess.Save`
- **Naming:** PascalCase for classes/methods/properties. camelCase for local vars and private fields (`_camelCase` prefix for private instance fields).
- **No MonoBehaviour on data classes.** `Square`, `Piece` and its subclasses, `SaveData`, `Move` — these are plain C# objects.
- **MonoBehaviours** are only for: `BoardManager`, `BoardVisualizer`, `PieceView`, `GameManager`, `TileSelector`, `MoveSelector`, `NarrativeController`, `HUDManager`, `SaveManager`, `AudioManager`.
- **No singletons** except `GameManager`, `SaveManager`, `AudioManager` — accessed via static `Instance` property with lazy init.
- **No magic numbers.** Board size = `BoardConstants.Size` (8). Piece values = constants in `PieceValues`.
- **Comments:** Only when the WHY is non-obvious. No explaining what the code does.
- **Error handling:** Only at system boundaries (file I/O, IAP callbacks, network). Internal chess logic uses asserts.

---

## Development Phases

| Phase | Scope | Milestone Due |
|---|---|---|
| **1 — Chess Engine Core** | Board, pieces, all move rules, check/checkmate/stalemate, special moves | 2026-06-04 |
| **2 — AI Opponent** | Minimax + alpha-beta, difficulty scaling, async | 2026-06-18 |
| **3 — 3D Art & Camera** | Camera, board model, piece models per faction, animations | 2026-07-16 |
| **4 — Story & Narrative** | Yarn Spinner, story bible, dialogue scripts, chapter unlocks | 2026-08-13 |
| **5 — UI/UX** | Main menu, chapter select, HUD, game over, settings | 2026-09-03 |
| **6 — Audio** | Music per chapter, SFX, AudioManager | 2026-09-17 |
| **7 — Mobile Optimization** | Profiling, draw calls, texture compression, build config | 2026-10-01 |
| **8 — QA & Bug Fix** | Full regression, chess rules, story, UI, AI, save/load | 2026-10-15 |
| **9 — Monetization & Analytics** | Unity IAP, Firebase, chapter unlock flow | 2026-10-22 |
| **10 — Launch** | Store listings, release builds, beta, submission | 2026-11-05 |

**Current phase: Phase 1 — Chess Engine Core**

All 89 tasks are tracked as GitHub Issues at: https://github.com/xdeltaxxvi-rgb/CHESS/issues
Project board: https://github.com/users/xdeltaxxvi-rgb/projects/1

---

## How to Work on an Issue (Agent Protocol)

Every agent session follows this exact protocol:

```
1. Read CLAUDE.md (this file) — understand the full context
2. Read the GitHub issue — understand the specific task, acceptance criteria, dependencies
3. Check dependencies — read the code from any issues this depends on (they are already merged)
4. Read only the relevant existing files — do not read the whole codebase
5. Implement the task to meet all acceptance criteria
6. Run any existing tests — do not break what already works
7. Create a PR — title: "[PX-TXX] Brief description", body from PR template
8. Stop — do not start the next issue. One issue per session.
```

**Never:**
- Implement more than the issue asks for
- Rename or refactor code not related to the current issue
- Create files or folders not in the architecture above without updating CLAUDE.md
- Merge your own PR — the human reviews first

---

## Every-3-Issues Cleanup Protocol

After every 3 merged issues, run this checklist before starting the next issue. Create a `chore/cleanup-after-issues-X-Y` branch and PR for any fixes found.

**1. CLAUDE.md audit**
- [ ] Repository layout still matches the actual filesystem
- [ ] All versions (Unity, packages, tools) still accurate
- [ ] Architecture decisions section still reflects the code that was actually written
- [ ] "Current phase" updated if a phase just completed

**2. PR queue**
- [ ] No open PRs are stacked — everything that's approved is merged to `main`
- [ ] `main` builds without errors

**3. Config files**
- [ ] `.gitignore` patterns still apply (no dead rules, no missing rules)
- [ ] `.gitattributes` LFS rules cover any new file types added

**4. Naming consistency**
- [ ] Scene names, folder names, and class names match what CLAUDE.md specifies
- [ ] No `SampleScene`, `NewScript`, or placeholder names left in the project

**5. Lessons learned**
- [ ] Any constraint discovered during the last 3 issues (engine limits, API changes, build surprises) added to CLAUDE.md under Architecture Decisions

If nothing needs changing, still create a one-line commit `chore: cleanup check after issues #N–#N — no changes needed` so there is a record that the check was done.

---

## Git Workflow

- **Branch naming:** `phase1/P1-T04-board-manager`, `phase2/P2-T03-minimax`, etc.
- **Commit style:** `feat: ...`, `fix: ...`, `chore: ...`, `test: ...` (conventional commits)
- **One PR per issue.** PRs close their issue via `Closes #N` in the PR body.
- **Never force-push to main.**
- **main branch** = always buildable and working.

---

## Key Contacts & Resources

- **GitHub repo:** https://github.com/xdeltaxxvi-rgb/CHESS
- **Project board:** https://github.com/users/xdeltaxxvi-rgb/projects/1
- **Issue tracker:** https://github.com/xdeltaxxvi-rgb/CHESS/issues
- **Unity version:** 6000.4.6f1 (Unity 6) — install via Unity Hub
- **IDE:** Visual Studio 2026
- **Yarn Spinner docs:** https://www.yarnspinner.dev/
- **Unity URP docs:** https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest

---

## Story Bible Summary (Full doc to be written in Issue #48)

- White and Black are biological brothers, raised together in the same household.
- They were separated by an event that will be revealed through story narration (not yet written — see Issue #48).
- White built a small but honest kingdom. Black, after the separation, accumulated power through means that corrupted him.
- The player discovers Black is now ruling the world. White sets out to expand his kingdom level by level to reach him.
- **The goal is not to defeat Black — it is to bring him back.** The final chess match is a confrontation, not a war.
- Each enemy faction (goblins, orcs, elves, etc.) serves under Black's influence, knowingly or not.
- The older brother's name, the separation event, and the resolution options are to be determined in Issue #48 (Story Bible).

---

## Monetization Model

- **Chapters 1–3:** Free to play. No IAP gate.
- **Chapters 4–8:** Locked behind IAP. Individual unlock: $0.99/chapter. Bundle (Ch4–8): $4.99.
- **Cosmetic IAP:** Piece skin packs, board themes. No gameplay advantage ever.
- **No forced ads.** Rewarded video for hints only — optional, never mandatory.
- **No pay-to-win.** AI difficulty scales with story chapter, never with purchases.

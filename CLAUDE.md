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
│   │   │   │   ├── Board/          ← AttackChecker.cs, BoardConstants.cs, BoardManager.cs, BoardVisualizer.cs,
│   │   │   │   │                      Move.cs, MoveValidator.cs, Square.cs, SquareColor.cs
│   │   │   │   ├── Pieces/         ← Piece.cs (base), PieceType.cs, PieceColor.cs, PieceView.cs,
│   │   │   │   │                      King.cs, Queen.cs, Rook.cs, Bishop.cs, Knight.cs, Pawn.cs
│   │   │   │   └── GameManager.cs  ← Central authority: turn system, game state, win/loss, difficulty wiring
│   │   │   ├── AI/
│   │   │   │   ├── ChessAI.cs            ← IChessAI interface + full negamax engine
│   │   │   │   ├── Evaluator.cs          ← Material + PST + pawn structure + king safety + mobility + bishop pair
│   │   │   │   ├── PieceValues.cs        ← Centipawn constants: P=100 N=320 B=330 R=500 Q=900
│   │   │   │   ├── PieceSquareTables.cs  ← 8×8 positional bonus arrays per piece type
│   │   │   │   ├── ZobristTable.cs       ← Deterministic 64-bit hash per piece/color/square (seed 20260508)
│   │   │   │   ├── TranspositionTable.cs ← 32 MB flat-array TT; Exact/LowerBound/UpperBound TTFlag enum
│   │   │   │   ├── DifficultySettings.cs  ← Difficulty enum (Easy/Medium/Hard) + PlayerPrefs persistence (key: "Difficulty")
│   │   │   │   ├── OpeningBook.cs         ← ~25-entry coordinate-notation book; keyed by comma-sep move history
│   │   │   │   └── AIPerformanceLogger.cs ← ProfilerMarkers (ChessAI.Search/.Negamax/.Quiescence/.Evaluate) + threshold-aware Debug.Log
│   │   │   ├── Input/
│   │   │   │   ├── TileSelector.cs ← Raycasting, tile/piece hit detection
│   │   │   │   └── MoveSelector.cs ← Valid move highlighting, move execution; fires OnMoveExecutedDetailed(from,to)
│   │   │   ├── Editor/             ← Editor-only scripts (stripped from builds automatically by Unity)
│   │   │   │   ├── SceneSetup.cs        ← One-time scene bootstrap tool (camera, board, lighting)
│   │   │   │   ├── DifficultyDebugMenu.cs ← Chess ▶ Difficulty menu: set Easy/Medium/Hard during playtests (checkmarks show active level)
│   │   │   │   └── AIGameValidator.cs   ← Chess ▶ Validate AI: runs 9 self-play games (3×difficulty), asserts no illegal moves
│   │   │   ├── Story/              ← Phase 4 (not yet created)
│   │   │   │   └── NarrativeController.cs ← Story state, Yarn triggers, faction swaps
│   │   │   ├── UI/                 ← Phase 5 (not yet created)
│   │   │   │   └── HUDManager.cs   ← In-game HUD, menus, dialogue overlay
│   │   │   └── Save/               ← Phase 5 (not yet created)
│   │   │       └── SaveManager.cs  ← Serialise/deserialise game progress to JSON; will absorb PlayerPrefs "Difficulty"
│   │   ├── Prefabs/
│   │   │   ├── Pieces/             ← One prefab per piece type per faction
│   │   │   └── UI/
│   │   ├── Scenes/
│   │   │   ├── SampleScene.unity   ← Current Phase 1–2 prototype scene (renamed to Game.unity in Phase 3)
│   │   │   ├── MainMenu.unity      ← Phase 5
│   │   │   ├── ChapterSelect.unity ← Phase 5
│   │   │   └── Game.unity          ← Phase 3 rename target; single game scene, faction/chapter at runtime
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
- **Sliding pieces (Rook, Bishop, Queen)** use the protected `Piece.AddSlidingMoves(moves, board, fileDirs, rankDirs)` helper — pass parallel int arrays of file/rank deltas for each ray direction. Do not re-implement the ray-scanning loop in individual subclasses.
- **`Chess.Input` namespace**: Any code inside this namespace must qualify Unity's input class as `UnityEngine.Input.` (e.g. `UnityEngine.Input.GetMouseButtonDown(0)`) to avoid ambiguity with the namespace name itself.
- **`Square` struct mutation**: To modify a field on a Square, always copy→modify→write back: `var sq = _board[f,r]; sq.Piece = p; _board[f,r] = sq;`. Direct field assignment on an array element of a value type does not compile.
- **`Square` struct constructor**: Unity's C# version (no `preview` language features) requires every auto-property to be explicitly assigned in the struct constructor — even if the value is the type default (e.g. `IsEnPassantTarget = false`). CS0843 is thrown otherwise.
- The board **data** (C# classes) is completely separate from the **visual** (GameObjects/prefabs). `BoardManager` owns data; a separate `BoardVisualizer` syncs GameObjects to data.
- **`Square.IsEnPassantTarget`**: transient bool flag set by `BoardManager.ExecuteMove` on the square a pawn passed through on a double-step; cleared at the start of every subsequent move. `Pawn.GetValidMoves` reads it directly from the board array — no signature change to `GetValidMoves` needed.
- **Special move pattern (castling, en passant, promotion)**: every special move must be handled in **three places**: `BoardManager.ExecuteMove` (data), `MoveSelector.ExecuteMove` (visual), and `MoveValidator.Simulate` (simulation). All three must stay in sync. Castling = King moves ±2 files. En passant = pawn moves to `IsEnPassantTarget` square; captured pawn is at `(to.x, from.y)`. Promotion = pawn lands on back rank; `BoardManager.PromotePawn` + `BoardVisualizer.ReplacePieceView`. The auto-Queen call in `MoveSelector` is the **Phase 5 UI extension point**.
- **Piece cube naming — must stay in sync with board position**: `TileSelector.TryParseSquare` derives the clicked square's board coordinate by parsing the hit GameObject's name. Piece cubes use the format `"{Color}_{Type}_{File}{Rank}"` (e.g. `"White_Pawn_A4"`). `BoardVisualizer.MovePieceView` **must** update `view.gameObject.name` every time a piece moves, or clicking a moved piece returns its original (now-empty) square — making it unselectable and making captures on it impossible. Never rename piece GameObjects to any other format.

### Input System
- **Active Input Handling must be set to "Both"** (Project Settings → Player → Other Settings → Active Input Handling). The project ships the new Input System package (required by some Unity 6 internals) but `TileSelector` uses the legacy `UnityEngine.Input` API. Setting to "Both" lets both coexist. Setting to "Input System Package (New)" only breaks `TileSelector`; setting to "Input Manager (Old)" only removes the deprecation warning but is the correct behaviour for this codebase. The deprecation warning that appears in Play mode ("This project uses Input Manager, which is marked for deprecation") is harmless and expected.
- Do **not** migrate `TileSelector` to the new Input System — it would require `PlayerInput` component plumbing and is out of scope for Phase 1.

### Game Flow
- `GameManager` is the single source of truth for: whose turn it is, whether game is over, current board state.
- Turn order: `White → Black → White`. White always goes first.
- After every move, `GameManager` checks: Is the opponent in check? Is it checkmate? Is it stalemate?
- Illegal moves are **impossible to execute** — they are filtered out before being offered to the player.

### AI
- AI lives behind `IChessAI` interface. `GameManager` calls `GetBestMove()` — never the concrete class directly.
- **Difficulty model: Easy / Medium / Hard** (player-chosen global setting, applies to ALL chapters). NOT per-chapter scaling.
  - Easy = depth 2, no quiescence search — makes mistakes, accessible to beginners.
  - Medium = depth 4, quiescence on — solid tactical play.
  - Hard = depth 6, quiescence on — strong play, challenges intermediate players.
  - Stored in `PlayerPrefs` key `"Difficulty"` (Phase 2); `GameManager.Start()` reads it via `DifficultySettings` and sets `_aiDepth`, `_useQuiescence`, `_useOpeningBook`. Phase 5 `SaveManager` will migrate this into `save.json`.
- **AI technique stack** (implemented across issues #28–#31):
  - **Evaluation** (#28 ✅): material (P=100 N=320 B=330 R=500 Q=900) + piece-square tables + endgame detection (non-pawn material < 1300cp)
  - **Evaluation** (#30 ✅): pawn structure (doubled −20/extra, isolated −15, passed +25+8×rank) + king safety (pawn shield +8/pawn, middlegame only) + mobility (3cp × pseudo-legal move advantage) + bishop pair (+30)
  - **Search** (#29 ✅): negamax + iterative deepening (depth 1→maxDepth) + quiescence search (captures until quiet)
  - **Pruning** (#30 ✅): alpha-beta + null move pruning (R=2 shallow / R=3 depth≥6; skipped in check or endgame) + late move reductions (reduction=1 after move 3, reduction=2 after move 8; re-searches at full depth if score > alpha)
  - **Ordering** (#29 ✅ + #30 ✅): TT best move first → MVV-LVA captures → killer moves (2 per ply) → quiet moves
  - **Caching** (#30 ✅): Zobrist hashing (ZobristTable.cs, seed 20260508) + 32 MB transposition table (TranspositionTable.cs, always-replace, TTFlag: Exact / LowerBound / UpperBound)
  - **Opening** (#31 ✅): hardcoded coordinate-notation book (~25 entries, Medium/Hard only). Keyed by comma-joined move history ("e2e4,e7e5,g1f3"). Book move validated for legality before playing. `OpeningBook.MoveToKey(Move)` converts a Move to "e2e4" format — call it from GameManager after every move.
  - **Difficulty persistence** (#31 ✅): `PlayerPrefs` key "Difficulty" (int: 0=Easy, 1=Medium, 2=Hard). Read in `GameManager.Start()`. Phase 5 `SaveManager` will migrate to `save.json`.
  - **Move history tracking** (#31 ✅): `GameManager` maintains `_moveHistoryKey` (comma-sep string). `MoveSelector.OnMoveExecutedDetailed` event fires after every move with from/to; `GameManager.AppendMoveHistory` appends it. History key is captured before the AI move (book lookup), then the AI's move is appended by the event handler after `SubmitMove` returns.
- AI computation runs on a **background thread** (C# `Task`). Main thread waits; "Thinking…" shown by HUDManager (Phase 5).
- AI must **never** return an illegal move. If no moves available, return null — GameManager handles checkmate/stalemate.
- **`IChessAI.GetBestMove` signature**: `Move? GetBestMove(Square[,] board, PieceColor color, int depth, bool useQuiescence, string moveSequenceKey)`. The `moveSequenceKey` is the comma-joined move history before the AI's current turn; pass `string.Empty` when opening book is disabled (Easy).
- **`MoveSelector.OnMoveExecutedDetailed`**: `event Action<Vector2Int, Vector2Int>` — fired alongside `OnMoveExecuted` with from/to coordinates. GameManager subscribes to build the move history string for opening book lookups.
- Do **not** use Stockfish or any external process/binary — iOS bans child processes; Android requires impractical JNI native plugin.
- **`ChessAI._useQuiescence` is a single-writer instance field** — safe because `GameManager` never overlaps AI calls (it disables `MoveSelector` before `Task.Run` and only re-enables it after `await` returns). If concurrent calls are ever introduced, this field must become a parameter.
- **Opening book uses 4-char coordinate notation only** — e.g. `"e2e4"`, NOT algebraic `"e4"` or `"Nf3"`. `OpeningBook.MoveToKey(Move)` is the single place that generates keys; call it for every move in GameManager.
- **`Array.Clear` on `Move?[MaxPly, 2]`** — works by treating the 2D array as a flat sequence (`_killers.Length = MaxPly * 2`). Sets all slots to `null` (Nullable default). No special handling needed.
- **`ChessAI.CloneBoard` must deep-clone Piece objects** — `Square` is a value type but `Piece` is a class. A shallow copy (struct copy only) means all board clones in the search tree share the same `Piece` references. `ApplyMove` mutates `piece.Position` and `piece.HasMoved` on those shared objects, corrupting the live BoardManager board. Fix: `ClonePiece(Piece p)` creates a fresh piece of the correct subtype and copies `HasMoved`. Every board clone in the AI search has its own Piece objects isolated from the main thread.
- **`MoveSelector.OnMoveExecutedDetailed` fires before `OnMoveExecuted`** — in `ExecuteMove`, the order is: detailed event first, then the plain event. GameManager's `AppendMoveHistory` therefore runs before `SwitchTurn`. This is intentional: the history is complete (including the triggering move) by the time `SwitchTurn` reads it.

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
- **Orthographic** camera. Rotation `(50°, 45°, 0°)`, orthographic size `7`. Position `(−10.7, 18, −10.7)` to look at world origin where the board sits. **50° pitch** (not 30°) is required — 30° is too shallow, making the far half of the board visually compressed and unclickable in practice.
- **Camera math rule for Euler(p, 45°, 0°)** — Unity applies Q = Qy·Qx, so forward = Qy(45°)·Qx(p)·(0,0,1) = (cos_p·0.707, −sin_p, cos_p·0.707). Position: d = 18/sin_p, cx = cz = −d·cos_p·0.707. For 45° yaw cx always equals cz. Example values: p=30° → pos(−22,18,−22); p=45° → pos(−12.7,18,−12.7); p=50° → pos(−10.7,18,−10.7); p=60° → pos(−7.4,18,−7.4).
- Orthographic size is tuned for a landscape 8×8 board. Adjust for portrait/device aspect ratio in Phase 7.
- Do not use perspective cameras for the board scene.

### Performance Targets
| Metric | Target |
|---|---|
| Draw calls (board scene) | ≤ 20 |
| Triangles (board scene) | ≤ 30k |
| Texture memory | ≤ 150 MB |
| FPS (mid-range Android) | 60 during gameplay, 30 during dialogue |
| AI move time — depth 2 (Easy) | < 500 ms |
| AI move time — depth 4 (Medium) | < 2 000 ms |
| AI move time — depth 6 (Hard) | < 3 000 ms |
| App size | < 150 MB |

### AI Profiling Procedure (Issue #33)
**In-Editor (quick check):**
1. Press Play → make a White move
2. Watch the Unity Console — `[AI Perf]` line appears after every Black move
3. Line shows: depth, elapsed ms, node count, ✓ (pass) or ⚠ OVER TARGET (fail)
4. Open **Window → Analysis → Profiler** → filter by category `AI` to see `ChessAI.Search` marker breakdown

**On Android device:**
1. Build a **Development Build** with **Autoconnect Profiler** enabled (File → Build Settings)
2. Install on Snapdragon 700 target via `adb install`
3. In Unity Profiler, click **Android Player** in the device dropdown — it connects over USB
4. Play a game, make moves — Profiler records live frame data
5. Find `ChessAI.Search` samples; check duration column against targets above
6. Check Memory section for GC Alloc spikes during AI turns (> 5 MB per move = warning logged)

**If targets are not met:**
- Depth 6 > 3 s: reduce Hard depth from 6 → 5 in `DifficultySettings.cs`
- Excess GC: the main source is `List<Move>` allocation in `GenerateOrderedMoves` / `GenerateCapturesOrdered` — pre-allocated pools would fix it (Phase 7 optimisation)
- High node count with slow time: check null move pruning is activating (requires depth ≥ 3 and not in endgame)

### AI Playtest Procedure (Issue #34)
**Automated legality check (editor, no human required):**
1. Open Unity → menu bar **Chess ▶ Validate AI ▶ Run Self-Play Test (9 games)**
2. Watch the Console — one line per game, final summary `✓ PASS` or `✗ FAIL — N illegal-move game(s)`
3. Any `[AIValidator] ILLEGAL` line is a regression — fix the move-generation or AI bug it points to before proceeding

**Manual difficulty playtest (human required, 2 games per level):**
1. Use **Chess ▶ Difficulty** menu to set the level (checkmark shows current level)
2. Press Play; level takes effect immediately via `DifficultySettings`
3. Play 2 full games against each level and assess:
   - **Easy (depth 2)**: makes occasional blunders, beatable by a casual player
   - **Medium (depth 4)**: plays solid tactics, punishes obvious mistakes
   - **Hard (depth 6)**: strong positional + tactical play, challenging for intermediate players
4. Watch the Console after each Black move for `[AI Perf]` timing confirmation

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

**Current phase: Phase 2 — AI Opponent**

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

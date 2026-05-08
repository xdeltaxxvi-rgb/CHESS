#!/bin/bash
# CHESS Game — GitHub Issues Creator
REPO="xdeltaxxvi-rgb/CHESS"
COUNT=0

ci() {
  local title="$1" labels="$2" milestone="$3" body="$4"
  local result
  result=$(gh issue create --repo "$REPO" --title "$title" --body "$body" --label "$labels" --milestone "$milestone" 2>&1)
  if [ $? -eq 0 ]; then
    COUNT=$((COUNT+1))
    echo "  [#$COUNT] $title"
  else
    echo "  FAIL: $title"
    echo "        $result"
  fi
}

# ── PHASE 1: Chess Engine Core ─────────────────────────────────────────
echo "=== PHASE 1 ==="
ci "[P1-T01] Set up Unity 3D (URP) project" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0
## Description
Initialize Unity 2022 LTS with URP template inside CHESS repo. Configure Git LFS.
## Acceptance Criteria
- [ ] Unity 2022 LTS opens without errors
- [ ] URP pipeline configured
- [ ] .gitignore excludes Library/, Temp/, Obj/"

ci "[P1-T02] Configure Unity project settings (portrait, Android/iOS)" \
   "phase:1-chess-engine,P0-critical,type:devops" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 2h | **Priority:** P0 | **Depends on:** #1
## Description
Build targets: Android API 26+, iOS 13+. Orientation: Portrait. URP mobile config.
## Acceptance Criteria
- [ ] Build settings include Android + iOS
- [ ] Default orientation: Portrait
- [ ] URP configured for mobile"

ci "[P1-T03] Set up project folder structure" \
   "phase:1-chess-engine,P1-high,type:devops" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 1h | **Priority:** P1 | **Depends on:** #1
## Description
Create Assets/ hierarchy: Scripts/Core, Scripts/AI, Scripts/Input, Scripts/Story, Scripts/UI, Scripts/Save, Prefabs, Scenes, Art/Models, Art/Materials, Audio, Dialogue.
## Acceptance Criteria
- [ ] All folders created with .gitkeep
- [ ] Structure matches architecture spec"

ci "[P1-T04] Create BoardManager.cs — 8x8 data structure" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 6h | **Priority:** P0
## Description
2D array (8x8) of Square objects. File A-H (0-7), Rank 1-8 (0-7).
Methods: GetSquare(file, rank), IsValidCoordinate(), IsOccupied(), GetPieceAt().
## Acceptance Criteria
- [ ] Board initialized as 8x8 array
- [ ] GetSquare returns correct Square
- [ ] IsValidCoordinate false for out-of-bounds"

ci "[P1-T05] Create Square.cs data model" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 2h | **Priority:** P0 | **Depends on:** #4
## Description
Square struct: int File (0-7), int Rank (0-7), SquareColor (Light/Dark), Piece occupying.
IsOccupied(), IsOccupiedByColor(). Color = (file+rank)%2.
## Acceptance Criteria
- [ ] Square correctly reports occupancy
- [ ] Color determined by (file+rank) mod 2"

ci "[P1-T06] Create Piece.cs base class + 6 subclasses" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 5h | **Priority:** P0
## Description
Abstract Piece: PieceType enum, PieceColor enum, Vector2Int position, bool HasMoved, abstract GetValidMoves(Board).
Subclasses: King, Queen, Rook, Bishop, Knight, Pawn.
## Acceptance Criteria
- [ ] All 6 types instantiate without error
- [ ] Each holds type, color, position
- [ ] HasMoved initializes false"

ci "[P1-T07] Instantiate board visually — 8x8 placeholder grid" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #4
## Description
Generate 64 quad GameObjects. Alternating light/dark materials. Tag each with file+rank.
## Acceptance Criteria
- [ ] 64 tiles visible in scene
- [ ] Correct checkerboard colouring
- [ ] Each tile tagged with file+rank"

ci "[P1-T08] Instantiate 32 pieces at starting positions" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #6, #7
## Description
Spawn 32 placeholder cubes at FEN starting squares. White ranks 1-2, Black ranks 7-8.
## Acceptance Criteria
- [ ] All 32 pieces at correct starting positions
- [ ] Each GameObject references its Piece data
- [ ] Correct colour tinting"

ci "[P1-T09] Implement raycasting for touch/click input" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 6h | **Priority:** P0
## Description
Camera raycast on touch/click. Detect hit on tile or piece. Return Square coordinate. Works with mouse and touch.
## Acceptance Criteria
- [ ] Click/tap on any tile returns correct file+rank
- [ ] Works in Unity editor and on Android"

ci "[P1-T10] Implement piece selection logic" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #9
## Description
Tap own piece to select (highlight). Tap elsewhere to deselect. Only active player's pieces selectable.
## Acceptance Criteria
- [ ] Only current player's pieces selectable
- [ ] Selected piece visually indicated
- [ ] Tapping elsewhere deselects"

ci "[P1-T11] Implement Pawn movement rules" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #6
Forward 1. Forward 2 from start. Diagonal capture only when enemy present. Direction per colour.
## Acceptance Criteria
- [ ] Moves forward 1 on clear square
- [ ] Moves forward 2 only from starting rank
- [ ] Cannot move forward if blocked
- [ ] Captures diagonally when enemy present"

ci "[P1-T12] Implement Rook movement rules" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #6
Horizontal/vertical rays to edge. Stop at first occupied (capture enemy, blocked by friendly).
## Acceptance Criteria
- [ ] Slides correctly in all 4 directions
- [ ] Blocked by friendly pieces
- [ ] Captures enemy but cannot pass"

ci "[P1-T13] Implement Knight movement rules" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 3h | **Priority:** P0 | **Depends on:** #6
All 8 L-shape offsets. Jumps over pieces. Cannot land on friendly.
## Acceptance Criteria
- [ ] All 8 L-moves generated correctly
- [ ] Ignores pieces between
- [ ] Cannot land on friendly"

ci "[P1-T14] Implement Bishop movement rules" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #6
Diagonal rays 4 directions. Same blocking as Rook.
## Acceptance Criteria
- [ ] Slides along all 4 diagonals
- [ ] Blocked by friendly and enemy correctly"

ci "[P1-T15] Implement Queen movement rules" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 2h | **Priority:** P0 | **Depends on:** #12, #14
Rook + Bishop combined. All 8 directions.
## Acceptance Criteria
- [ ] Moves in all 8 directions
- [ ] Blocking logic identical to Rook/Bishop"

ci "[P1-T16] Implement King movement rules" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #6
One square any direction. Cannot move to attacked square.
## Acceptance Criteria
- [ ] Moves one square in all directions
- [ ] Illegal to move into check
- [ ] Cannot capture own piece"

ci "[P1-T17] Highlight valid move tiles on selection" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #10, #11, #12, #13, #14, #15, #16
Overlay valid tiles: green for empty, red for capture. Clear on deselect.
## Acceptance Criteria
- [ ] Valid moves highlighted on selection
- [ ] Capture squares visually distinct
- [ ] Highlights cleared on deselect"

ci "[P1-T18] Implement move execution — move + capture + deselect" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #17
Tap highlighted tile: move piece, update Board, remove captured, deselect, pass turn.
## Acceptance Criteria
- [ ] Piece moves to target (data + visual)
- [ ] Captured piece removed from scene and data
- [ ] Board state correct after every move"

ci "[P1-T19] Implement turn system — White/Black alternation" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #18
White first. Switch after each move. Lock input for inactive player.
## Acceptance Criteria
- [ ] Only White moves on White's turn
- [ ] Turn switches after every legal move
- [ ] Turn indicator updated"

ci "[P1-T20] Implement check detection" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #19
After every move: scan enemy attacks. If King attacked: InCheck=true. Flash King red. Filter moves to only those resolving check.
## Acceptance Criteria
- [ ] Check detected correctly
- [ ] King tile flagged visually
- [ ] Player can only make moves that exit check"

ci "[P1-T21] Implement checkmate detection" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #20
Zero legal moves AND in check = checkmate. Trigger GameOver(winner).
## Acceptance Criteria
- [ ] Detected in standard positions (Scholar's Mate etc.)
- [ ] Game ends, winner declared"

ci "[P1-T22] Implement stalemate detection" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 3h | **Priority:** P0 | **Depends on:** #21
Zero legal moves AND not in check = stalemate. Trigger GameOver(draw).
## Acceptance Criteria
- [ ] Stalemate detected correctly
- [ ] Game ends as draw"

ci "[P1-T23] Implement special move — Castling" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #20
Kingside and queenside. Conditions: neither King/Rook moved, no pieces between, no check during.
## Acceptance Criteria
- [ ] Kingside castling works
- [ ] Queenside castling works
- [ ] All 5 conditions enforced
- [ ] Unavailable after King or Rook moves"

ci "[P1-T24] Implement special move — En passant" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #11
After enemy pawn double-step: passing square available for exactly one turn.
## Acceptance Criteria
- [ ] Available immediately after double-step
- [ ] Not available after next move
- [ ] Captured pawn removed correctly"

ci "[P1-T25] Implement special move — Pawn promotion" \
   "phase:1-chess-engine,P0-critical,type:feature" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #11
Pawn reaches back rank: show piece selection UI. Replace with chosen piece. Default Queen after 10s.
## Acceptance Criteria
- [ ] Promotion triggered on back rank
- [ ] All 4 choices available (Q/R/B/N)
- [ ] Board updated immediately"

ci "[P1-T26] Phase 1 playtest — 2 complete games" \
   "phase:1-chess-engine,P0-critical,type:qa" \
   "Phase 1: Chess Engine Core" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #21, #22, #23, #24, #25
Two humans play 2 complete games start-to-checkmate. All special moves tested. All bugs logged.
## Acceptance Criteria
- [ ] 2 games completed without crashes
- [ ] All special moves tested
- [ ] No illegal moves accepted
- [ ] All bugs logged as issues"

# ── PHASE 2: AI Opponent ─────────────────────────────────────────────
echo "=== PHASE 2 ==="
ci "[P2-T01] Create ChessAI.cs — pluggable AI interface" \
   "phase:2-ai,P0-critical,type:feature" \
   "Phase 2: AI Opponent" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #26
IChessAI interface: GetBestMove(BoardState, PieceColor, depth). Pluggable design.
## Acceptance Criteria
- [ ] Interface defined
- [ ] ChessAI implements interface
- [ ] GameManager calls AI via interface"

ci "[P2-T02] Implement board evaluation function" \
   "phase:2-ai,P0-critical,type:feature" \
   "Phase 2: AI Opponent" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #27
Material: Pawn=100, Knight=320, Bishop=330, Rook=500, Queen=900. Piece-square bonus tables.
## Acceptance Criteria
- [ ] Positive score for winning positions
- [ ] Captures scored correctly
- [ ] Positional tables applied"

ci "[P2-T03] Implement minimax algorithm" \
   "phase:2-ai,P0-critical,type:feature" \
   "Phase 2: AI Opponent" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #28
Recursive minimax, configurable depth. Maximize AI, minimize opponent.
## Acceptance Criteria
- [ ] AI selects only legal move in forced-mate positions
- [ ] Depth=1 makes basic captures
- [ ] Depth=3 avoids simple blunders"

ci "[P2-T04] Implement alpha-beta pruning" \
   "phase:2-ai,P0-critical,type:feature" \
   "Phase 2: AI Opponent" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #29
Add alpha-beta cutoffs. Reduce search 50%+ vs unoptimized. Move ordering: captures first.
## Acceptance Criteria
- [ ] Same moves selected as without pruning
- [ ] Depth=4 under 3 seconds on Snapdragon 700
- [ ] Move ordering applied"

ci "[P2-T05] Implement AI difficulty scaling per chapter" \
   "phase:2-ai,P0-critical,type:feature" \
   "Phase 2: AI Opponent" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #30
Ch1=depth 2, Ch2-3=3, Ch4-5=4, Ch6-7=5, Ch8=6 (Boss).
## Acceptance Criteria
- [ ] Clearly easier Ch1 vs Ch8
- [ ] Difficulty changes on chapter load"

ci "[P2-T06] Run AI on background thread (async)" \
   "phase:2-ai,P1-high,type:perf" \
   "Phase 2: AI Opponent" \
   "**Est:** 5h | **Priority:** P1 | **Depends on:** #31
Minimax on C# Task. Thinking spinner. Apply on main thread. Zero frame drops.
## Acceptance Criteria
- [ ] Zero frame drops during computation
- [ ] Thinking indicator visible
- [ ] Move applies correctly after async"

ci "[P2-T07] Profile AI performance on target Android device" \
   "phase:2-ai,P0-critical,type:perf" \
   "Phase 2: AI Opponent" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #33
Deploy to Android Snapdragon 700. Measure depth 2-6 move times.
## Acceptance Criteria
- [ ] Depth 2: under 0.5s
- [ ] Depth 4: under 2s
- [ ] Depth 6: under 3s
- [ ] No memory leaks after 10 AI turns"

ci "[P2-T08] Phase 2 AI playtest — all difficulty levels" \
   "phase:2-ai,P0-critical,type:qa" \
   "Phase 2: AI Opponent" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #34
2 games vs AI at each level 1-5. AI never illegal moves.
## Acceptance Criteria
- [ ] Never illegal move in 10 games
- [ ] Easy (Ch1) beatable by beginner
- [ ] Hard (Ch8) challenges intermediate player"

# ── PHASE 3: 3D Art & Camera ─────────────────────────────────────────
echo "=== PHASE 3 ==="
ci "[P3-T01] Set up isometric orthographic camera (portrait)" \
   "phase:3-art,P0-critical,type:feature" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 4h | **Priority:** P0
Orthographic. Position (0,18,0), rotation (30,45,0). Board fills upper 70% portrait.
## Acceptance Criteria
- [ ] Board fully visible in portrait
- [ ] No perspective distortion
- [ ] Correct on 1080x2340 and 720x1560"

ci "[P3-T02] Responsive board scaling across screen sizes" \
   "phase:3-art,P0-critical,type:feature" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #37
Orthographic size from aspect ratio. UI Canvas anchors below board.
## Acceptance Criteria
- [ ] Correct framing on 720p portrait
- [ ] Correct framing on 1440p portrait
- [ ] No board/UI overlap"

ci "[P3-T03] Model chessboard (low-poly, under 500 tris)" \
   "phase:3-art,P0-critical,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 8h | **Priority:** P0
8x8 integrated mesh, edge trim, base. Under 500 tris. Single material/draw call.
## Acceptance Criteria
- [ ] Readable at isometric angle
- [ ] Under 500 triangles
- [ ] Single draw call for board"

ci "[P3-T04] Model 6 White piece types (low-poly fantasy)" \
   "phase:3-art,P0-critical,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 24h | **Priority:** P0
K/Q/R/B/N/P. White kingdom theme. ~600 tris each. Silhouette instantly recognizable.
## Acceptance Criteria
- [ ] All 6 pieces imported
- [ ] Each under 800 tris
- [ ] Silhouettes distinct per type
- [ ] Consistent scale"

ci "[P3-T05] Model 6 Black piece types (shadow kingdom theme)" \
   "phase:3-art,P0-critical,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 12h | **Priority:** P0 | **Depends on:** #41
Dark stone/obsidian. Reuse topology where possible. Same poly budget.
## Acceptance Criteria
- [ ] 6 Black pieces modelled
- [ ] Distinct from White but same silhouettes
- [ ] Under 800 tris each"

ci "[P3-T06] Model Goblin faction piece set (Chapter 2)" \
   "phase:3-art,P1-high,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 16h | **Priority:** P1
Goblin enemy pieces Ch2. Green palette, crude weapons.
## Acceptance Criteria
- [ ] 6 Goblin pieces modelled
- [ ] Roles identifiable by silhouette
- [ ] Loads at runtime on Ch2"

ci "[P3-T07] Model Orc faction piece set (Chapter 3)" \
   "phase:3-art,P1-high,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 16h | **Priority:** P1
Orc pieces Ch3. Heavy armour, axes, brown/grey palette.
## Acceptance Criteria
- [ ] 6 Orc pieces modelled
- [ ] Distinct from Goblin set
- [ ] Correct faction on Ch3"

ci "[P3-T08] Create texture atlas (1024x1024 per faction)" \
   "phase:3-art,P0-critical,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #41, #42
Pack 6 pieces into single 1024x1024 atlas per faction. 1 material per team. Under 4 draw calls for all 32.
## Acceptance Criteria
- [ ] White team: 1 atlas, 1 material
- [ ] Enemy team: 1 atlas, 1 material
- [ ] No UV seams
- [ ] Under 4 draw calls for 32 pieces"

ci "[P3-T09] Implement piece move animation (lerp 0.4s)" \
   "phase:3-art,P1-high,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 6h | **Priority:** P1 | **Depends on:** #41
Lerp 0.4s ease-in-out. Knight arc. Block input during animation.
## Acceptance Criteria
- [ ] Smooth animation to new position
- [ ] Input blocked during animation
- [ ] Knight has arc"

ci "[P3-T10] Implement piece capture animation" \
   "phase:3-art,P1-high,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 4h | **Priority:** P1 | **Depends on:** #47
Captured piece: scale-down + fade 0.3s. Remove GameObject after.
## Acceptance Criteria
- [ ] Piece removed with animation
- [ ] No lingering GameObjects
- [ ] Feels satisfying"

ci "[P3-T11] Implement chapter environment and skybox per theme" \
   "phase:3-art,P1-high,type:art" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 12h | **Priority:** P1
Ch1: Kingdom. Ch2: Goblin caves. Ch3: Orc wastes. Swap on chapter load.
## Acceptance Criteria
- [ ] Distinct atmosphere per chapter
- [ ] Loads without frame spike
- [ ] Consistent lighting with pieces"

ci "[P3-T12] Visual QA on multiple screen sizes" \
   "phase:3-art,P0-critical,type:qa" \
   "Phase 3: 3D Art and Camera" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #38
Test: 720x1560, 1080x2340, 1440x3200, tablet 2048x1536.
## Acceptance Criteria
- [ ] Board visible on all 4 resolutions
- [ ] No UI/board overlap
- [ ] Pieces readable at smallest resolution"

# ── PHASE 4: Story & Narrative ────────────────────────────────────────
echo "=== PHASE 4 ==="
ci "[P4-T01] Install and configure Yarn Spinner" \
   "phase:4-story,P0-critical,type:feature" \
   "Phase 4: Story and Narrative" \
   "**Est:** 3h | **Priority:** P0
Add via Package Manager. YarnProject asset. DialogueRunner wired up.
## Acceptance Criteria
- [ ] Installed without console errors
- [ ] Test dialogue displays in Play mode
- [ ] DialogueRunner pauses/resumes game"

ci "[P4-T02] Write Story Bible — all 8 chapters" \
   "phase:4-story,P0-critical,type:story" \
   "Phase 4: Story and Narrative" \
   "**Est:** 24h | **Priority:** P0
Brothers backstory, separation, older brother's change, White's kingdom, Ch1-8 beats, faction roles, final confrontation.
## Acceptance Criteria
- [ ] All 8 chapter beats documented
- [ ] Character motivations clear and consistent
- [ ] Enemy factions tied to narrative
- [ ] Approved before dialogue writing"

ci "[P4-T03] Write Chapter 1 dialogue (.yarn scripts)" \
   "phase:4-story,P0-critical,type:story" \
   "Phase 4: Story and Narrative" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #52
Opening intro, pre-match, mid-match lines, post-victory, post-defeat.
## Acceptance Criteria
- [ ] All 5 moments scripted
- [ ] Plays in-game without errors
- [ ] Earnest, determined tone"

ci "[P4-T04] Write Chapter 2-4 dialogue (.yarn scripts)" \
   "phase:4-story,P0-critical,type:story" \
   "Phase 4: Story and Narrative" \
   "**Est:** 14h | **Priority:** P0 | **Depends on:** #52
Ch2: Goblin king. Ch3: Orc warlord. Ch4: Elf + first revelation about older brother.
## Acceptance Criteria
- [ ] Pre/post dialogue per chapter
- [ ] Stakes escalate each chapter
- [ ] Ch4 delivers major revelation"

ci "[P4-T05] Write Chapter 5-8 dialogue (.yarn scripts)" \
   "phase:4-story,P1-high,type:story" \
   "Phase 4: Story and Narrative" \
   "**Est:** 16h | **Priority:** P1 | **Depends on:** #52
Ch5-7: escalation. Ch8: final confrontation. Emotionally resonant.
## Acceptance Criteria
- [ ] Ch8 pre-match sets emotional climax
- [ ] Ch8 post-victory delivers resolution
- [ ] Post-defeat does not break story state"

ci "[P4-T06] Implement NarrativeController.cs" \
   "phase:4-story,P0-critical,type:feature" \
   "Phase 4: Story and Narrative" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #51
Story state machine. Yarn triggers pre/post match. Story flags. Faction model swaps.
## Acceptance Criteria
- [ ] Pre-match dialogue fires each chapter
- [ ] Post-match fires on victory/defeat
- [ ] Story flags persist
- [ ] Faction swap works on load"

ci "[P4-T07] Implement chapter unlock system" \
   "phase:4-story,P0-critical,type:feature" \
   "Phase 4: Story and Narrative" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #57
Ch1-3 free. Ch4+ behind IAP. Check unlock before load. Receipt validates.
## Acceptance Criteria
- [ ] Ch1-3 load without IAP
- [ ] Ch4+ shows lock + prompt
- [ ] Unlocks permanently after IAP
- [ ] Restore re-unlocks on reinstall"

ci "[P4-T08] Implement story progress save/load" \
   "phase:4-story,P0-critical,type:feature" \
   "Phase 4: Story and Narrative" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #58
SaveManager: chapter, completed[], story flags, IAP unlocks. JSON to persistentDataPath.
## Acceptance Criteria
- [ ] Kill app, reopen — resumes correctly
- [ ] All story flags restored
- [ ] IAP unlocks survive reinstall"

ci "[P4-T09] Phase 4 story QA playthrough (Ch 1-4)" \
   "phase:4-story,P0-critical,type:qa" \
   "Phase 4: Story and Narrative" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #57, #59
All 4 free chapters end-to-end. Victory + defeat paths. Chapter unlock gate.
## Acceptance Criteria
- [ ] All triggers fire correctly
- [ ] No duplicate or skipped dialogue
- [ ] Both paths tested
- [ ] Save/load across chapter boundary"

# ── PHASE 5: UI/UX ──────────────────────────────────────────────────
echo "=== PHASE 5 ==="
ci "[P5-T01] Design and implement main menu screen" \
   "phase:5-ui,P0-critical,type:feature" \
   "Phase 5: UI UX" \
   "**Est:** 10h | **Priority:** P0
Portrait: CHESS logo, animated background, Play/Settings/Credits, fade to Chapter Select.
## Acceptance Criteria
- [ ] All buttons visible on 720p-1440p
- [ ] All buttons respond to touch
- [ ] Smooth transition to Chapter Select"

ci "[P5-T02] Design and implement chapter select screen" \
   "phase:5-ui,P0-critical,type:feature" \
   "Phase 5: UI UX" \
   "**Est:** 10h | **Priority:** P0 | **Depends on:** #58
Scrollable list. Lock/unlock states. IAP prompt on locked tap.
## Acceptance Criteria
- [ ] All 8 chapters listed
- [ ] Lock/unlock state correct
- [ ] IAP prompt fires on locked tap
- [ ] Completed chapters show indicator"

ci "[P5-T03] Design and implement in-game HUD" \
   "phase:5-ui,P0-critical,type:feature" \
   "Phase 5: UI UX" \
   "**Est:** 10h | **Priority:** P0
Bottom 30%: player/enemy portraits, turn indicator, captured pieces, move counter, check warning.
## Acceptance Criteria
- [ ] HUD visible without obscuring board
- [ ] Turn indicator updates in real time
- [ ] Check warning on check
- [ ] Captured pieces display correctly"

ci "[P5-T04] Implement dialogue UI overlay" \
   "phase:5-ui,P0-critical,type:feature" \
   "Phase 5: UI UX" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #57
Dialogue box bottom. Portrait, name, typewriter text. Tap-advance, Skip. Yarn integration.
## Acceptance Criteria
- [ ] Dialogue without blocking board
- [ ] Typewriter effect correct
- [ ] Skip dismisses entire scene
- [ ] Portrait swaps per speaker"

ci "[P5-T05] Implement game over screen" \
   "phase:5-ui,P0-critical,type:feature" \
   "Phase 5: UI UX" \
   "**Est:** 5h | **Priority:** P0
Victory: crown + story text + Continue. Defeat: sword + Try Again / Main Menu.
## Acceptance Criteria
- [ ] Correct screen for win/loss/draw
- [ ] Story context from NarrativeController
- [ ] Buttons work correctly"

ci "[P5-T06] Implement settings screen" \
   "phase:5-ui,P1-high,type:feature" \
   "Phase 5: UI UX" \
   "**Est:** 5h | **Priority:** P1
Music/SFX volume, Move Hints toggle, Animation Speed, Privacy Policy, version.
## Acceptance Criteria
- [ ] Settings persist via PlayerPrefs
- [ ] Audio sliders affect volume immediately
- [ ] Move hints toggle works"

ci "[P5-T07] Phase 5 UI/UX QA on physical device" \
   "phase:5-ui,P0-critical,type:qa" \
   "Phase 5: UI UX" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #61, #62, #63, #64, #65, #66
Physical Android. Touch targets >= 44dp, no overlaps, legible at 720p, back button correct.
## Acceptance Criteria
- [ ] All buttons respond correctly
- [ ] No overlapping UI elements
- [ ] Readable at 720p
- [ ] Back button navigates correctly"

# ── PHASE 6: Audio ─────────────────────────────────────────────────
echo "=== PHASE 6 ==="
ci "[P6-T01] Source background music for Chapters 1-3" \
   "phase:6-audio,P1-high,type:feature" \
   "Phase 6: Audio" \
   "**Est:** 12h | **Priority:** P1
3 looping tracks: Ch1 Kingdom (hopeful), Ch2 Goblin Caves (dark), Ch3 Orc Wastes (heavy). OGG format.
## Acceptance Criteria
- [ ] 3 tracks imported and looping
- [ ] Fits emotional tone of each chapter"

ci "[P6-T02] Implement piece movement and capture SFX" \
   "phase:6-audio,P1-high,type:feature" \
   "Phase 6: Audio" \
   "**Est:** 4h | **Priority:** P1
Wood click for movement. Heavier for capture. Via AudioManager.
## Acceptance Criteria
- [ ] Movement SFX on every move
- [ ] Capture SFX distinct
- [ ] No delay over 50ms"

ci "[P6-T03] Implement check, checkmate, and victory SFX" \
   "phase:6-audio,P1-high,type:feature" \
   "Phase 6: Audio" \
   "**Est:** 3h | **Priority:** P1
Check: sting. Win: fanfare. Loss: defeat sting. Stalemate: neutral tone.
## Acceptance Criteria
- [ ] All 4 events play at correct moments
- [ ] BG music ducks on stings"

ci "[P6-T04] Implement UI SFX and AudioManager singleton" \
   "phase:6-audio,P2-medium,type:feature" \
   "Phase 6: Audio" \
   "**Est:** 4h | **Priority:** P2
AudioManager: PlaySFX, PlayMusic, SetVolumes. UI sounds: tap, transition, unlock fanfare, dialogue advance.
## Acceptance Criteria
- [ ] AudioManager accessible from anywhere
- [ ] Music crossfades between chapters
- [ ] All UI has audio feedback"

# ── PHASE 7: Mobile Optimization ────────────────────────────────────
echo "=== PHASE 7 ==="
ci "[P7-T01] Profile game on target Android device" \
   "phase:7-optimization,P0-critical,type:perf" \
   "Phase 7: Mobile Optimization" \
   "**Est:** 6h | **Priority:** P0
Dev build on Snapdragon 700. Unity Profiler: CPU ms, GPU ms, draw calls, memory, GC.
## Acceptance Criteria
- [ ] Profiling data captured
- [ ] Top 3 bottlenecks identified
- [ ] Baseline documented"

ci "[P7-T02] Optimize draw calls — SRP Batcher and GPU Instancing" \
   "phase:7-optimization,P0-critical,type:perf" \
   "Phase 7: Mobile Optimization" \
   "**Est:** 5h | **Priority:** P0 | **Depends on:** #72
Enable SRP Batcher. GPU Instancing on pieces. Under 20 draw calls total.
## Acceptance Criteria
- [ ] Under 20 draw calls for board + pieces
- [ ] No visual regressions"

ci "[P7-T03] Compress all textures for mobile (ASTC/PVRTC)" \
   "phase:7-optimization,P0-critical,type:perf" \
   "Phase 7: Mobile Optimization" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #72
Android: ASTC 6x6. iOS: ASTC 6x6. Total VRAM under 150 MB.
## Acceptance Criteria
- [ ] All textures use mobile compression
- [ ] VRAM under 150 MB
- [ ] No visible quality loss at 1080p"

ci "[P7-T04] Implement async scene loading for chapter transitions" \
   "phase:7-optimization,P1-high,type:perf" \
   "Phase 7: Mobile Optimization" \
   "**Est:** 6h | **Priority:** P1 | **Depends on:** #72
LoadSceneAsync. Loading screen with progress bar. Under 3s on mid-range.
## Acceptance Criteria
- [ ] No frame freeze on transition
- [ ] Loading screen with progress
- [ ] Under 3 seconds"

ci "[P7-T05] Configure Android and iOS release build settings" \
   "phase:7-optimization,P0-critical,type:devops" \
   "Phase 7: Mobile Optimization" \
   "**Est:** 6h | **Priority:** P0
Android: ARM64+ARMv7, IL2CPP, Stripping=Medium, API 26-33. iOS: arm64, IL2CPP, min iOS 13.
## Acceptance Criteria
- [ ] Android AAB builds and installs
- [ ] iOS IPA archives
- [ ] App size under 150 MB"

ci "[P7-T06] Performance QA on 3 device tiers" \
   "phase:7-optimization,P0-critical,type:qa" \
   "Phase 7: Mobile Optimization" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #72, #73, #74, #75, #76
Low-end (2018, 2GB), Mid (2021, SD720G), High (2023, SD8 Gen2).
## Acceptance Criteria
- [ ] Low-end: 30+ FPS
- [ ] Mid-range: 60 FPS
- [ ] High-end: stable 60 FPS
- [ ] All: chapter loads under 5s"

# ── PHASE 8: QA & Bug Fix ────────────────────────────────────────────
echo "=== PHASE 8 ==="
ci "[P8-T01] Create master test plan document" \
   "phase:8-qa,P0-critical,type:qa" \
   "Phase 8: QA and Bug Fix" \
   "**Est:** 6h | **Priority:** P0
All test cases: chess rules, story (all 8 chapters), UI (all screens), AI, save/load. Stored in /docs/TEST_PLAN.md.
## Acceptance Criteria
- [ ] Covers all functional requirements
- [ ] Each case has input, expected output, pass/fail
- [ ] Stored in /docs/TEST_PLAN.md"

ci "[P8-T02] Chess rules regression — 10 full games" \
   "phase:8-qa,P0-critical,type:qa" \
   "Phase 8: QA and Bug Fix" \
   "**Est:** 10h | **Priority:** P0 | **Depends on:** #79
Covers Scholar's Mate, Back Rank Mate, stalemate, all castling, en passant, all 4 promotions.
## Acceptance Criteria
- [ ] 10 games without crashes
- [ ] All special moves tested
- [ ] No illegal moves accepted
- [ ] All end-game conditions correct"

ci "[P8-T03] Story progression regression — all 8 chapters" \
   "phase:8-qa,P0-critical,type:qa" \
   "Phase 8: QA and Bug Fix" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #79
All 8 chapters including IAP flow. Victory + defeat per chapter. Save/load at every boundary.
## Acceptance Criteria
- [ ] All 8 chapters without story bugs
- [ ] Both paths tested per chapter
- [ ] Save/load works at every boundary"

ci "[P8-T04] Bug fix sprint — all P0 and P1 issues" \
   "phase:8-qa,P0-critical,type:bug" \
   "Phase 8: QA and Bug Fix" \
   "**Est:** 24h | **Priority:** P0 | **Depends on:** #80, #81, #82
Resolve all P0 (game-breaking) and P1 (major feature broken) bugs. Re-test each fix.
## Acceptance Criteria
- [ ] All P0 resolved and re-tested
- [ ] All P1 resolved or deferred with docs
- [ ] No new P0 introduced"

ci "[P8-T05] Final sign-off regression pass" \
   "phase:8-qa,P0-critical,type:qa" \
   "Phase 8: QA and Bug Fix" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #83
Full playthrough after all fixes. 2 games + all chapters + all UI. Product Owner sign-off.
## Acceptance Criteria
- [ ] Zero P0 bugs open
- [ ] All P1 resolved or formally deferred
- [ ] Product Owner signed off"

# ── PHASE 9: Monetization & Analytics ───────────────────────────────
echo "=== PHASE 9 ==="
ci "[P9-T01] Integrate Unity IAP and configure product catalogue" \
   "phase:9-monetization,P0-critical,type:feature" \
   "Phase 9: Monetization Analytics" \
   "**Est:** 8h | **Priority:** P0
Products: chapter_4_unlock, chapter_5_unlock, chapters_4_8_bundle, piece_skin_pack_1.
## Acceptance Criteria
- [ ] All products in both stores
- [ ] Unity IAP initializes without error
- [ ] Product IDs match between Unity and stores"

ci "[P9-T02] Implement chapter unlock purchase and restore flow" \
   "phase:9-monetization,P0-critical,type:feature" \
   "Phase 9: Monetization Analytics" \
   "**Est:** 8h | **Priority:** P0 | **Depends on:** #85
Tap locked -> IAP prompt -> unlock permanently. Restore Purchases in Settings.
## Acceptance Criteria
- [ ] Purchase flow completes in sandbox
- [ ] Unlocks immediately after purchase
- [ ] Restore re-unlocks on fresh install
- [ ] Failed purchase does not unlock"

ci "[P9-T03] Integrate Firebase Analytics and instrument events" \
   "phase:9-monetization,P1-high,type:feature" \
   "Phase 9: Monetization Analytics" \
   "**Est:** 8h | **Priority:** P1
Events: session_start, chapter_start, chapter_complete, match_result, iap_initiated, iap_complete, session_end.
## Acceptance Criteria
- [ ] Firebase initializes on both platforms
- [ ] All 7 events visible in debug view
- [ ] No PII in any event"

ci "[P9-T04] IAP sandbox testing — Android and iOS" \
   "phase:9-monetization,P0-critical,type:qa" \
   "Phase 9: Monetization Analytics" \
   "**Est:** 6h | **Priority:** P0 | **Depends on:** #86
Test: buy chapter, cancel, buy bundle, restore, interrupted purchase.
## Acceptance Criteria
- [ ] All 4 products purchasable in sandbox
- [ ] Cancelled purchase does not unlock
- [ ] Restore works on both platforms
- [ ] Interrupted purchase resolves correctly"

# ── PHASE 10: Launch Preparation ────────────────────────────────────
echo "=== PHASE 10 ==="
ci "[P10-T01] Create Google Play store listing" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 6h | **Priority:** P0
Icon 512x512, feature graphic 1024x500, 8 portrait screenshots, descriptions, content rating, privacy policy.
## Acceptance Criteria
- [ ] All required assets uploaded
- [ ] Content rating complete
- [ ] Privacy policy URL live
- [ ] Listing looks compelling"

ci "[P10-T02] Create Apple App Store listing" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 6h | **Priority:** P0
Icon 1024x1024, portrait screenshots (6.5 and 5.5 inch), metadata, age rating, privacy labels.
## Acceptance Criteria
- [ ] All screenshot sizes uploaded
- [ ] Age rating complete
- [ ] Privacy labels correct
- [ ] Keywords optimized"

ci "[P10-T03] Generate signed Android AAB release build" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 4h | **Priority:** P0
Release keystore. Signed AAB. Sideload test. Upload to internal track. Under 150 MB.
## Acceptance Criteria
- [ ] Signed AAB installs cleanly
- [ ] No Development Build watermark
- [ ] Under 150 MB
- [ ] Keystore backed up securely (NOT in repo)"

ci "[P10-T04] Generate iOS IPA and upload to TestFlight" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 4h | **Priority:** P0
Archive Xcode Release. Upload via Transporter. Confirm install on iPhone.
## Acceptance Criteria
- [ ] IPA archives without errors
- [ ] TestFlight build available
- [ ] Installs on iPhone iOS 13+"

ci "[P10-T05] Closed beta — 15-20 external testers" \
   "phase:10-launch,P0-critical,type:qa" \
   "Phase 10: Launch Preparation" \
   "**Est:** 10h | **Priority:** P0 | **Depends on:** #91, #92
15-20 testers, structured feedback form, 1-week window.
## Acceptance Criteria
- [ ] 15+ testers complete feedback
- [ ] No P0 bugs reported
- [ ] Average rating 3.5+/5"

ci "[P10-T06] Submit to Google Play for review" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 3h | **Priority:** P0 | **Depends on:** #93
Final AAB to Production track. Free, worldwide. Submit (3-7 days).
## Acceptance Criteria
- [ ] Submitted to Production
- [ ] No policy violations
- [ ] Review approval received"

ci "[P10-T07] Submit to Apple App Store for review" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 3h | **Priority:** P0 | **Depends on:** #93
IPA via App Store Connect. Export compliance, content rights. Review 1-3 days.
## Acceptance Criteria
- [ ] Submitted for review
- [ ] No rejection for metadata/policy
- [ ] Approval received"

ci "[P10-T08] Launch day checklist and go-live" \
   "phase:10-launch,P0-critical,type:devops" \
   "Phase 10: Launch Preparation" \
   "**Est:** 4h | **Priority:** P0 | **Depends on:** #94, #95
Set live. Post social media. Monitor Crashlytics 24h. Respond to early reviews.
## Acceptance Criteria
- [ ] Both stores live and downloadable
- [ ] Social launch post published
- [ ] Crashlytics monitored 24h
- [ ] Day-1 crash rate under 0.1%"

echo ""
echo "================================"
echo "DONE — Total issues created: $COUNT"

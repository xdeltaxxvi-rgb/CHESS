#!/usr/bin/env pwsh
# CHESS Game — GitHub Issues Creator
# Run once to populate all project tasks as GitHub Issues
# Usage: .\scripts\create-issues.ps1

param(
  [string]$Repo = "xdeltaxxvi-rgb/CHESS"
)

# Helper: get milestone number by title
function Get-MilestoneNumber($title) {
  $ms = gh api "repos/$Repo/milestones" --paginate | ConvertFrom-Json
  $m = $ms | Where-Object { $_.title -eq $title }
  return $m.number
}

$ms = gh api "repos/$Repo/milestones" --paginate | ConvertFrom-Json

function MN($title) {
  return ($ms | Where-Object { $_.title -eq $title }).number
}

$issues = @(

  # ─── PHASE 1: CHESS ENGINE CORE ──────────────────────────────────────────
  @{
    title     = "[P1-T01] Set up Unity 3D (URP) project"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T01
**Priority:** P0 — Critical
**Estimated Hours:** 4

## Description
Initialize Unity 2022 LTS project with URP template inside the CHESS repo. Configure Git LFS for binary assets.

## Acceptance Criteria
- [ ] Unity 2022 LTS project opens without errors
- [ ] URP pipeline asset configured
- [ ] .gitignore excludes Library/, Temp/, Obj/
- [ ] Project folder committed to main branch
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T02] Configure Unity project settings (portrait, Android/iOS)"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T02
**Priority:** P0 — Critical
**Estimated Hours:** 2
**Depends on:** #1

## Description
Set build targets for Android (API 26+) and iOS (13+). Lock orientation to portrait. Configure URP asset for mobile (no SSAO, no shadows on low tier).

## Acceptance Criteria
- [ ] Build settings include Android + iOS targets
- [ ] Default orientation: Portrait
- [ ] URP configured for mobile quality
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:devops")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T03] Set up project folder structure"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T03
**Priority:** P1 — High
**Estimated Hours:** 1
**Depends on:** #1

## Description
Create the canonical Assets/ folder hierarchy: Scripts/Core, Scripts/AI, Scripts/Input, Scripts/Story, Scripts/UI, Scripts/Save, Prefabs, Scenes, Art/Models, Art/Materials, Audio, Dialogue.

## Acceptance Criteria
- [ ] All folders created with .gitkeep placeholders
- [ ] Folder structure matches architecture doc
"@
    labels    = @("phase:1-chess-engine","P1-high","type:devops")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T04] Create BoardManager.cs — 8x8 data structure"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T04
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
2D array [8,8] of Square objects. File A–H (0–7), Rank 1–8 (0–7) coordinate system. Methods: GetSquare(file, rank), IsValidCoordinate(), IsOccupied(), GetPieceAt().

## Acceptance Criteria
- [ ] Board initialised as 8x8 array
- [ ] GetSquare(file, rank) returns correct Square
- [ ] IsValidCoordinate() returns false for out-of-bounds
- [ ] Unit tested with NUnit
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T05] Create Square.cs data model"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T05
**Priority:** P0 — Critical
**Estimated Hours:** 2
**Depends on:** #4

## Description
Square struct: int File (0–7), int Rank (0–7), SquareColor (Light/Dark), Piece OccupyingPiece (nullable). Utility: IsOccupied(), IsOccupiedByColor(PieceColor).

## Acceptance Criteria
- [ ] Square correctly reports occupancy
- [ ] Color determined by (file + rank) % 2
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T06] Create Piece.cs base class + 6 subclasses"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T06
**Priority:** P0 — Critical
**Estimated Hours:** 5

## Description
Abstract Piece base: PieceType enum, PieceColor enum, Vector2Int position, bool HasMoved, abstract List<Vector2Int> GetValidMoves(Board).
Subclasses: King, Queen, Rook, Bishop, Knight, Pawn — each overrides GetValidMoves().

## Acceptance Criteria
- [ ] All 6 piece types instantiate without error
- [ ] Each piece holds its type, color, position
- [ ] HasMoved initialises false
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T07] Instantiate board visually — 8x8 placeholder grid"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T07
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #4

## Description
Generate 64 quad/plane GameObjects at runtime from BoardManager. Apply alternating light/dark materials. Tag each tile with its Square coordinate.

## Acceptance Criteria
- [ ] 64 tiles visible in Scene view
- [ ] Correct checkerboard colouring
- [ ] Each tile tagged with file+rank
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T08] Instantiate 32 pieces at starting positions"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T08
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #6, #7

## Description
Spawn 32 placeholder cube GameObjects at correct FEN starting squares. White on ranks 1–2, Black on ranks 7–8. Link each GameObject to its Piece data object.

## Acceptance Criteria
- [ ] All 32 pieces visible at correct starting positions
- [ ] Each piece GameObject references its Piece data
- [ ] Correct colour tinting (white vs black cubes)
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T09] Implement raycasting for touch/click input"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T09
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
Camera raycast on touch/click input. Detect hit on board tile or piece GameObject. Return Square coordinate. Must work with both mouse (editor) and touch (mobile).

## Acceptance Criteria
- [ ] Click/tap on any tile returns correct file+rank
- [ ] Click/tap on a piece returns that piece's square
- [ ] Works in Unity editor (mouse) and on Android (touch)
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T10] Implement piece selection logic"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T10
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #9

## Description
Tap own piece → select (highlight). Tap elsewhere or same piece → deselect. Only active player's pieces are selectable. Visual: selected piece gets a colour ring/outline.

## Acceptance Criteria
- [ ] Only current player's pieces selectable
- [ ] Selected piece visually indicated
- [ ] Tapping elsewhere deselects
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T11] Implement Pawn movement rules"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T11
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #6

## Description
Forward 1 square (blocked if occupied). Forward 2 from starting rank (both squares must be empty). Diagonal capture (only if enemy piece present). Direction reversed for Black.

## Acceptance Criteria
- [ ] Pawn moves forward 1 on clear square
- [ ] Pawn moves forward 2 only from rank 2 (White) / rank 7 (Black)
- [ ] Pawn cannot move forward if blocked
- [ ] Pawn captures diagonally only when enemy present
- [ ] Direction correct for both colours
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T12] Implement Rook movement rules"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T12
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #6

## Description
Horizontal and vertical rays to board edge. Stop at first occupied square (include if enemy, exclude if friendly). Return all valid destination squares.

## Acceptance Criteria
- [ ] Rook slides correctly in all 4 directions
- [ ] Blocked by friendly pieces (cannot pass or land)
- [ ] Can capture enemy piece but not move past
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T13] Implement Knight movement rules"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T13
**Priority:** P0 — Critical
**Estimated Hours:** 3
**Depends on:** #6

## Description
All 8 L-shape offsets (±1,±2) and (±2,±1). Jumps over pieces. Only blocked by friendly pieces on destination. Board boundary check.

## Acceptance Criteria
- [ ] All 8 L-moves generated correctly
- [ ] Knight ignores pieces in between
- [ ] Cannot land on friendly piece
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T14] Implement Bishop movement rules"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T14
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #6

## Description
Diagonal rays in 4 directions to board edge. Same blocking logic as Rook. Stays on same colour squares throughout game.

## Acceptance Criteria
- [ ] Bishop slides along all 4 diagonals
- [ ] Blocked by friendly and enemy pieces correctly
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T15] Implement Queen movement rules"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T15
**Priority:** P0 — Critical
**Estimated Hours:** 2
**Depends on:** #12, #14

## Description
Combine Rook + Bishop move generation. All 8 directions. Reuse existing ray-cast logic.

## Acceptance Criteria
- [ ] Queen moves in all 8 directions
- [ ] Blocking logic identical to Rook/Bishop
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T16] Implement King movement rules"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T16
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #6

## Description
One square in any of 8 directions. Cannot move to a square attacked by any enemy piece. Cannot capture own pieces. Check validation must exclude moves that leave King in check.

## Acceptance Criteria
- [ ] King moves one square in all directions
- [ ] Illegal to move into check
- [ ] Cannot capture own piece
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T17] Highlight valid move tiles on selection"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T17
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #10, #11-#16

## Description
When a piece is selected, overlay valid destination tiles with a coloured highlight (green dot for empty, red tint for capture). Clear highlights on deselect.

## Acceptance Criteria
- [ ] Valid moves highlighted immediately on selection
- [ ] Capture squares visually distinct from empty move squares
- [ ] Highlights cleared when piece deselected
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T18] Implement move execution — move + capture + deselect"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T18
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #17

## Description
Player taps highlighted tile → move piece to target, update Board data, remove captured piece from board and data, deselect, clear highlights, pass turn.

## Acceptance Criteria
- [ ] Piece moves to target square (data + visual)
- [ ] Captured piece removed from scene and data
- [ ] Board state updated correctly after every move
- [ ] Highlights cleared after move
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T19] Implement turn system — White/Black alternation"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T19
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #18

## Description
White moves first. After White executes a move, switch active player to Black. Lock input for inactive player. Display whose turn it is (HUD placeholder).

## Acceptance Criteria
- [ ] Only White can move on White's turn
- [ ] Turn switches after every legal move
- [ ] Turn indicator updated in UI
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T20] Implement check detection"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T20
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #19

## Description
After every move: scan all enemy pieces' attack squares. If any attack the current King's position, set InCheck = true. Visual: flash King tile red. Filter all subsequent moves to only those that resolve check.

## Acceptance Criteria
- [ ] Check detected correctly after any move
- [ ] King tile visually flagged when in check
- [ ] Player can only make moves that exit check
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T21] Implement checkmate detection — end game"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T21
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #20

## Description
After a move puts opponent in check: test all opponent's legal moves. If zero moves available AND in check → checkmate. Trigger GameOver(winner).

## Acceptance Criteria
- [ ] Checkmate detected in all standard positions (Scholar's Mate, Back Rank Mate, etc.)
- [ ] Game ends and winner declared
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T22] Implement stalemate detection — draw"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T22
**Priority:** P0 — Critical
**Estimated Hours:** 3
**Depends on:** #21

## Description
If active player has zero legal moves AND is NOT in check → stalemate. Trigger GameOver(draw).

## Acceptance Criteria
- [ ] Stalemate detected correctly
- [ ] Game ends as draw (not win/loss)
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T23] Implement special move — Castling"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T23
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #20

## Description
Kingside and queenside castling. Conditions: neither King nor Rook has moved; no pieces between them; King not in check; King does not pass through or land on attacked square.

## Acceptance Criteria
- [ ] Kingside castling executes correctly
- [ ] Queenside castling executes correctly
- [ ] All 5 conditions enforced (no move, no check, no obstruction)
- [ ] Castling unavailable after King or Rook has moved
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T24] Implement special move — En passant"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T24
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #11

## Description
After enemy pawn double-step, the passing square is available for capture by adjacent pawn for exactly one turn. State flag cleared after next move.

## Acceptance Criteria
- [ ] En passant available immediately after enemy double-step
- [ ] En passant no longer available after next move
- [ ] Captured pawn removed correctly
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T25] Implement special move — Pawn promotion"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T25
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #11

## Description
When a pawn reaches rank 8 (White) or rank 1 (Black): pause game, show piece selection UI (Queen/Rook/Bishop/Knight). Replace pawn with chosen piece. Default to Queen after 10s timeout.

## Acceptance Criteria
- [ ] Promotion triggered on reaching back rank
- [ ] All 4 promotion choices available
- [ ] Board updated with new piece immediately
- [ ] Game continues after promotion
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:feature")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  @{
    title     = "[P1-T26] Phase 1 — Manual playtest (2 full games)"
    body      = @"
**Phase:** Phase 1 — Chess Engine Core
**Task ID:** P1-T26
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #21, #22, #23, #24, #25

## Description
Two humans play 2 complete games from opening to checkmate. Verify all special moves occur at least once. Log every bug found.

## Acceptance Criteria
- [ ] 2 complete games played without crashes
- [ ] All special moves tested (castling, en passant, promotion)
- [ ] No illegal moves possible
- [ ] Checkmate correctly ends game both times
- [ ] All bugs logged as new issues
"@
    labels    = @("phase:1-chess-engine","P0-critical","type:qa")
    milestone = MN("Phase 1: Chess Engine Core")
  },

  # ─── PHASE 2: AI OPPONENT ────────────────────────────────────────────────
  @{
    title     = "[P2-T01] Create ChessAI.cs — pluggable AI interface"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T01
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #26

## Description
IChessAI interface: Move GetBestMove(BoardState, PieceColor, int depth). ChessAI.cs implementation. Pluggable so future engines can replace it without touching GameManager.

## Acceptance Criteria
- [ ] Interface defined
- [ ] ChessAI.cs implements interface
- [ ] GameManager calls AI via interface (not concrete class)
"@
    labels    = @("phase:2-ai","P0-critical","type:feature")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T02] Implement board evaluation function"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T02
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #27

## Description
Static board evaluator: material score (Pawn=100, Knight=320, Bishop=330, Rook=500, Queen=900, King=20000). Piece-square bonus tables for positional play. Returns score from AI's perspective.

## Acceptance Criteria
- [ ] Evaluation returns positive score for winning positions
- [ ] Captures scored correctly
- [ ] Positional tables applied (centre control, king safety)
"@
    labels    = @("phase:2-ai","P0-critical","type:feature")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T03] Implement minimax algorithm"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T03
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #28

## Description
Recursive minimax with configurable depth. Maximise for AI colour, minimise for opponent. Return best move at root. Handle terminal states (checkmate, stalemate).

## Acceptance Criteria
- [ ] AI selects the only legal move in forced-mate positions
- [ ] Depth=1 AI makes basic captures
- [ ] Depth=3 AI avoids simple blunders
"@
    labels    = @("phase:2-ai","P0-critical","type:feature")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T04] Implement alpha-beta pruning"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T04
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #29

## Description
Add α-β cutoffs to minimax. Must reduce search time by ≥50% vs unoptimised minimax at depth 4. Move ordering: captures first, then centre moves.

## Acceptance Criteria
- [ ] Same moves selected as minimax without pruning
- [ ] Depth=4 completes in < 3 seconds on Snapdragon 700
- [ ] Move ordering applied (captures prioritised)
"@
    labels    = @("phase:2-ai","P0-critical","type:feature")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T05] Implement AI difficulty scaling per chapter"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T05
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #30

## Description
Map story chapter to AI depth: Ch1=depth 2 (Beginner), Ch2-3=depth 3, Ch4-5=depth 4, Ch6-7=depth 5, Ch8=depth 6 (Boss). Expose DifficultyLevel setting in GameManager.

## Acceptance Criteria
- [ ] AI clearly easier in Chapter 1 than Chapter 8
- [ ] Difficulty change takes effect immediately on chapter load
"@
    labels    = @("phase:2-ai","P0-critical","type:feature")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T06] Run AI on background thread (async)"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T06
**Priority:** P1 — High
**Estimated Hours:** 5
**Depends on:** #31

## Description
Move minimax computation to C# Task/Thread. GameManager shows 'Thinking…' spinner while AI computes. Apply result on main thread when done. No frame rate drop during AI turn.

## Acceptance Criteria
- [ ] Zero frame drops during AI computation
- [ ] 'Thinking' indicator visible during AI turn
- [ ] AI move applies correctly after async completion
"@
    labels    = @("phase:2-ai","P1-high","type:perf")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T07] Profile AI performance on target Android device"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T07
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #33

## Description
Deploy to physical Android device (Snapdragon 700-series or equivalent). Measure AI move time at depth 2–6. Ensure all difficulty levels complete within 3 seconds. Profile memory usage.

## Acceptance Criteria
- [ ] Depth 2: < 0.5s
- [ ] Depth 4: < 2s
- [ ] Depth 6: < 3s
- [ ] No memory leaks after 10 AI turns
"@
    labels    = @("phase:2-ai","P0-critical","type:perf")
    milestone = MN("Phase 2: AI Opponent")
  },

  @{
    title     = "[P2-T08] Phase 2 — AI playtest (all difficulty levels)"
    body      = @"
**Phase:** Phase 2 — AI Opponent
**Task ID:** P2-T08
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #34

## Description
Play 2 games against AI at each difficulty level (1–5). Verify AI never makes illegal moves. Verify difficulty feels appropriate. Log any crashes or illegal AI moves.

## Acceptance Criteria
- [ ] AI never makes an illegal move across 10 games
- [ ] Easy (Ch1) difficulty is beatable by a beginner
- [ ] Hard (Ch8) difficulty challenges an intermediate player
- [ ] All 10 games complete without crashes
"@
    labels    = @("phase:2-ai","P0-critical","type:qa")
    milestone = MN("Phase 2: AI Opponent")
  },

  # ─── PHASE 3: 3D ART & CAMERA ─────────────────────────────────────────
  @{
    title     = "[P3-T01] Set up isometric orthographic camera (portrait)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T01
**Priority:** P0 — Critical
**Estimated Hours:** 4

## Description
Camera: orthographic, position (0,18,0), rotation (30°,45°,0°). Framing: board fills upper 70% of portrait screen. Orthographic size calculated from board dimensions.

## Acceptance Criteria
- [ ] Board fully visible and centred in portrait frame
- [ ] No perspective distortion
- [ ] Board occupies 65–75% of screen height
- [ ] Looks correct on 1080x2340 and 720x1560
"@
    labels    = @("phase:3-art","P0-critical","type:feature")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T02] Implement responsive board scaling across screen sizes"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T02
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #37

## Description
Orthographic size dynamically calculated from screen aspect ratio. UI Canvas anchors below board. Board stays proportional on 720p, 1080p, 1440p portrait displays and tablet (16:10).

## Acceptance Criteria
- [ ] Board correctly framed on 720p portrait
- [ ] Board correctly framed on 1440p portrait
- [ ] No board/UI overlap on any tested screen
"@
    labels    = @("phase:3-art","P0-critical","type:feature")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T03] Model chessboard (low-poly, < 500 tris)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T03
**Priority:** P0 — Critical
**Estimated Hours:** 8

## Description
Low-poly board mesh: 8x8 squares integrated into single mesh, edge trim, base. Materials: light square, dark square, border. Baked AO texture. Target: < 500 tris.

## Acceptance Criteria
- [ ] Board looks clean and readable at isometric angle
- [ ] Under 500 triangles
- [ ] Single draw call for entire board (one material)
"@
    labels    = @("phase:3-art","P0-critical","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T04] Model 6 White piece types (low-poly fantasy)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T04
**Priority:** P0 — Critical
**Estimated Hours:** 24

## Description
King, Queen, Rook, Bishop, Knight, Pawn — White / light kingdom theme. Stylized low-poly. Each piece ~600 tris at high LOD. Silhouette must be instantly recognisable at board scale.

## Acceptance Criteria
- [ ] All 6 piece types modelled and imported
- [ ] Each piece ≤ 800 tris (high LOD)
- [ ] Silhouettes clearly distinguish each type
- [ ] Consistent scale: Pawn shortest, King tallest
"@
    labels    = @("phase:3-art","P0-critical","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T05] Model 6 Black piece types (shadow/dark kingdom theme)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T05
**Priority:** P0 — Critical
**Estimated Hours:** 12
**Depends on:** #41

## Description
Black / older brother's dark kingdom theme. Reuse mesh topology where possible, vary silhouette details. Dark stone / obsidian aesthetic. Same poly budget as White set.

## Acceptance Criteria
- [ ] 6 Black piece types modelled and imported
- [ ] Visually distinct from White set but same piece-type silhouettes
- [ ] ≤ 800 tris each
"@
    labels    = @("phase:3-art","P0-critical","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T06] Model Goblin faction piece set (Chapter 2 enemies)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T06
**Priority:** P1 — High
**Estimated Hours:** 16

## Description
Replace Black pieces with goblin-themed models for Chapter 2. Maintain piece-type silhouette readability. Goblin green palette, crude weapons, crude armour.

## Acceptance Criteria
- [ ] 6 Goblin piece types modelled
- [ ] Piece roles immediately identifiable by silhouette
- [ ] Loaded at runtime by NarrativeController on Ch2
"@
    labels    = @("phase:3-art","P1-high","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T07] Model Orc faction piece set (Chapter 3 enemies)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T07
**Priority:** P1 — High
**Estimated Hours:** 16

## Description
Orc-themed piece set for Chapter 3. Brute strength aesthetic — heavy armour, axes, tusks. Brown/grey earth palette.

## Acceptance Criteria
- [ ] 6 Orc piece types modelled
- [ ] Visually distinct from Goblin set
- [ ] Correct faction loaded on Chapter 3
"@
    labels    = @("phase:3-art","P1-high","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T08] Create texture atlas for piece sets (1024x1024 per faction)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T08
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #41, #42

## Description
Pack all 6 piece UV maps into a single 1024×1024 texture atlas per faction. One material per faction team. Goal: ≤ 2 draw calls per team during board rendering.

## Acceptance Criteria
- [ ] White team: 1 atlas, 1 material
- [ ] Black/enemy team: 1 atlas, 1 material
- [ ] No visible seams or UV bleeding
- [ ] ≤ 4 draw calls for all 32 pieces combined
"@
    labels    = @("phase:3-art","P0-critical","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T09] Implement piece move animation (lerp 0.4s)"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T09
**Priority:** P1 — High
**Estimated Hours:** 6
**Depends on:** #41

## Description
Lerp piece world position from source tile to destination tile over 0.4s with ease-in-out curve. Optional slight arc for jumping pieces (Knight). Block input during animation.

## Acceptance Criteria
- [ ] Piece smoothly animates to new position
- [ ] Input blocked during animation (no double-moves)
- [ ] Knight has slight arc; sliding pieces stay at board level
"@
    labels    = @("phase:3-art","P1-high","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T10] Implement piece capture animation"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T10
**Priority:** P1 — High
**Estimated Hours:** 4
**Depends on:** #47

## Description
Captured piece: scale down + fade out over 0.3s OR fly off board. Play simultaneously with attacker moving in. Remove GameObject after animation.

## Acceptance Criteria
- [ ] Captured piece visually removed with animation
- [ ] No lingering GameObjects after capture
- [ ] Animation feels satisfying and readable
"@
    labels    = @("phase:3-art","P1-high","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T11] Implement chapter environment / skybox per theme"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T11
**Priority:** P1 — High
**Estimated Hours:** 12

## Description
Unique skybox + background dressing per chapter theme:
- Ch1: Small kingdom (golden sunrise, stone walls)
- Ch2: Goblin caves (dark cavern, torchlight)
- Ch3: Orc wastes (stormy plains, fire)
Swap via NarrativeController on chapter load.

## Acceptance Criteria
- [ ] Distinct visual atmosphere per chapter
- [ ] Background loads without frame spike
- [ ] Consistent lighting with piece materials
"@
    labels    = @("phase:3-art","P1-high","type:art")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  @{
    title     = "[P3-T12] Visual QA on multiple screen sizes"
    body      = @"
**Phase:** Phase 3 — 3D Art & Camera
**Task ID:** P3-T12
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #38

## Description
Test board framing, piece readability, and UI layout on: 720×1560, 1080×2340, 1440×3200, and one tablet (2048×1536). Fix any cropping or overlap.

## Acceptance Criteria
- [ ] Board fully visible on all 4 tested resolutions
- [ ] No UI/board overlap
- [ ] Piece silhouettes readable at smallest tested resolution
"@
    labels    = @("phase:3-art","P0-critical","type:qa")
    milestone = MN("Phase 3: 3D Art and Camera")
  },

  # ─── PHASE 4: STORY & NARRATIVE ──────────────────────────────────────────
  @{
    title     = "[P4-T01] Install and configure Yarn Spinner"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T01
**Priority:** P0 — Critical
**Estimated Hours:** 3

## Description
Add Yarn Spinner via Unity Package Manager. Create YarnProject asset. Add DialogueRunner component to a persistent NarrativeManager GameObject. Wire up basic test dialogue.

## Acceptance Criteria
- [ ] Yarn Spinner installed (no console errors)
- [ ] Test dialogue displays in Unity Play mode
- [ ] DialogueRunner pauses/resumes game correctly
"@
    labels    = @("phase:4-story","P0-critical","type:feature")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T02] Write Story Bible — full narrative document (all 8 chapters)"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T02
**Priority:** P0 — Critical
**Estimated Hours:** 24

## Description
Complete narrative document covering:
- Brothers' backstory and separation event
- Why the older brother became the antagonist
- White brother's small kingdom setup
- Chapter-by-chapter story beats (Ch1–8)
- Enemy faction roles in the story
- Final confrontation and resolution options

## Acceptance Criteria
- [ ] All 8 chapter story beats documented
- [ ] Character motivations clear and consistent
- [ ] Enemy factions tied to narrative (not just skins)
- [ ] Document reviewed and approved before dialogue writing
"@
    labels    = @("phase:4-story","P0-critical","type:story")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T03] Write Chapter 1 dialogue (.yarn scripts)"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T03
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #52

## Description
Yarn scripts for Chapter 1: opening intro (brothers' backstory hint), pre-match scene-setter, 2–3 mid-match optional dialogue lines, post-victory beat, post-defeat beat.

## Acceptance Criteria
- [ ] All 5 dialogue moments scripted
- [ ] Dialogue plays in-game without errors
- [ ] Tone matches story bible (earnest, determined younger brother)
"@
    labels    = @("phase:4-story","P0-critical","type:story")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T04] Write Chapter 2–4 dialogue (.yarn scripts)"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T04
**Priority:** P0 — Critical
**Estimated Hours:** 14
**Depends on:** #52

## Description
Yarn dialogue for Ch2 (Goblin king), Ch3 (Orc warlord), Ch4 (first Elf encounter — first revelation about older brother's true nature).

## Acceptance Criteria
- [ ] Each chapter has pre/post-match dialogue
- [ ] Story escalates — stakes feel higher each chapter
- [ ] Ch4 delivers first major story revelation
"@
    labels    = @("phase:4-story","P0-critical","type:story")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T05] Write Chapter 5–8 dialogue (.yarn scripts)"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T05
**Priority:** P1 — High
**Estimated Hours:** 16
**Depends on:** #52

## Description
Yarn dialogue for Ch5–7 (escalating factions) and Ch8 (final confrontation with the older brother). Chapter 8 should feel emotionally resonant — not just another match.

## Acceptance Criteria
- [ ] Ch8 pre-match sets up the emotional climax
- [ ] Post-victory (Ch8) delivers narrative resolution
- [ ] Optional post-defeat dialogue doesn't break story state
"@
    labels    = @("phase:4-story","P1-high","type:story")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T06] Implement NarrativeController.cs"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T06
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #51

## Description
Manages story state machine. Triggers Yarn dialogue before/after matches. Tracks story flags (e.g. brotherRevealSeen=true). Swaps enemy faction models per chapter. Fires Unity Events for cutscene triggers.

## Acceptance Criteria
- [ ] Pre-match dialogue fires before every chapter's first game
- [ ] Post-match dialogue fires on victory and defeat
- [ ] Story flags persist between sessions
- [ ] Faction model swap works on chapter load
"@
    labels    = @("phase:4-story","P0-critical","type:feature")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T07] Implement chapter unlock system"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T07
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #57

## Description
Chapters 1–3 free. Chapters 4+ locked behind IAP. NarrativeController checks unlock state before loading chapter. Unlocked state stored in SaveManager. IAP receipt validates unlock.

## Acceptance Criteria
- [ ] Ch1–3 load without IAP
- [ ] Ch4+ shows lock screen with purchase prompt
- [ ] After IAP, chapter unlocks permanently
- [ ] Restore purchases re-unlocks on reinstall
"@
    labels    = @("phase:4-story","P0-critical","type:feature")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T08] Implement story progress save/load"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T08
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #58

## Description
SaveManager serialises: current chapter, completed chapters[], story flags dictionary, IAP unlocks[], last match result. Saved to JSON in Application.persistentDataPath. Loaded on app start.

## Acceptance Criteria
- [ ] Kill app mid-chapter, reopen — resume at correct chapter
- [ ] All story flags restored correctly
- [ ] IAP unlocks survive reinstall (via restore purchases)
"@
    labels    = @("phase:4-story","P0-critical","type:feature")
    milestone = MN("Phase 4: Story and Narrative")
  },

  @{
    title     = "[P4-T09] Phase 4 — Story QA playthrough (Ch 1–4)"
    body      = @"
**Phase:** Phase 4 — Story & Narrative
**Task ID:** P4-T09
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #57, #59

## Description
Play all 4 free chapters end-to-end. Verify all dialogue triggers at the right moments. Test victory and defeat story paths. Verify chapter unlock gate. Log all dialogue/story bugs.

## Acceptance Criteria
- [ ] All story triggers fire correctly in Ch1–4
- [ ] No duplicate or skipped dialogue
- [ ] Victory and defeat paths both tested
- [ ] Save/load works across chapter boundary
"@
    labels    = @("phase:4-story","P0-critical","type:qa")
    milestone = MN("Phase 4: Story and Narrative")
  },

  # ─── PHASE 5: UI/UX ──────────────────────────────────────────────────────
  @{
    title     = "[P5-T01] Design & implement main menu screen"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T01
**Priority:** P0 — Critical
**Estimated Hours:** 10

## Description
Portrait main menu: CHESS logo, animated background (chessboard + slow pan), Play, Settings, Credits buttons. Scene transition to Chapter Select with fade.

## Acceptance Criteria
- [ ] Logo and all buttons visible on 720p–1440p portrait
- [ ] All buttons respond to touch correctly
- [ ] Smooth scene transition to Chapter Select
"@
    labels    = @("phase:5-ui","P0-critical","type:feature")
    milestone = MN("Phase 5: UI UX")
  },

  @{
    title     = "[P5-T02] Design & implement chapter select screen"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T02
**Priority:** P0 — Critical
**Estimated Hours:** 10
**Depends on:** #58

## Description
Scrollable chapter map/list. Each node shows: chapter number, name (story-based), completion status (crown/checkmark). Locked chapters show padlock + IAP price. Tap unlocked chapter → load game scene.

## Acceptance Criteria
- [ ] All 8 chapters listed
- [ ] Locked/unlocked state correct per save data
- [ ] IAP prompt fires on locked chapter tap
- [ ] Completed chapters show completion indicator
"@
    labels    = @("phase:5-ui","P0-critical","type:feature")
    milestone = MN("Phase 5: UI UX")
  },

  @{
    title     = "[P5-T03] Design & implement in-game HUD"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T03
**Priority:** P0 — Critical
**Estimated Hours:** 10

## Description
Portrait HUD overlay (bottom 30% of screen): player portrait + name (left), enemy portrait + name (right), turn indicator (glow on active side), captured pieces row, move counter, check warning banner.

## Acceptance Criteria
- [ ] HUD visible without obscuring board
- [ ] Turn indicator updates in real time
- [ ] Check warning appears when King is in check
- [ ] Captured pieces display correctly
"@
    labels    = @("phase:5-ui","P0-critical","type:feature")
    milestone = MN("Phase 5: UI UX")
  },

  @{
    title     = "[P5-T04] Implement dialogue UI overlay"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T04
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #57

## Description
Dialogue box anchored at bottom. Character portrait (left), name tag, text body (TextMeshPro with typewriter effect). Tap-to-advance, Skip button. Integrates with Yarn Spinner DialogueRunner output.

## Acceptance Criteria
- [ ] Dialogue displays over game scene without blocking board
- [ ] Typewriter effect plays correctly
- [ ] Tap advances; Skip dismisses entire scene
- [ ] Character portrait swaps per speaker
"@
    labels    = @("phase:5-ui","P0-critical","type:feature")
    milestone = MN("Phase 5: UI UX")
  },

  @{
    title     = "[P5-T05] Implement game over screen"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T05
**Priority:** P0 — Critical
**Estimated Hours:** 5

## Description
Victory screen: crown icon + story context text + 'Continue' button (advances story). Defeat screen: sword icon + retry prompt + 'Try Again' / 'Main Menu'. Animated entrance.

## Acceptance Criteria
- [ ] Correct screen shown for win/loss/draw
- [ ] Story context text populated from NarrativeController
- [ ] Buttons work correctly
"@
    labels    = @("phase:5-ui","P0-critical","type:feature")
    milestone = MN("Phase 5: UI UX")
  },

  @{
    title     = "[P5-T06] Implement settings screen"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T06
**Priority:** P1 — High
**Estimated Hours:** 5

## Description
Settings: Music Volume (slider), SFX Volume (slider), Show Move Hints (toggle), Piece Animation Speed (toggle fast/slow), Privacy Policy link, Version number.

## Acceptance Criteria
- [ ] All settings persist via PlayerPrefs
- [ ] Audio sliders affect in-game volume immediately
- [ ] Move hints toggle works in-game
"@
    labels    = @("phase:5-ui","P1-high","type:feature")
    milestone = MN("Phase 5: UI UX")
  },

  @{
    title     = "[P5-T07] Phase 5 — UI/UX QA on physical device"
    body      = @"
**Phase:** Phase 5 — UI/UX
**Task ID:** P5-T07
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #61-#66

## Description
Test all screens on a physical Android device. Verify: touch targets ≥ 44dp, no overlapping elements, all fonts legible at 720p, back button behaviour, no text truncation.

## Acceptance Criteria
- [ ] All buttons respond correctly to touch
- [ ] No overlapping UI elements on any screen
- [ ] Readable on 720p (smallest supported)
- [ ] Back button navigates correctly throughout
"@
    labels    = @("phase:5-ui","P0-critical","type:qa")
    milestone = MN("Phase 5: UI UX")
  },

  # ─── PHASE 6: AUDIO ──────────────────────────────────────────────────────
  @{
    title     = "[P6-T01] Source/produce background music for Chapters 1–3"
    body      = @"
**Phase:** Phase 6 — Audio
**Task ID:** P6-T01
**Priority:** P1 — High
**Estimated Hours:** 12

## Description
3 looping ambient tracks: Ch1 Kingdom Theme (hopeful, orchestral lite), Ch2 Goblin Caves (dark, percussive), Ch3 Orc Wastes (heavy, drums). ~2 min each, seamless loop. OGG format.

## Acceptance Criteria
- [ ] 3 tracks produced/sourced and imported
- [ ] Each track loops seamlessly
- [ ] Fits emotional tone of chapter
"@
    labels    = @("phase:6-audio","P1-high","type:feature")
    milestone = MN("Phase 6: Audio")
  },

  @{
    title     = "[P6-T02] Implement piece movement & capture SFX"
    body      = @"
**Phase:** Phase 6 — Audio
**Task ID:** P6-T02
**Priority:** P1 — High
**Estimated Hours:** 4

## Description
Wood-on-board click for piece movement. Heavier impact for capture. Varies subtly per faction (stone pieces = heavier; goblin pieces = clunk). Played via AudioManager singleton.

## Acceptance Criteria
- [ ] Movement SFX plays on every piece move
- [ ] Capture SFX distinct from movement SFX
- [ ] No audio clipping or delay > 50ms
"@
    labels    = @("phase:6-audio","P1-high","type:feature")
    milestone = MN("Phase 6: Audio")
  },

  @{
    title     = "[P6-T03] Implement check, checkmate, and victory SFX"
    body      = @"
**Phase:** Phase 6 — Audio
**Task ID:** P6-T03
**Priority:** P1 — High
**Estimated Hours:** 3

## Description
Check: short dramatic sting. Checkmate (player wins): victory fanfare. Checkmate (player loses): defeat sting. Stalemate: neutral resolve tone.

## Acceptance Criteria
- [ ] All 4 audio events play at correct game moments
- [ ] No audio overlap with background music (duck BG on sting)
"@
    labels    = @("phase:6-audio","P1-high","type:feature")
    milestone = MN("Phase 6: Audio")
  },

  @{
    title     = "[P6-T04] Implement UI SFX and AudioManager"
    body      = @"
**Phase:** Phase 6 — Audio
**Task ID:** P6-T04
**Priority:** P2 — Medium
**Estimated Hours:** 4

## Description
AudioManager singleton: PlaySFX(clip), PlayMusic(clip, loop), SetMusicVolume(), SetSFXVolume(). UI sounds: button tap, menu transition, chapter unlock fanfare, dialogue advance click.

## Acceptance Criteria
- [ ] AudioManager accessible from any script
- [ ] Music fades between chapters (1s crossfade)
- [ ] All UI interactions have audio feedback
- [ ] Volume settings from Settings screen apply correctly
"@
    labels    = @("phase:6-audio","P2-medium","type:feature")
    milestone = MN("Phase 6: Audio")
  },

  # ─── PHASE 7: MOBILE OPTIMISATION ────────────────────────────────────────
  @{
    title     = "[P7-T01] Profile game on target Android device"
    body      = @"
**Phase:** Phase 7 — Mobile Optimization
**Task ID:** P7-T01
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
Deploy Development Build to physical Android (Snapdragon 700-class). Run Unity Profiler deep profile over one full game. Capture: CPU ms/frame, GPU ms/frame, draw calls, memory, GC alloc.

## Acceptance Criteria
- [ ] Profiling data captured for CPU, GPU, memory
- [ ] Identify top 3 performance bottlenecks
- [ ] Baseline documented (before optimization)
"@
    labels    = @("phase:7-optimization","P0-critical","type:perf")
    milestone = MN("Phase 7: Mobile Optimization")
  },

  @{
    title     = "[P7-T02] Optimise draw calls — SRP Batcher + GPU Instancing"
    body      = @"
**Phase:** Phase 7 — Mobile Optimization
**Task ID:** P7-T02
**Priority:** P0 — Critical
**Estimated Hours:** 5
**Depends on:** #72

## Description
Enable SRP Batcher in URP settings. Apply GPU Instancing on piece materials. Use MaterialPropertyBlock for per-piece colour variation. Target: < 20 draw calls for full board scene.

## Acceptance Criteria
- [ ] Draw calls reduced to ≤ 20 for board + pieces scene
- [ ] No visual regressions after batching changes
"@
    labels    = @("phase:7-optimization","P0-critical","type:perf")
    milestone = MN("Phase 7: Mobile Optimization")
  },

  @{
    title     = "[P7-T03] Compress all textures for mobile (ASTC/PVRTC)"
    body      = @"
**Phase:** Phase 7 — Mobile Optimization
**Task ID:** P7-T03
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #72

## Description
Set all texture import settings: Android → ASTC 6x6, iOS → ASTC 6x6 (PVRTC fallback). Verify no visible quality loss at 1080p portrait. Check total texture memory < 150 MB.

## Acceptance Criteria
- [ ] All textures use mobile compression format
- [ ] Texture memory ≤ 150 MB at runtime
- [ ] No visible quality loss on 1080p device
"@
    labels    = @("phase:7-optimization","P0-critical","type:perf")
    milestone = MN("Phase 7: Mobile Optimization")
  },

  @{
    title     = "[P7-T04] Implement async scene loading for chapter transitions"
    body      = @"
**Phase:** Phase 7 — Mobile Optimization
**Task ID:** P7-T04
**Priority:** P1 — High
**Estimated Hours:** 6
**Depends on:** #72

## Description
LoadSceneAsync for all scene transitions. Show loading screen with progress bar during load. Unload previous scene assets immediately. Target: < 3s chapter load on mid-range device.

## Acceptance Criteria
- [ ] No frame freeze on scene transition
- [ ] Loading screen visible with progress indicator
- [ ] Chapter loads within 3 seconds on target device
"@
    labels    = @("phase:7-optimization","P1-high","type:perf")
    milestone = MN("Phase 7: Mobile Optimization")
  },

  @{
    title     = "[P7-T05] Configure Android + iOS release build settings"
    body      = @"
**Phase:** Phase 7 — Mobile Optimization
**Task ID:** P7-T05
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
Android: ARM64 + ARMv7, IL2CPP, Managed Stripping=Medium, target API 33, min API 26. iOS: arm64 only, IL2CPP, min iOS 13, bitcode off. Both: disable Development Build flag.

## Acceptance Criteria
- [ ] Android AAB builds and installs on device
- [ ] iOS IPA archives in Xcode
- [ ] Neither build shows Development Build watermark
- [ ] App size < 150 MB
"@
    labels    = @("phase:7-optimization","P0-critical","type:devops")
    milestone = MN("Phase 7: Mobile Optimization")
  },

  @{
    title     = "[P7-T06] Performance QA on 3 device tiers"
    body      = @"
**Phase:** Phase 7 — Mobile Optimization
**Task ID:** P7-T06
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #72-#76

## Description
Test on: Low-end Android (2018-era, 2GB RAM), Mid-range (2021, Snapdragon 720G), High-end (2023, Snapdragon 8 Gen 2). Measure FPS, load time, battery at all 3 tiers.

## Acceptance Criteria
- [ ] Low-end: ≥ 30 FPS sustained
- [ ] Mid-range: ≥ 60 FPS during gameplay
- [ ] High-end: stable 60 FPS
- [ ] All tiers: chapter loads < 5s
"@
    labels    = @("phase:7-optimization","P0-critical","type:qa")
    milestone = MN("Phase 7: Mobile Optimization")
  },

  # ─── PHASE 8: QA & BUG FIX ───────────────────────────────────────────────
  @{
    title     = "[P8-T01] Create master test plan document"
    body      = @"
**Phase:** Phase 8 — QA & Bug Fix
**Task ID:** P8-T01
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
Document all test cases: chess rules (all pieces, all special moves, all edge cases), story triggers (all 8 chapters, victory/defeat paths), UI (all screens, all buttons), AI (all difficulties), save/load.

## Acceptance Criteria
- [ ] Test plan covers all functional requirements
- [ ] Each test case has: input, expected output, pass/fail
- [ ] Stored in /docs/TEST_PLAN.md in repo
"@
    labels    = @("phase:8-qa","P0-critical","type:qa")
    milestone = MN("Phase 8: QA and Bug Fix")
  },

  @{
    title     = "[P8-T02] Chess rules regression — 10 full games"
    body      = @"
**Phase:** Phase 8 — QA & Bug Fix
**Task ID:** P8-T02
**Priority:** P0 — Critical
**Estimated Hours:** 10
**Depends on:** #79

## Description
Play 10 complete games covering: Scholar's Mate, Back Rank Mate, stalemate, all castling variants, en passant, all 4 promotion choices, discovered check, double check.

## Acceptance Criteria
- [ ] 10 games completed without crashes
- [ ] All special moves function correctly
- [ ] No illegal moves accepted
- [ ] All end-game conditions trigger correctly
"@
    labels    = @("phase:8-qa","P0-critical","type:qa")
    milestone = MN("Phase 8: QA and Bug Fix")
  },

  @{
    title     = "[P8-T03] Story progression regression (all 8 chapters)"
    body      = @"
**Phase:** Phase 8 — QA & Bug Fix
**Task ID:** P8-T03
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #79

## Description
Complete all 8 chapters including IAP unlock flow. Test victory + defeat path for each chapter. Verify save/load at every chapter boundary. All story flags must persist.

## Acceptance Criteria
- [ ] All 8 chapters complete without story bugs
- [ ] Both victory and defeat branches tested per chapter
- [ ] Save/load works at every chapter boundary
"@
    labels    = @("phase:8-qa","P0-critical","type:qa")
    milestone = MN("Phase 8: QA and Bug Fix")
  },

  @{
    title     = "[P8-T04] Bug fix sprint — all P0 and P1 issues"
    body      = @"
**Phase:** Phase 8 — QA & Bug Fix
**Task ID:** P8-T04
**Priority:** P0 — Critical
**Estimated Hours:** 24
**Depends on:** #80, #81, #82

## Description
Address all P0 (crash, game-breaking) and P1 (major feature broken) bugs logged during QA phases. Re-test each fix. P2/P3 bugs deferred to v1.1 unless quick fix.

## Acceptance Criteria
- [ ] All P0 bugs resolved and re-tested
- [ ] All P1 bugs resolved or documented as deferred
- [ ] Regression pass confirms no new P0 introduced
"@
    labels    = @("phase:8-qa","P0-critical","type:bug")
    milestone = MN("Phase 8: QA and Bug Fix")
  },

  @{
    title     = "[P8-T05] Final sign-off regression pass"
    body      = @"
**Phase:** Phase 8 — QA & Bug Fix
**Task ID:** P8-T05
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #83

## Description
Full playthrough after all bug fixes. Play 2 games, advance through all story chapters, test all UI. Confirm zero P0 bugs remain. Product Owner sign-off required.

## Acceptance Criteria
- [ ] Zero P0 bugs open
- [ ] All P1 bugs resolved or formally deferred
- [ ] Product Owner has signed off on build
"@
    labels    = @("phase:8-qa","P0-critical","type:qa")
    milestone = MN("Phase 8: QA and Bug Fix")
  },

  # ─── PHASE 9: MONETISATION & ANALYTICS ────────────────────────────────────
  @{
    title     = "[P9-T01] Integrate Unity IAP and configure product catalogue"
    body      = @"
**Phase:** Phase 9 — Monetization & Analytics
**Task ID:** P9-T01
**Priority:** P0 — Critical
**Estimated Hours:** 8

## Description
Add Unity IAP package. Configure products in App Store Connect + Google Play Console: chapter_4_unlock ($0.99), chapter_5_unlock ($0.99), chapters_4_8_bundle ($4.99), piece_skin_pack_1 ($1.99).

## Acceptance Criteria
- [ ] All IAP products created in both stores
- [ ] Unity IAP SDK initialises without error
- [ ] Product IDs match between Unity and store consoles
"@
    labels    = @("phase:9-monetization","P0-critical","type:feature")
    milestone = MN("Phase 9: Monetization Analytics")
  },

  @{
    title     = "[P9-T02] Implement chapter unlock purchase + restore flow"
    body      = @"
**Phase:** Phase 9 — Monetization & Analytics
**Task ID:** P9-T02
**Priority:** P0 — Critical
**Estimated Hours:** 8
**Depends on:** #85

## Description
Purchase flow: user taps locked chapter → IAP prompt → on success: unlock chapter permanently in SaveManager → load chapter. Restore Purchases button in Settings restores all previous purchases.

## Acceptance Criteria
- [ ] Purchase flow completes end-to-end in sandbox
- [ ] Chapter unlocks immediately after purchase
- [ ] Restore Purchases re-unlocks on fresh install
- [ ] Failed/cancelled purchase does not unlock chapter
"@
    labels    = @("phase:9-monetization","P0-critical","type:feature")
    milestone = MN("Phase 9: Monetization Analytics")
  },

  @{
    title     = "[P9-T03] Integrate Firebase Analytics + instrument key events"
    body      = @"
**Phase:** Phase 9 — Monetization & Analytics
**Task ID:** P9-T03
**Priority:** P1 — High
**Estimated Hours:** 8

## Description
Add Firebase Unity SDK. Instrument events: session_start, chapter_start(chapter_id), chapter_complete(chapter_id, moves_made), match_result(result, difficulty), iap_initiated(product_id), iap_complete(product_id), session_end(duration).

## Acceptance Criteria
- [ ] Firebase initialises without error on Android + iOS
- [ ] All 7 events visible in Firebase console debug view
- [ ] No PII sent in any event
"@
    labels    = @("phase:9-monetization","P1-high","type:feature")
    milestone = MN("Phase 9: Monetization Analytics")
  },

  @{
    title     = "[P9-T04] IAP sandbox testing — Android + iOS"
    body      = @"
**Phase:** Phase 9 — Monetization & Analytics
**Task ID:** P9-T04
**Priority:** P0 — Critical
**Estimated Hours:** 6
**Depends on:** #86

## Description
Test all purchase flows in sandbox: buy chapter, cancel purchase, buy bundle, restore purchases, interrupted purchase (kill app mid-purchase). Android internal testing track + iOS TestFlight.

## Acceptance Criteria
- [ ] All 4 products purchasable in sandbox
- [ ] Cancelled purchase does not unlock
- [ ] Restore Purchases works on both platforms
- [ ] Interrupted purchase resolves correctly on re-open
"@
    labels    = @("phase:9-monetization","P0-critical","type:qa")
    milestone = MN("Phase 9: Monetization Analytics")
  },

  # ─── PHASE 10: LAUNCH PREPARATION ────────────────────────────────────────
  @{
    title     = "[P10-T01] Create Google Play store listing"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T01
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
Google Play Console: App icon (512x512), feature graphic (1024x500), 8 portrait screenshots, short description (80 chars), full description (4000 chars), content rating questionnaire, privacy policy URL.

## Acceptance Criteria
- [ ] All required assets uploaded
- [ ] Content rating complete (expected: Everyone 10+)
- [ ] Privacy policy URL live
- [ ] Listing looks compelling and accurate
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T02] Create Apple App Store listing"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T02
**Priority:** P0 — Critical
**Estimated Hours:** 6

## Description
App Store Connect: App icon (1024x1024), 6.5'' and 5.5'' portrait screenshots (required sizes), app preview video (optional), metadata, age rating, privacy labels.

## Acceptance Criteria
- [ ] All screenshot sizes uploaded
- [ ] Age rating complete
- [ ] Privacy nutrition labels filled correctly
- [ ] Keywords optimised for discoverability
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T03] Generate signed Android AAB release build"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T03
**Priority:** P0 — Critical
**Estimated Hours:** 4

## Description
Create release keystore. Build signed AAB with release configuration. Verify on physical device via sideload. Upload to Google Play internal testing track. Confirm < 150 MB.

## Acceptance Criteria
- [ ] Signed AAB installs cleanly on Android
- [ ] No 'Development Build' watermark
- [ ] App size ≤ 150 MB
- [ ] Keystore backed up securely (not in repo)
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T04] Generate iOS IPA and upload to TestFlight"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T04
**Priority:** P0 — Critical
**Estimated Hours:** 4

## Description
Archive in Xcode with Release configuration. Upload to App Store Connect via Transporter. Distribute to TestFlight internal testers. Confirm install on physical iPhone.

## Acceptance Criteria
- [ ] IPA archives without build errors
- [ ] TestFlight build available for internal testers
- [ ] Installs and runs on iPhone (iOS 13+)
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T05] Closed beta — 15–20 external testers"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T05
**Priority:** P0 — Critical
**Estimated Hours:** 10
**Depends on:** #91, #92

## Description
Recruit 15–20 testers via Google Play internal testing + TestFlight. Structured feedback form covering: chess rule correctness, story engagement, difficulty feel, UI clarity, performance, overall rating. 1-week feedback window.

## Acceptance Criteria
- [ ] ≥ 15 testers complete feedback form
- [ ] No P0 bugs reported by testers
- [ ] Average overall rating ≥ 3.5/5 from testers
- [ ] All tester-reported P0/P1 bugs fixed before launch
"@
    labels    = @("phase:10-launch","P0-critical","type:qa")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T06] Submit to Google Play for review"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T06
**Priority:** P0 — Critical
**Estimated Hours:** 3
**Depends on:** #93

## Description
Upload final signed AAB to Production track. Complete content rating, pricing (Free), countries (worldwide). Submit for Google Play review (typically 3–7 days).

## Acceptance Criteria
- [ ] Build submitted to Production track
- [ ] No policy violations flagged during upload
- [ ] Review approval received
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T07] Submit to Apple App Store for review"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T07
**Priority:** P0 — Critical
**Estimated Hours:** 3
**Depends on:** #93

## Description
Submit IPA for App Store review via App Store Connect. Complete all required metadata, export compliance, content rights. Apple review typically 1–3 days.

## Acceptance Criteria
- [ ] Build submitted for review
- [ ] No rejection for metadata or policy issues
- [ ] App Store approval received
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  },

  @{
    title     = "[P10-T08] Launch day checklist and go-live"
    body      = @"
**Phase:** Phase 10 — Launch Preparation
**Task ID:** P10-T08
**Priority:** P0 — Critical
**Estimated Hours:** 4
**Depends on:** #94, #95

## Description
Launch day: set Android/iOS release live, post social media launch announcement, monitor Firebase Crashlytics for Day-1 crash rate (target < 0.1%), monitor store reviews, respond to early user feedback.

## Acceptance Criteria
- [ ] Both store listings live and publicly downloadable
- [ ] Social media launch post published
- [ ] Crashlytics monitored for first 24 hours
- [ ] Day-1 crash rate < 0.1%
"@
    labels    = @("phase:10-launch","P0-critical","type:devops")
    milestone = MN("Phase 10: Launch Preparation")
  }
)

Write-Host "Total issues to create: $($issues.Count)"
Write-Host "Starting issue creation..."

$created = 0
foreach ($issue in $issues) {
  $labelStr = $issue.labels -join ","
  $result = gh issue create `
    --repo $Repo `
    --title $issue.title `
    --body $issue.body `
    --label $labelStr `
    --milestone $issue.milestone
  if ($LASTEXITCODE -eq 0) {
    $created++
    $num = ($result -split "/")[-1]
    Write-Host "  [$created/$($issues.Count)] #$num — $($issue.title.Substring(0, [Math]::Min(60, $issue.title.Length)))"
  } else {
    Write-Host "  FAILED: $($issue.title)"
  }
}

Write-Host "`n✓ Created $created / $($issues.Count) issues"

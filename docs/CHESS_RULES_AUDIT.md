# Chess Rules Audit

> Read-only audit of the chess rules layer. No code changes. Each finding lists file:line, severity, impact, and a recommended fix. Use as a punch list for future sessions.

**Scope:** `Piece` + all subclasses, `MoveValidator`, `BoardManager`, `AttackChecker`, `MoveSelector`, `BoardVisualizer`, `GameManager` (game-end detection). Did NOT audit `ChessAI`, `Evaluator`, `TranspositionTable`.

**Date:** 2026-05-12. **Auditor:** Claude (Opus 4.7).

---

## Executive summary

The rules layer is mostly correct and the three-place pattern (data / sim / visual) for special moves is consistently followed. **No critical playable bugs.** Two HIGH-severity findings (one is dead-but-buggy code, one is a missing FIDE draw rule). Several MEDIUM and LOW items are quality issues or invariants worth documenting.

| Severity | Count | Fixed in this PR |
|---|---|---|
| Critical | 0 | — |
| High | 2 | **2 (H1, H2)** |
| Medium | 5 | 3 (M1 assert, M3, M4 comment) |
| Low | 4 | 0 (all out of scope — see below) |

**Resolved here:** H1, H2, M1, M3, M4. Open follow-ups: M2 (promotion UI, Phase 5), M5 (refactor — explicitly declined), L1–L4 (intentional / out-of-phase / replaced by art work).

---

## HIGH

### H1 — `King.GetValidMoves` runs a buggy attack-filter that is redundant with `MoveValidator` ✅ FIXED

**File:** `CHESS/Assets/Scripts/Core/Pieces/King.cs:24`

```csharp
if (AttackChecker.IsAttackedBy(board, f, r, Opponent())) continue;
```

**What's wrong.** This call is performed with the King still on its original square. The King itself blocks slider rays. A destination square `(f, r)` may be reported NOT attacked because the King at its current position blocks the attacker's ray — even though the King would obviously not be there after moving. Classic "king moves backward along a slider ray" case:

> White King on (4, 4). Black Queen on (4, 0). Queen rays up file 4: hits King at (4, 4). King is in check. King "moves" to (4, 5). From (4, 5) `IsAttackedBy` casts down file 4 → (4, 4) is still occupied by the White King → ray stops, queen not detected. The check reports (4, 5) as safe, even though once the king moves the queen would reach it.

**Why it's not a playable bug today.** `MoveValidator.GetLegalMoves` (lines 22-32) always re-runs `Simulate` + `IsKingInCheck` on every pseudo-legal move. The simulated board moves the King out of the way, so the queen IS detected, and the illegal King move IS filtered out. The buggy check in `King.cs:24` is **dead defensive code that produces wrong answers but is masked downstream**.

**Other consumers of `King.GetValidMoves` that DO NOT go through `MoveValidator`:**
- `GameManager.FindCheckingPieces` (line 247) — iterates every opponent piece's `GetValidMoves` to find checkers for the red/orange highlight. Doesn't matter in practice (a King can never check another King — they can't be adjacent in any legal position).
- `AIPerformanceLogger` profiler markers — irrelevant.
- The AI search loop — assumed to filter for legality before playing; not verified in this audit (`ChessAI` was out of scope).

**Risk if left.** Any future code path that calls `Piece.GetValidMoves` directly and *skips* `MoveValidator` would silently allow illegal King moves into check. The bug is a trap.

**Recommended fix.** Delete `King.cs:24` (the `IsAttackedBy` call). Make `King.GetValidMoves` consistent with every other piece — return pseudo-legal moves only, defer to `MoveValidator` for legality. Side benefit: removes ~8 `IsAttackedBy` calls per King move during AI search.

```csharp
// BEFORE:
if (board[f, r].IsOccupiedByColor(Color)) continue;
if (AttackChecker.IsAttackedBy(board, f, r, Opponent())) continue;
moves.Add(new Vector2Int(f, r));

// AFTER:
if (board[f, r].IsOccupiedByColor(Color)) continue;
moves.Add(new Vector2Int(f, r));
```

`AddCastlingMoves` is unchanged — its `IsAttackedBy` calls are correct (see § Notes-on-castling below).

---

### H2 — Missing FIDE draw rules: insufficient material / 50-move / threefold repetition ✅ FIXED

**Implementation summary (in this PR):**

1. **Insufficient material** — `MoveValidator.IsInsufficientMaterial(board)`. Detects K vs K, K+(B or N) vs K, and K+B vs K+B with same-coloured bishops. Two-knights-vs-K is intentionally NOT auto-drawn (mate is technically possible under FIDE).
2. **50-move rule** — `BoardManager._halfMoveClock`. Reset to 0 on any pawn move or capture (regular OR en passant). Incremented otherwise. `BoardManager.HalfMoveClock` exposed for GameManager. Threshold: 100 half-moves.
3. **Threefold repetition** — `BoardManager.GetPositionKey(nextMover)` returns a FEN-style fingerprint: piece placement + side to move + castling rights + EP target. `GameManager` keeps a `Dictionary<string,int>` count keyed on this; starting position is seeded with count 1 in `Start()`; game ends when any entry hits 3.
4. **Wired into `GameManager.SwitchTurn`** after the checkmate/stalemate check, before fire-event-and-continue. Extracted shared `EndGame(winner, reason)` helper so all five end-states (checkmate, stalemate, insufficient material, 50-move, threefold) share a single code path. Logs include the reason.

**Testing burden on user.** No Unity test was run by the agent (cleanup-only PR). Verification suggestions:
- Open Unity → Chess ▶ Validate AI ▶ Run Self-Play Test (9 games). Should pass; should also terminate within the existing time bounds (previously, dead-position endgames could in theory loop forever).
- Manual: play out an obvious K vs K position; the game should end with "Insufficient material — Draw" in the Console.

**Files:** `MoveValidator.cs`, `BoardManager.cs`, `GameManager.SwitchTurn`

**What's missing.** Game-end detection covers checkmate and stalemate only (`GameManager.cs:111-127`). The following standard FIDE draws are NOT enforced:

| Rule | Condition | Where to add |
|---|---|---|
| **Insufficient material** | K vs K, K+B vs K, K+N vs K, K+B vs K+B with same-coloured bishops | Helper in `MoveValidator`; check after every move in `GameManager.SwitchTurn` |
| **Fifty-move rule** | 50 half-moves without a pawn move or capture → claimable draw | `BoardManager.ExecuteMove` increments / resets a half-move clock; `GameManager` reads it |
| **Threefold repetition** | Same position reached 3 times → claimable draw | `BoardManager` maintains a position-key history (Zobrist hash works) keyed at `ExecuteMove`; check after each move |

**Impact.** AI vs AI in dead positions never terminates. A late-endgame K+N vs K with the player vs. Hard AI can loop indefinitely; players will perceive it as a frozen app. Real concern for Phase 8 QA — `AIGameValidator`'s 9-game self-play could in principle never finish (current cap is presumably depth-2 with random tie-breaks, so unlikely, but still a latent risk).

**Recommended fix.** Open a dedicated issue under Phase 8 (`[P8-T0X] Implement FIDE draw rules`). Could be done now but it's cross-cutting (touches `BoardManager`, `MoveValidator`, `GameManager`, and benefits from Zobrist via the AI's `ZobristTable`). Note: insufficient-material alone is cheap and high-value — could be a smaller, earlier issue.

---

## MEDIUM

### M1 — `MoveSelector.ExecuteMove` doesn't re-validate moves before applying ✅ FIXED

**File:** `CHESS/Assets/Scripts/Input/MoveSelector.cs:74` (and `SubmitMove`:134)

`HandleSquareSelected` only calls `ExecuteMove` when `_validMoves.Contains(coord)` — that's the player gate. But `SubmitMove(from, to)` (the AI entry point) calls `ExecuteMove` directly with no re-validation. Per CLAUDE.md the AI is contracted never to return an illegal move, but the contract is not enforced.

**Risk.** A regression in `ChessAI` (or a future agent's `IChessAI` impl) could return an illegal move and corrupt the board silently.

**Fix.** Add a `Debug.Assert` (Editor-only cost):

```csharp
public void SubmitMove(Vector2Int from, Vector2Int to)
{
    var board = _boardManager.GetBoard();
    var piece = board[from.x, from.y].Piece;
    Debug.Assert(piece != null && piece.Color == CurrentPlayer,
        $"SubmitMove: no own piece at {from}");
    Debug.Assert(GetLegalMoves(piece, board).Contains(to),
        $"SubmitMove: illegal move {from}→{to}");
    ExecuteMove(from, to);
}
```

---

### M2 — Promotion hard-coded to Queen (no underpromotion UI)

**File:** `MoveSelector.cs:99`

```csharp
_boardManager.PromotePawn(to, PieceType.Queen);
```

Documented in CLAUDE.md as the Phase 5 UI extension point. Affects rare endgame correctness (Q-promotion can stalemate where N-promotion forks/wins, etc.). Intentional for Phase 1 — log this as a Phase 5 acceptance criterion on issue #58 / new issue.

---

### M3 — Pawn double-step uses `!HasMoved` instead of starting-rank check ✅ FIXED

**File:** `Pawn.cs:27`

```csharp
if (!HasMoved && IsInBounds(file, fwd2) && !board[file, fwd2].IsOccupied)
```

Today this is equivalent to "pawn on starting rank" because pawns never move backward and `HasMoved` is set on every move + `BoardManager.PromotePawn` sets `HasMoved = true` on the promoted piece. **Invariant-safe.**

**Latent risk.** If anyone ever resets `HasMoved = false` on a pawn that's off its starting rank (e.g., a future "undo" feature, AI search code that incorrectly clones state), double-step from a non-starting rank becomes legal.

**Fix.** Belt-and-suspenders — tighten the check:

```csharp
int startRank = Color == PieceColor.White ? 1 : 6;
if (rank == startRank && IsInBounds(file, fwd2) && !board[file, fwd2].IsOccupied)
```

---

### M4 — `MoveValidator.Simulate` shallow-clones the board (Piece refs shared) ✅ FIXED (doc-only)

**File:** `MoveValidator.cs:49` `(Square[,])board.Clone()`

`Square` is a struct (cloned by value), but `Piece` is a class — the simulated board shares the same `Piece` objects as the live board. **Safe today** because `Simulate` only nulls `Square.Piece` and re-assigns references; it never mutates `Piece.Position` or `Piece.HasMoved`.

Contrast: `ChessAI.CloneBoard` *does* deep-clone Pieces because the search applies moves recursively and mutates piece state. CLAUDE.md (line 167) documents this requirement for AI; the same warning is not present for `MoveValidator`.

**Fix.** Add a comment on `Simulate` explicitly forbidding piece-state mutation, so a future agent doesn't inline a `piece.Position = to` line "to keep things consistent" and silently corrupt the live board.

---

### M5 — `BoardManager` has no public API to detect en-passant target externally

**File:** `BoardManager.cs:9` (`private Vector2Int? _enPassantTarget`)

Other code (e.g. AI move generation in `ChessAI`, hypothetically the UI) reads EP state by inspecting `Square.IsEnPassantTarget`. That works, but the canonical source is the private field — duplicated state, lock-step risk. Worth folding into a single source eventually.

**Fix.** Optional refactor — make `_enPassantTarget` a getter, or remove it entirely and derive from the square flags. Low priority.

---

## LOW

### L1 — `AttackChecker` slider arrays not derived from `BoardConstants`

**File:** `AttackChecker.cs:51, 73`

Hard-coded `{ 1, -1, 0, 0 }` literals — fine because chess geometry isn't going to change. Just an aesthetic consistency note.

### L2 — `BoardVisualizer.SpawnPiece` uses primitive Cubes

**File:** `BoardVisualizer.cs:64`

`GameObject.CreatePrimitive(PrimitiveType.Cube)` — Phase 1 placeholder. Phase 3 art will replace this with the realistic 3D piece prefabs (issues #37, #38, etc.). Already tracked; noted for completeness.

### L3 — `BoardVisualizer.MovePieceView` rebuilds the name string every time

**File:** `BoardVisualizer.cs:96`

Necessary (per the `TileSelector.TryParseSquare` convention noted in CLAUDE.md). Coupling is tight; one stray `Destroy → re-instantiate` path that forgets to set the name would break click-input on a moved piece. Already documented in CLAUDE.md line 130; just flagging the fragility.

### L4 — `GameManager.ShowCheckVisual` uses `material.color`, which creates a per-renderer material instance

**File:** `GameManager.cs:217`

Accessing `MeshRenderer.material` instantiates a unique material for the GameObject — breaks SRP batching for those tiles until the check resolves. Minor perf (2-3 tiles affected, brief duration). Phase 7 optimization candidate.

---

## Notes on castling (verified correct, despite looking suspicious)

I initially suspected `King.AddCastlingMoves`'s `IsAttackedBy` calls on path squares (`5,0`, `6,0` etc.) had the same king-blocks-slider bug as H1. They don't, for two reasons:

1. **Same-rank sliders** behind the king (file 0-3) that could attack a path square through the king at file 4 would ALSO be attacking the king directly at file 4 → king is in check → the first check at line 39 (`AttackedBy(Position.x, rank, opp)`) returns true → castling correctly rejected.
2. **Diagonal sliders** cannot pass through `(4, 0)` to reach `(5, 0)`, `(6, 0)`, `(2, 0)`, or `(3, 0)` — those aren't on a diagonal through file 4.

So path-square attack detection is correct even with the king still on its original square. Don't "fix" `AddCastlingMoves`; just kill the redundant filter in `GetValidMoves` (H1).

`MoveValidator.Simulate` correctly relocates the rook during castling simulation (lines 62-73), so check-after-castle detection works.

---

## Notes on en passant (verified correct)

- `BoardManager.ExecuteMove` correctly clears the previous EP target before applying the new move (lines 102-108) — no stale EP flags across turns. ✓
- `Pawn.GetValidMoves` reads `IsEnPassantTarget` directly from the board — no signature change to `GetValidMoves` needed, per CLAUDE.md line 128. ✓
- `MoveValidator.Simulate` correctly removes the bypassed pawn before running check detection (lines 53-56). ✓
- The captured pawn at `(to.x, from.y)` is guaranteed to be an opponent pawn because the EP target is set ONLY in the immediate next ply after a double-step, and cleared at the start of every other move. ✓

---

## Recommended action order

1. ✅ **H1** (`King.cs:24` filter removed) — **done in this PR.**
2. ✅ **H2** (all three FIDE draws) — **done in this PR.**
3. ✅ **M1** (assert in `SubmitMove`) — **done in this PR.**
4. ✅ **M3** (Pawn double-step starting-rank check) — **done in this PR.**
5. ✅ **M4** (`Simulate` mutation-warning comment) — **done in this PR.**
6. ⏳ **M2** (promotion-choice UI for under-promotion) — Phase 5 work; open as new issue or fold into #58.
7. **Explicitly declined / out of scope:**
   - **M5** (redundant EP state) — refactor risk > value; live with the two-source coupling.
   - **L1** (`AttackChecker` direction-count literals) — aesthetic; chess geometry is fixed.
   - **L2** (Cube placeholder pieces) — replaced naturally by Phase-3 piece prefabs (#37, #38, …).
   - **L3** (`MovePieceView` name-string rebuild) — architectural coupling, documented in CLAUDE.md.
   - **L4** (`material.color` SRP-batching break) — Phase-7 perf work; premature.

This PR bundles the audit document with the H1+H2+M1+M3+M4 fixes — the audit explains "why", the fixes are the "what". Item-by-item verification trace in each finding above.

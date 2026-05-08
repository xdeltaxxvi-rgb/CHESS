using System;
using UnityEngine;
using Chess.Core.Pieces;
using Chess.Input;

namespace Chess.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private MoveSelector _moveSelector;

        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

        // HUDManager subscribes to this to update the turn indicator.
        public event Action<PieceColor> OnTurnChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _moveSelector.CurrentPlayer = CurrentTurn;
        }

        private void OnEnable() => _moveSelector.OnMoveExecuted += SwitchTurn;
        private void OnDisable() => _moveSelector.OnMoveExecuted -= SwitchTurn;

        private void SwitchTurn()
        {
            CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            _moveSelector.CurrentPlayer = CurrentTurn;
            OnTurnChanged?.Invoke(CurrentTurn);
        }
    }
}

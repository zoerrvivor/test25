using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Test25.Gameplay.Entities;
using Test25.Utilities;
using Test25;

namespace Test25.Gameplay.Managers
{
    public class TurnManager
    {
        public int CurrentPlayerIndex { get; private set; }
        public float Wind { get; private set; }
        public MatchSettings Settings { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsMatchOver { get; private set; }
        public string GameOverMessage { get; private set; }
        public int CurrentRound { get; private set; }
        public int TotalRounds { get; private set; }

        public TurnManager()
        {
            CurrentPlayerIndex = 0;
            Wind = 0;
        }

        public void StartGame(MatchSettings settings)
        {
            Settings = settings;
            TotalRounds = settings.NumRounds;
            CurrentRound = 1;
            ResetState();
        }

        public void StartNextRound()
        {
            CurrentRound++;
            ResetState();
        }

        private void ResetState()
        {
            IsGameOver = false;
            IsMatchOver = false;
            CurrentPlayerIndex = -1;
            Wind = 0;
            GameOverMessage = "";
        }

        public bool NextTurn(List<Tank> players)
        {
            if (IsGameOver) return false;

            // Find next active player
            int attempts = 0;
            int nextIndex = CurrentPlayerIndex;
            do
            {
                nextIndex = (nextIndex + 1) % players.Count;
                attempts++;
            } while (!players[nextIndex].IsActive && attempts < players.Count);

            CurrentPlayerIndex = nextIndex;

            // If we looped through everyone and found no one active, the game should be over via CheckWinCondition
            if (!players[CurrentPlayerIndex].IsActive) return false;

            Wind = (float)(Rng.Instance.NextDouble() * 20 - 10);

            return true;
        }

        public void CheckWinCondition(List<Tank> players)
        {
            if (IsGameOver) return;
            int activeCount = 0;
            Tank lastSurvivor = null;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].IsActive)
                {
                    activeCount++;
                    lastSurvivor = players[i];
                }
            }

            if (activeCount <= 1)
            {
                IsGameOver = true;
                if (lastSurvivor != null)
                {
                    GameOverMessage = $"{lastSurvivor.Name} Wins Round {CurrentRound}!";
                    lastSurvivor.Score++;
                    lastSurvivor.Money += 500;
                }
                else
                {
                    GameOverMessage = "Draw!";
                }

                // Participation award
                for (int i = 0; i < players.Count; i++) players[i].Money += 100;

                if (CurrentRound >= TotalRounds)
                {
                    IsMatchOver = true;
                    GameOverMessage += "\nMATCH OVER!";
                }
            }
        }
    }
}

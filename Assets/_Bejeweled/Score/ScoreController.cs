using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    public class ScoreController : MonoBehaviour
    {
        private const string SCORECONTROLLER_DEBUG_PREFIX = "<b>ScoreController:</b>";

        [Header("Components")]
        [SerializeField]
        private Table table;

        [Header("Values")]
        [SerializeField]
        private int scoreForTile;
        [SerializeField]
        private int comboMultiplier;

        private int score;
        public int Score
        {
            get => score;
        }

        public event EventHandler<ScoreUpdateArgs> OnScoreUpdated;

        private void Start()
        {
            table.OnTileMatch += OnTileMatch;
        }

        private void OnTileMatch(object sender, Table.TileMatchArgs e)
        {
            int difference = (e.TileMatch.MatchSize * scoreForTile) * (1 + ((e.Combo - 1) * comboMultiplier));

            score += difference;

            Debug.Log($"{SCORECONTROLLER_DEBUG_PREFIX} Scored {difference}, Combo: {e.Combo}, Current Score: {score}");

            OnScoreUpdated?.Invoke(this, new ScoreUpdateArgs(difference, e.Combo, score, e.TileMatch));
        }

        public class ScoreUpdateArgs : EventArgs
        {
            public readonly int ScoreDifference;
            public readonly int ComboCount;
            public readonly int CurrentScore;
            public readonly TileMatch TileMatch;

            public ScoreUpdateArgs(int scoreDifference, int comboCount, int currentScore, TileMatch tileMatch)
            {
                ScoreDifference = scoreDifference;
                ComboCount = comboCount;
                CurrentScore = currentScore;
                TileMatch = tileMatch;
            }
        }
    }
}
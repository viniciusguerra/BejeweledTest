using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bejeweled
{
    public class ScorePanel : MonoBehaviour
    {
        [SerializeField]
        private ScoreController scoreController;
        [SerializeField]
        private TextMeshProUGUI scoreText;

        private void Start()
        {
            scoreController.OnScoreUpdated += OnScoreUpdated;
        }

        private void OnScoreUpdated(object sender, ScoreController.ScoreUpdateArgs e)
        {
            scoreText.text = e.CurrentScore.ToString();
        }
    }
}
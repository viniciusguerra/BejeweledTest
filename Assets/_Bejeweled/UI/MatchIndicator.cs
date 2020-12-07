using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bejeweled
{
    public class MatchIndicator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private ScoreController scoreController;
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private TextMeshProUGUI scoreText;

        [Header("Values")]
        [SerializeField]
        private AnimationCurve scaleEase;
        [SerializeField]
        private float displayTime;

        private Stack<ScoreController.ScoreUpdateArgs> scoreUpdateStack;

        private Coroutine displayCoroutine;

        private void Start()
        {
            scoreUpdateStack = new Stack<ScoreController.ScoreUpdateArgs>();

            scoreController.OnScoreUpdated += OnScoreUpdated;
        }

        private void OnScoreUpdated(object sender, ScoreController.ScoreUpdateArgs e)
        {
            scoreUpdateStack.Push(e);

            if (displayCoroutine == null)
                displayCoroutine = StartCoroutine(DisplayCoroutine());
        }

        private IEnumerator DisplayCoroutine()
        {
            do
            {
                var update = scoreUpdateStack.Pop();

                transform.position = update.TileMatch.Position;

                scoreText.text = update.ScoreDifference.ToString();

                canvasGroup.alpha = 1;

                float counter = 0;

                // scale MatchIndicator
                do
                {
                    transform.localScale = Vector3.one * scaleEase.Evaluate(counter / displayTime);

                    counter += Time.deltaTime;

                    yield return null;

                } while (counter < displayTime);

                transform.localScale = Vector3.one;

                counter = 0;

                // fade MatchIndicator
                do
                {
                    canvasGroup.alpha = Mathf.Lerp(1, 0, counter / displayTime);

                    counter += Time.deltaTime;

                    yield return null;

                } while (counter < displayTime);

                canvasGroup.alpha = 0;

            } while (scoreUpdateStack.Count > 0);

            displayCoroutine = null;
        }
    }
}
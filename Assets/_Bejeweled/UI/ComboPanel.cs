using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bejeweled
{
    public class ComboPanel : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private ScoreController scoreController;
        [SerializeField]
        private TextMeshProUGUI comboText;

        [Header("Values")]
        [SerializeField]
        private float hideDelay;
        [SerializeField]
        private float hideTime;

        private void Start()
        {
            scoreController.OnScoreUpdated += OnScoreUpdate;
        }

        private Coroutine hideCoroutine;

        private void OnScoreUpdate(object sender, ScoreController.ScoreUpdateArgs e)
        {
            if (e.ComboCount > 1)
            {
                comboText.alpha = 1;

                comboText.text = $"{e.ComboCount}x Combo!";

                if(hideCoroutine != null)
                    StopCoroutine(hideCoroutine);

                hideCoroutine = StartCoroutine(HideCoroutine());
            }
        }

        private IEnumerator HideCoroutine()
        {
            yield return new WaitForSeconds(hideDelay);

            float counter = 0;

            do
            {
                comboText.alpha = Mathf.Lerp(1, 0, counter / hideTime);

                counter += Time.deltaTime;

                yield return null;

            } while (counter < hideTime);

            comboText.alpha = 0;
        }
    }
}
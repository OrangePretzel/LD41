using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LD41
{
	public class ScoreThings : MonoBehaviour
	{
		public Image BGGround;
		public Text ScoreText;

		public void UpdateScore(int score)
		{
			ScoreText.text = $"{score}";
		}

		public void Win()
		{
			StartCoroutine(FlashAnim(Color.green, 3));
		}

		public void Lose()
		{
			StartCoroutine(FlashAnim(Color.red, 3));
		}

		public void ScoreFlash()
		{
			StartCoroutine(FlashAnim(Color.yellow, 1));
		}

		public IEnumerator FlashAnim(Color color, int repeats)
		{
			var ogColor = BGGround.color;

			for (int i = 0; i < repeats; i++)
			{
				BGGround.color = color;
				yield return new WaitForSecondsRealtime(0.25f);
				BGGround.color = ogColor;
				yield return new WaitForSecondsRealtime(0.25f);
			}
		}
	}
}

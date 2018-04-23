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
			StartCoroutine(WinAnim());
		}

		public IEnumerator WinAnim()
		{
			BGGround.color = Color.green;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.white;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.green;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.white;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.green;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.white;
			yield return new WaitForSecondsRealtime(0.25f);
		}

		public void Lose()
		{
			StartCoroutine(LoseAnim());
		}

		public IEnumerator LoseAnim()
		{
			BGGround.color = Color.red;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.white;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.red;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.white;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.red;
			yield return new WaitForSecondsRealtime(0.25f);
			BGGround.color = Color.white;
			yield return new WaitForSecondsRealtime(0.25f);
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

namespace LD41
{
	public class PlayerIcon : MonoBehaviour
	{
		public SpriteRenderer MyImage;

		[SerializeField]
		private Color OGColor;
		[SerializeField]
		private Vector3 targetPos = Vector3.zero;

		private void Awake()
		{
			OGColor = MyImage.color;
		}

		public bool Enab = false;
		public int TeamID = -1;
		public int PlayerID = 0;

		public void SetStuff(int playerID, int teamID, bool enabled)
		{
			Enab = enabled;
			TeamID = teamID;
			PlayerID = playerID;

			MyImage.color = Enab ? OGColor : OGColor * 0.75f;
			MyImage.transform.localScale = Enab ? Vector3.one : new Vector3(0.5f, 0.5f, 1f);
			if (TeamID == -1)
				targetPos = Vector3.zero;
			else
				targetPos = GameManager.Instance.PIconTeamLocations[TeamID];

			var diff = targetPos - transform.position;
			if (diff.sqrMagnitude > 4)
				transform.position += diff * Time.unscaledDeltaTime * 10;
			else
				transform.position = targetPos;
		}

		private void LateUpdate()
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, 0);
		}
	}
}

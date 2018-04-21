using UnityEngine;

namespace LD41
{
	[RequireComponent(typeof(Camera))]
	public class MetroidCamera : MonoBehaviour
	{
		private Camera _camera;

		private void OnEnable()
		{
			_camera = GetComponent<Camera>();
		}

		public void TransitionToRoom(Room room)
		{
			transform.position = room.transform.position + new Vector3(20 * CONST.PIXELS_PER_UNIT / 4, 16 * CONST.PIXELS_PER_UNIT / 4, transform.position.z);
		}
	}
}

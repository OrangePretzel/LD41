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
			transform.position = room.transform.position + new Vector3(room.LevelTileMap.Width * CONST.PIXELS_PER_UNIT / 4, room.LevelTileMap.Height * CONST.PIXELS_PER_UNIT / 4, transform.position.z);
			_camera.orthographicSize = room.LevelTileMap.Width / 4 * CONST.PIXELS_PER_UNIT;
		}
	}
}

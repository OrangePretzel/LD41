using LD41.Tilemaps;
using UnityEngine;

namespace LD41
{
	public class Room : MonoBehaviour, IPoolableObject
	{
		[SerializeField]
		public Vector2Int?[] DoorPositions = new Vector2Int?[4];
		public LevelTileMap LevelTileMap;

		public DetailedRoomInfo DetailedRoomInfo;

		private void OnEnable()
		{
			LevelTileMap = GetComponentInChildren<LevelTileMap>();
		}

		public void ActivateFromPool()
		{
			gameObject.SetActive(true);
		}

		public void ReturnToPool()
		{
			gameObject.SetActive(false);
		}

		public void Init(int width, int height)
		{
			LevelTileMap.Init();
			LevelTileMap.gameObject.SetActive(false);
		}

		public void DeactivateRoom()
		{
			//LevelTileMap.gameObject.SetActive(false);
		}

		public void ActivateRoom()
		{
			if (DetailedRoomInfo == null) throw new System.Exception("blargons are not set");
			//LevelTileMap.gameObject.SetActive(true);
			LevelTileMap.Render(DetailedRoomInfo.RoomLayout);
		}

		public void SetRoomInfo(DetailedRoomInfo roomInfo) => DetailedRoomInfo = roomInfo;
	}
}

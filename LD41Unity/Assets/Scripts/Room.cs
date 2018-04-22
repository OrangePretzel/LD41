using LD41.Tilemaps;
using UnityEngine;

namespace LD41
{
	public class Room : MonoBehaviour, IPoolableObject
	{
		[SerializeField]
		public Door[] Doors = new Door[4];
		public LevelTileMap LevelTileMap;

		public DetailedRoomInfo DetailedRoomInfo;

		public Vector3 RoomCenter => transform.position + new Vector3(20 * CONST.PIXELS_PER_UNIT / 4, 16 * CONST.PIXELS_PER_UNIT / 4, 0);

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
			gameObject.SetActive(false);
		}

		public void DeactivateRoom()
		{
			gameObject.SetActive(false);
			//LevelTileMap.gameObject.SetActive(false);
		}

		public void ActivateRoom()
		{
			if (DetailedRoomInfo == null) throw new System.Exception("blargons are not set");

			gameObject.SetActive(true);
			//LevelTileMap.gameObject.SetActive(true);
			LevelTileMap.Render(DetailedRoomInfo.RoomLayout);

			//if (DetailedRoomInfo.RoomInfo.RoomType == RoomInfo.RoomTypes.Start)
			//	Unlock();
			//else
				Lock();
		}

		public void Lock()
		{
			foreach (var door in Doors)
				door.CloseDoor();
		}

		public void Unlock()
		{
			if (DetailedRoomInfo.RoomInfo.HasConnectionRight) Doors[0]?.OpenDoor();
			else Doors[0]?.CloseDoor();
			if (DetailedRoomInfo.RoomInfo.HasConnectionTop) Doors[1]?.OpenDoor();
			else Doors[1]?.CloseDoor();
			if (DetailedRoomInfo.RoomInfo.HasConnectionLeft) Doors[2]?.OpenDoor();
			else Doors[2]?.CloseDoor();
			if (DetailedRoomInfo.RoomInfo.HasConnectionDown) Doors[3]?.OpenDoor();
			else Doors[3]?.CloseDoor();
		}

		public void SetRoomInfo(DetailedRoomInfo roomInfo)
		{
			DetailedRoomInfo = roomInfo;
			transform.position = new Vector3(roomInfo.RoomInfo.Position.x * 21 * CONST.PIXELS_PER_UNIT, roomInfo.RoomInfo.Position.y * 17 * CONST.PIXELS_PER_UNIT, 0);
		}
	}
}

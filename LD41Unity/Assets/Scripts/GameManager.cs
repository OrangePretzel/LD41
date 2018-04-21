using LD41.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LD41
{
	public class GameManager : MonoBehaviour
	{
		#region Singleton

		private static GameManager instance;

		private void MakeSingleton()
		{
			if (instance != null && instance != this)
			{
				Destroy(this);
			}

			instance = this;
		}

		#endregion

		// If true we are paused
		public bool IsPaused { get; private set; }
		public static bool IsGamePaused => instance.IsPaused;

		public Object RoomPrefab;

		private MetroidCamera _camera;

		private List<RoomInfo> _roomInfos = new List<RoomInfo>();

		private ObjectPool _roomPool;
		private List<Room> _roomsObjects = new List<Room>();
		private Room _currentRoom;

		private void Awake()
		{
			MakeSingleton();
		}

		private void OnEnable()
		{
			_camera = FindObjectOfType<MetroidCamera>();

			InitializeObjectPools();
			ResetGame();
			NewLevel();
		}

		private void Start()
		{
			LoadRoom(_roomsObjects.First(r => r.DetailedRoomInfo.RoomInfo.RoomType == RoomInfo.RoomTypes.Start));
		}

		private void InitializeObjectPools()
		{
			var particleObjectPool = new GameObject("Room Object Pool");
			_roomPool = particleObjectPool.AddComponent<ObjectPool>();
			_roomPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(RoomPrefab, _roomPool.transform);
				gObj.name = "Room Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				poolable.ReturnToPool();
				return poolable;
			});
			_roomPool.AllocateObjects(14);
		}

		public void ResetGame()
		{
			foreach (var poolable in _roomsObjects)
				_roomPool.ReturnObjectToPool(poolable);
			_roomsObjects.Clear();
			_currentRoom = null;
		}

		public void NewLevel()
		{
			_roomInfos = LevelGenerator.GenerateLevel();

			foreach (var roomInfo in _roomInfos)
			{
				var roomObj = (Room)_roomPool.GetObjectFromPool();
				var detailedRoomInfo = RoomGenerator.GenerateRoom(roomInfo);
				roomObj.SetRoomInfo(detailedRoomInfo);
				_roomsObjects.Add(roomObj);
			}
		}

		public void LoadRoom(Room room)
		{
			_currentRoom?.DeactivateRoom();
			_currentRoom = room;
			_currentRoom?.ActivateRoom();
			_camera.TransitionToRoom(room);
		}
	}
}

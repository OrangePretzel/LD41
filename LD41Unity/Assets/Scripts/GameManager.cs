using LD41.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LD41
{
	public class GameManager : MonoBehaviour
	{
		public Object RoomPrefab;

		private MetroidCamera _camera;

		private ObjectPool _roomPool;
		private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>();
		private Room _currentRoom;

		private void OnEnable()
		{
			_camera = FindObjectOfType<MetroidCamera>();

			InitializeObjectPools();
			Reset();
			NewLevel();
			GotoRoom(_rooms[Vector2Int.zero]);

			foreach (var room in _rooms)
			{
				// TODO: Remove this, just for testing
				room.Value.LevelTileMap.Render();
			}
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

		public void Reset()
		{
			foreach (var poolable in _rooms)
				_roomPool.ReturnObjectToPool(poolable.Value);
			_rooms.Clear();
			_currentRoom = null;
		}

		public void NewLevel()
		{
			_rooms = GenerateRooms();
		}

		public void GotoRoom(Room room)
		{
			_currentRoom?.DeactivateRoom();
			_currentRoom = room;
			_currentRoom?.ActivateRoom();
			_camera.TransitionToRoom(room);
		}

		private Dictionary<Vector2Int, Room> GenerateRooms()
		{
			string log = "Room Creation Log:\n";
			Random.InitState(1370);

			var roomPositions = new List<Vector2Int>();
			var roomConnections = new List<System.Tuple<Vector2Int, Vector2Int>>();
			int targetRoomCount = Random.Range(8, 14);

			int rX = 0;
			int rY = 0;
			roomPositions.Add(new Vector2Int(rX, rY));

			while (roomPositions.Count < targetRoomCount)
			{
				var randRoom = roomPositions[Random.Range(0, roomPositions.Count)];
				var randDir = Random.Range(0, 4);
				switch (randDir)
				{
					case 0: // right
						rX = randRoom.x + 1;
						rY = randRoom.y;
						if (roomPositions.Any(r => r.y == rY && r.x == rX)) continue;
						break;
					case 1: // top
						rX = randRoom.x;
						rY = randRoom.y + 1;
						if (roomPositions.Any(r => r.y == rY && r.x == rX)) continue;
						break;
					case 2: // left
						rX = randRoom.x - 1;
						rY = randRoom.y;
						if (roomPositions.Any(r => r.y == rY && r.x == rX)) continue;
						break;
					case 3: // bottom
						rX = randRoom.x;
						rY = randRoom.y - 1;
						if (roomPositions.Any(r => r.y == rY && r.x == rX)) continue;
						break;
				}

				roomPositions.Add(new Vector2Int(rX, rY));

				if (roomPositions.Contains(new Vector2Int(rX + 1, rY))) // right
					roomConnections.Add(new System.Tuple<Vector2Int, Vector2Int>(new Vector2Int(rX, rY), new Vector2Int(rX + 1, rY)));
				if (roomPositions.Contains(new Vector2Int(rX, rY + 1))) // top
					roomConnections.Add(new System.Tuple<Vector2Int, Vector2Int>(new Vector2Int(rX, rY), new Vector2Int(rX, rY + 1)));
				if (roomPositions.Contains(new Vector2Int(rX - 1, rY))) // left
					roomConnections.Add(new System.Tuple<Vector2Int, Vector2Int>(new Vector2Int(rX, rY), new Vector2Int(rX - 1, rY)));
				if (roomPositions.Contains(new Vector2Int(rX, rY - 1))) // bottom
					roomConnections.Add(new System.Tuple<Vector2Int, Vector2Int>(new Vector2Int(rX, rY), new Vector2Int(rX, rY - 1)));
			}

			var rooms = new Dictionary<Vector2Int, Room>();
			for (int roomIndex = 0; roomIndex < roomPositions.Count; roomIndex++)
			{
				var roomPos = roomPositions[roomIndex];
				var room = GenerateRoom();
				room.name = $"Room {roomPos}";
				rooms.Add(roomPos, room);
				log += $"Added room at {roomPos}\n";
			}

			foreach (var connection in roomConnections)
			{
				var room1 = rooms[connection.Item1];
				var room2 = rooms[connection.Item2];
				var dir = connection.Item2 - connection.Item1;

				Vector2Int room1Door, room2Door;
				if (dir.x == 1)
				{
					log += $"Connection (right) made between {connection.Item1} and {connection.Item2}\n";

					room1Door = new Vector2Int(room1.LevelTileMap.Width - 1, Random.Range(1, room1.LevelTileMap.Height - 2));
					room1.DoorPositions[0] = room1Door;

					room2Door = new Vector2Int(0, Random.Range(1, room2.LevelTileMap.Height - 2));
					room2.DoorPositions[2] = room2Door;
				}
				else if (dir.x == -1)
				{
					log += $"Connection (left) made between {connection.Item1} and {connection.Item2}\n";

					room1Door = new Vector2Int(0, Random.Range(1, room1.LevelTileMap.Height - 2));
					room1.DoorPositions[2] = room1Door;

					room2Door = new Vector2Int(room2.LevelTileMap.Width - 1, Random.Range(1, room2.LevelTileMap.Height - 2));
					room2.DoorPositions[0] = room2Door;
				}
				else if (dir.y == 1)
				{
					log += $"Connection (up) made between {connection.Item1} and {connection.Item2}\n";

					room1Door = new Vector2Int(Random.Range(1, room1.LevelTileMap.Width - 2), room1.LevelTileMap.Height - 1);
					room1.DoorPositions[1] = room1Door;

					room2Door = new Vector2Int(Random.Range(1, room2.LevelTileMap.Width - 2), 0);
					room2.DoorPositions[3] = room2Door;
				}
				else
				{
					log += $"Connection (down) made between {connection.Item1} and {connection.Item2}\n";

					room1Door = new Vector2Int(Random.Range(1, room1.LevelTileMap.Width - 2), 0);
					room1.DoorPositions[3] = room1Door;

					room2Door = new Vector2Int(Random.Range(1, room2.LevelTileMap.Width - 2), room2.LevelTileMap.Height - 1);
					room2.DoorPositions[1] = room2Door;
				}
				room1.LevelTileMap.LevelTiles[room1Door.x, room1Door.y].Type = LevelTile.TileTypes.None;
				room2.LevelTileMap.LevelTiles[room2Door.x, room2Door.y].Type = LevelTile.TileTypes.None;
			}

			Debug.Log(log);
			return rooms;
		}

		private Room GenerateStartingRoom()
		{
			int rW = 12;
			int rH = 10;

			var room = (Room)_roomPool.GetObjectFromPool();
			room.Init(rW, rH);

			for (int j = 0; j < rH; j++)
				for (int i = 0; i < rW; i++)
				{
					if (i == 0 || j == 0 || i == rW - 1 || j == rH - 1)
						// Edge
						room.LevelTileMap.LevelTiles[i, j].Type = Tilemaps.LevelTile.TileTypes.Wall;
					else
						// Nothing
						room.LevelTileMap.LevelTiles[i, j].Type = Tilemaps.LevelTile.TileTypes.None;
				}

			return room;
		}

		private Room GenerateRoom()
		{
			const int MinRoomSize = 10;
			const int MaxRoomSize = 16;

			int rW = Random.Range(MinRoomSize, MaxRoomSize + 1);
			int rH = Random.Range(MinRoomSize, MaxRoomSize + 1);

			var room = (Room)_roomPool.GetObjectFromPool();
			room.Init(rW, rH);

			for (int j = 0; j < rH; j++)
				for (int i = 0; i < rW; i++)
				{
					if (i == 0 || j == 0 || i == rW - 1 || j == rH - 1)
						// Edge
						room.LevelTileMap.LevelTiles[i, j].Type = Tilemaps.LevelTile.TileTypes.Wall;
					else
						// Nothing
						room.LevelTileMap.LevelTiles[i, j].Type = Tilemaps.LevelTile.TileTypes.None;
				}

			return room;
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace LD41
{
	public class RoomInfo
	{
		public enum RoomTypes
		{
			Normal,
			Empty,
			Finish,
			Start
		}

		public RoomTypes RoomType;
		public Vector2Int Position;
		public bool HasConnectionRight;
		public bool HasConnectionTop;
		public bool HasConnectionLeft;
		public bool HasConnectionDown;
		public int DistFromStart;

		public RoomInfo(RoomTypes roomType, Vector2Int position)
		{
			RoomType = roomType;
			Position = position;
		}
	}

	public static class LevelGenerator
	{
		private const int MAX_SIZE = 6;
		private const int MIN_ROOMS = 8;
		private const int MAX_ROOMS = 16;

		private static Vector2Int[] POSSIBLE_DIRS = new Vector2Int[]
		{
			Vector2Int.right,
			Vector2Int.up,
			Vector2Int.left,
			Vector2Int.down
		};

		public static List<RoomInfo> GenerateLevel()
		{
			Random.InitState(1370);

			RoomInfo[,] roomLayout = new RoomInfo[MAX_SIZE, MAX_SIZE];
			List<RoomInfo> rooms = new List<RoomInfo>();

			System.Action<Vector2Int, RoomInfo> addRoom = (pos, room) =>
			{
				rooms.Add(room);
				roomLayout[pos.x, pos.y] = room;
			};

			System.Func<int, int, bool> isRoomPresent = (posx, posy) => IsInBounds(posx, posy) && roomLayout[posx, posy] != null;

			var startPos = GetRandVec2Int(0, MAX_SIZE, 0, MAX_SIZE);
			int roomCount = Random.Range(MIN_ROOMS, MAX_ROOMS);
			addRoom(startPos, new RoomInfo(RoomInfo.RoomTypes.Start, startPos));

			while (rooms.Count < roomCount)
			{
				var randRoom = rooms[Random.Range(0, rooms.Count)];
				var randDir = GetRandomDir();
				var targetPos = randRoom.Position + randDir;
				if (IsInBounds(targetPos.x, targetPos.y) && roomLayout[targetPos.x, targetPos.y] == null)
				{
					addRoom(targetPos, new RoomInfo(RoomInfo.RoomTypes.Normal, targetPos));
				}
			}

			foreach (var room in rooms)
			{
				room.HasConnectionRight = isRoomPresent(room.Position.x + 1, room.Position.y);
				room.HasConnectionTop = isRoomPresent(room.Position.x, room.Position.y + 1);
				room.HasConnectionLeft = isRoomPresent(room.Position.x - 1, room.Position.y);
				room.HasConnectionDown = isRoomPresent(room.Position.x, room.Position.y - 1);
			}

			// Calculate dists
			var frontier = new Queue<RoomInfo>();
			var startRoom = rooms[0];
			startRoom.DistFromStart = -1;
			frontier.Enqueue(startRoom);

			System.Action<RoomInfo, int> enqueNext = (potentialRoom, currDist) =>
			{
				if (potentialRoom.DistFromStart != 0) return;
				potentialRoom.DistFromStart = currDist + 1;
				frontier.Enqueue(potentialRoom);
			};

			RoomInfo curr = null;
			while (frontier.Count > 0)
			{
				curr = frontier.Dequeue();
				if (curr.HasConnectionRight) enqueNext(roomLayout[curr.Position.x + 1, curr.Position.y], curr.DistFromStart);
				if (curr.HasConnectionTop) enqueNext(roomLayout[curr.Position.x, curr.Position.y + 1], curr.DistFromStart);
				if (curr.HasConnectionLeft) enqueNext(roomLayout[curr.Position.x - 1, curr.Position.y], curr.DistFromStart);
				if (curr.HasConnectionDown) enqueNext(roomLayout[curr.Position.x, curr.Position.y - 1], curr.DistFromStart);
			}

			startRoom.DistFromStart = 0;
			curr.RoomType = RoomInfo.RoomTypes.Finish;

			return rooms;
		}

		private static bool IsInBounds(int x, int y) => x >= 0 && x < MAX_SIZE && y >= 0 && y < MAX_SIZE;

		private static Vector2Int GetRandomDir() => POSSIBLE_DIRS[Random.Range(0, POSSIBLE_DIRS.Length)];
		private static Vector2Int GetRandVec2Int(int xMin, int xMax, int yMin, int yMax) =>
			 new Vector2Int(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
	}

	public class LevelTile
	{
		public enum TileTypes
		{
			None,
			Wall,
			Door,
			TowerSpot
		}

		public TileTypes Type;

		public LevelTile(TileTypes type)
		{
			Type = type;
		}
	}

	public class DetailedRoomInfo
	{
		public RoomInfo RoomInfo;
		public LevelTile[,] RoomLayout;
		public List<Vector2Int> Spawns;

		public DetailedRoomInfo(RoomInfo roomInfo, int width, int height)
		{
			RoomInfo = roomInfo;
			RoomLayout = new LevelTile[width, height];
			Spawns = new List<Vector2Int>();
		}
	}

	public static class RoomGenerator
	{
		private const int ROOM_WIDTH = 20;
		private const int ROOM_HEIGHT = 16;

		private static bool IsInBounds(int x, int y) => x >= 0 && x < ROOM_WIDTH && y >= 0 && y < ROOM_HEIGHT;
		private static bool IsBoundary(int x, int y) => x == 0 || x == ROOM_WIDTH - 1 || y == 0 || y == ROOM_HEIGHT - 1;
		private static bool IsDoorPosition(int x, int y) => IsBoundary(x, y) && ((x == ROOM_WIDTH / 2 || x == ROOM_WIDTH / 2 - 1) || (y == ROOM_HEIGHT / 2 || y == ROOM_HEIGHT / 2 - 1));

		public static DetailedRoomInfo GenerateRoom(RoomInfo roomInfo, string mapName)
		{
			var detailedRoomInfo = new DetailedRoomInfo(roomInfo, ROOM_WIDTH, ROOM_HEIGHT);
			var template = ChooseRandomTemplate(mapName);

			for (int j = 0; j < ROOM_HEIGHT; j++)
				for (int i = 0; i < ROOM_WIDTH; i++)
				{
					var tile = new LevelTile(LevelTile.TileTypes.None);
					if (IsBoundary(i, j))
					{
						tile.Type = LevelTile.TileTypes.Wall;
					}
					else
					{
						var templateChar = template[j * ROOM_WIDTH + i];
						switch (templateChar)
						{
							case '@':
								detailedRoomInfo.Spawns.Add(new Vector2Int(i, j));
								break;
							case 'X':
								tile.Type = LevelTile.TileTypes.Wall;
								break;
							case '.':
								break;
							default:
								throw new System.Exception($"Not implemented char [{templateChar}]");
						}
					}
					detailedRoomInfo.RoomLayout[i, j] = tile;
				}

			return detailedRoomInfo;
		}

		public static string ChooseRandomTemplate(string mapName)
		{
			var asset = Resources.Load<TextAsset>($"RoomTemplates\\{mapName}");
			return asset.text.Replace("\n", "").Replace("\r", "").Replace(" ", "");
		}

		private static Dictionary<string, string[]> _LEVEL_TEMPLATES;
		private static Dictionary<string, string[]> LEVEL_TEMPLATES
		{
			get { return _LEVEL_TEMPLATES ?? (_LEVEL_TEMPLATES = GetLevelTemplates()); }
		}
		private static Dictionary<string, string[]> GetLevelTemplates()
		{
			var dict = new Dictionary<string, string[]>()
			{
				// Key: RULD

				// Generic
				{ "0000", new string[]{ "Empty_0000" } },

				// Dead Ends
				{ "1000", new string[]{ } },
				{ "0100", new string[]{ } },
				{ "0010", new string[]{ } },
				{ "0001", new string[]{ } },

				// 2 Adjacent Rooms
				{ "1100", new string[]{ } },
				{ "1010", new string[]{ } },
				{ "1001", new string[]{ } },
				{ "0110", new string[]{ } },
				{ "0101", new string[]{ } },
				{ "0011", new string[]{ } },

				// Three Adjacent Rooms
				{ "0111", new string[]{ } },
				{ "1011", new string[]{ } },
				{ "1101", new string[]{ } },
				{ "1110", new string[]{ } },

				// Four Adjacent Rooms
				{ "1111", new string[]{ } },
			};

			return dict;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LD41
{
	public class GameManager : MonoBehaviour
	{
		#region Singleton

		private static GameManager instance;
		public static GameManager Instance => instance;

		private void MakeSingleton()
		{
			if (instance != null && instance != this)
			{
				Destroy(this);
			}

			instance = this;
		}

		#endregion

		[SerializeField]
		private List<PlayerCharacter> Players;

		// If true we are paused
		public bool IsPaused { get; private set; }
		public static bool IsGamePaused => instance.IsPaused;

		public Object RoomPrefab;
		public Object EnemyPrefab;
		public Object BulletPrefab;

		private MetroidCamera _camera;

		private List<RoomInfo> _roomInfos = new List<RoomInfo>();

		private ObjectPool _roomPool;
		private List<Room> _roomsObjects = new List<Room>();
		private Room _currentRoom;

		private ObjectPool _projectilePool;
		private ObjectPool _enemyPool;

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

		public void AddScoreFor(PlayerCharacter murderer)
		{
			Debug.Log($"{murderer.name} is a murderer! +1");
		}

		public IEnumerator RespawnPlayerAfterDelay(PlayerCharacter playerToRespawn)
		{
			yield return new WaitForSeconds(2.0f);
			playerToRespawn.transform.position = _currentRoom.RoomCenter;
			playerToRespawn.CurrentHealth = playerToRespawn.MaxHealth;
			playerToRespawn.gameObject.SetActive(true);
		}

		public void RespawnPlayer(PlayerCharacter playerToRespawn)
		{
			StartCoroutine(RespawnPlayerAfterDelay(playerToRespawn));
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

			var projectilePoolObject = new GameObject("Projectile Object Pool");
			_projectilePool = projectilePoolObject.AddComponent<ObjectPool>();
			_projectilePool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(BulletPrefab, _projectilePool.transform);
				gObj.name = "Projectile Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				poolable.ReturnToPool();
				return poolable;
			});
			_projectilePool.AllocateObjects(50);

			var enemyPoolObject = new GameObject("Enemy Object Pool");
			_enemyPool = enemyPoolObject.AddComponent<ObjectPool>();
			_enemyPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(EnemyPrefab, _enemyPool.transform);
				gObj.name = "Enemy Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				poolable.ReturnToPool();
				return poolable;
			});
			_enemyPool.AllocateObjects(30);
		}

		public IPoolableObject GetBullet()
		{
			return _projectilePool.GetObjectFromPool();
		}

		public void ReturnBullet(IPoolableObject bullet)
		{
			_projectilePool.ReturnObjectToPool(bullet);
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
				roomObj.name = $"Room ({roomInfo.RoomType}) at {roomInfo.Position}";
				var detailedRoomInfo = RoomGenerator.GenerateRoom(roomInfo);
				roomObj.SetRoomInfo(detailedRoomInfo);
				roomObj.DeactivateRoom();
				_roomsObjects.Add(roomObj);
			}
		}

		public Enemy GetEnemy()
		{
			return (Enemy)_enemyPool.GetObjectFromPool();
		}

		public void ReturnEnemy(Enemy e)
		{
			_enemyPool.ReturnObjectToPool(e);
		}

		public void LoadRoom(Room room)
		{
			_currentRoom?.DeactivateRoom();
			_currentRoom = room;
			_currentRoom.ActivateRoom();
			_camera.TransitionToRoom(room);
			foreach (var player in Players)
			{
				player.transform.position = room.RoomCenter;
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LD41
{
	public class GameManager : MonoBehaviour
	{
		public struct GameSettings
		{
			public bool[] PlayersEnabled;
			public int[] PlayerTeams;
			public int MapIndex;
			public int WinningScore;
		}
		public GameSettings GamSetts;
		private List<string> AllMaps = new List<string>();

		public enum GameState
		{
			MainMenu,
			Paused,
			Playing,
			CharSel,
			GamSetties
		}

		public Canvas MainMenu;
		public Canvas CharSelect;
		public Canvas GamSettiesScreen;
		public Canvas GameUI;
		public Canvas PauseMenu;

		public Button StartButt;
		public Text MapName;

		public Text CountDown;
		public ScoreThings[] ScoreThings;

		public PlayerIcon[] PIcons;
		public Vector3[] PIconTeamLocations;

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
			DontDestroyOnLoad(gameObject);
		}

		#endregion

		[SerializeField]
		private List<PlayerCharacter> Players;

		public Color[] TeamColors;

		// If true we are paused
		public GameState _gameState = GameState.MainMenu;
		public static bool IsGamePaused => instance?._gameState != GameState.Playing;

		public Object RoomPrefab;
		public Object PlayerPrefab;
		public Object BulletPrefab;
		public Object ParticlePrefab;

		private bool ShouldContinue = false;

		private MetroidCamera _camera;

		private List<RoomInfo> _roomInfos = new List<RoomInfo>();

		private ObjectPool _roomPool;
		private List<Room> _roomsObjects = new List<Room>();
		[SerializeField]
		private Room CurrentRoom;

		private ObjectPool _particlePool;
		private ObjectPool _projectilePool;
		private ObjectPool _playerPool;

		private void Awake()
		{
			MakeSingleton();
			LoadMaps();

			GamSetts = new GameSettings();
			GamSetts.PlayersEnabled = new bool[4];
			GamSetts.PlayerTeams = new int[4];
			GamSetts.MapIndex = 0;
			GamSetts.WinningScore = 3;

			TeamScores = new int[4];

			for (int i = 0; i < 4; i++)
			{
				GamSetts.PlayersEnabled[i] = false;
				GamSetts.PlayerTeams[i] = 0;
				TeamScores[i] = 0;
			}

			UpdatePlayerIcons();
		}

		public void LoadMaps()
		{
			AllMaps = new List<string>()
			{
				"Delicious_Absorb",
				"Knowledgeable_Telling",
				"Macho_Pen",
				"Maddening_Round",
				"Pathetic_Tumble",
				"Powerful_Sun",
				"Snake_Pinch",
				"Substantial_Swim",
				"Succinct_Appliance",
				"Tough_Drip"
			};
		}

		private float lastSwticheriodus = 0;
		public float SwticheriodusDelay = 0.25f;

		public void NextMap()
		{
			if (!(Time.unscaledTime - lastSwticheriodus > SwticheriodusDelay)) return;
			GamSetts.MapIndex = (GamSetts.MapIndex + 1) % AllMaps.Count;
			lastSwticheriodus = Time.unscaledTime;
		}

		public void PreviousMap()
		{
			if (!(Time.unscaledTime - lastSwticheriodus > SwticheriodusDelay)) return;
			var prev = GamSetts.MapIndex - 1;
			if (prev < 0)
				prev = AllMaps.Count - 1;
			GamSetts.MapIndex = prev % AllMaps.Count;
			lastSwticheriodus = Time.unscaledTime;
		}

		public void UpdateGameSettyScreen()
		{
			MapName.text = AllMaps[GamSetts.MapIndex].Replace("_", " ");
		}

		public void UpdatePlayerIcons()
		{
			for (int i = 0; i < 4; i++)
				PIcons[i].SetStuff(i, GamSetts.PlayerTeams[i], GamSetts.PlayersEnabled[i]);
		}

		public void PlayerJoin(int playerID)
		{
			GamSetts.PlayersEnabled[playerID] = true;
			UpdatePlayerIcons();
		}

		public void PlayerLeave(int playerID)
		{
			GamSetts.PlayersEnabled[playerID] = false;
			UpdatePlayerIcons();
		}

		public void AssignPlayerTeam(int playerID, int team)
		{
			GamSetts.PlayerTeams[playerID] = team;
			UpdatePlayerIcons();
		}

		public void UnassignPlayerTeam(int playerID)
		{
			GamSetts.PlayerTeams[playerID] = -1;
			UpdatePlayerIcons();
		}

		private void OnEnable()
		{
			_camera = FindObjectOfType<MetroidCamera>();

			InputHelper.OnResetPlayerDevices = numCons => OnPlayerReset();

			InitializeObjectPools();
		}

		private void OnPlayerReset()
		{
			for (int i = 0; i < 4; i++)
			{
				GamSetts.PlayersEnabled[i] = false;
				GamSetts.PlayerTeams[i] = -1;
			}

			if (_gameState == GameState.Playing)
			{
				ShouldContinue = true;
				ChangeState(GameState.CharSel);
			}
		}

		private void Start()
		{
			ChangeState(GameState.MainMenu);
		}

		private void OnDrawGizmos()
		{
			if (_gameState == GameState.Playing)
			{
				foreach (var spawn in CurrentRoom.DetailedRoomInfo.Spawns)
				{
					Gizmos.DrawSphere(CurrentRoom.transform.position + (Vector3)((Vector2)spawn) * CONST.PIXELS_PER_UNIT / 2, 5f);
				}
			}
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(CurrentRoom.RoomCenter, 1f);
			Gizmos.color = Color.white;
		}

		public void UpdateScores()
		{
			for (int i = 0; i < 4; i++)
				ScoreThings[i].UpdateScore(TeamScores[i]);
		}

		public void Resume()
		{
			ChangeState(GameState.Playing);
		}

		public void QuitToMM()
		{
			ResetGame();
			GOTOMainMenu();
		}

		private void Update()
		{
			if (_gameState == GameState.Playing)
			{
				UpdateScores();
				for (int i = 0; i < 4; i++)
				{
					var input = InputHelper.GetPlayerInput(i);
					if (input.Menu)
					{
						ChangeState(GameState.Paused);
					}
				}
			}
			else if (_gameState == GameState.CharSel)
			{
				for (int i = 0; i < 4; i++)
				{
					var pInput = InputHelper.GetPlayerInput(i);
					GamSetts.PlayersEnabled[pInput.PlayerID] = !string.IsNullOrWhiteSpace(pInput.PlayerDevice);

					if (pInput.Back)
					{
						if (GamSetts.PlayerTeams[pInput.PlayerID] != -1)
							GamSetts.PlayerTeams[pInput.PlayerID] = -1;
						else
						{
							InputHelper.DisconnectPlayerDevice(pInput.PlayerID);
							continue;
						}
					}

					var movement = new Vector2(pInput.HorizontalMovement, pInput.VerticalMovement);
					if (movement.sqrMagnitude > 0.5f)
					{
						if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
						{
							if (movement.x > 0)
							{
								GamSetts.PlayerTeams[pInput.PlayerID] = 2;
							}
							else if (movement.x < 0)
							{
								GamSetts.PlayerTeams[pInput.PlayerID] = 1;
							}
						}
						else
						{
							if (movement.y < 0)
							{
								GamSetts.PlayerTeams[pInput.PlayerID] = 3;
							}
							else if (movement.y > 0)
							{
								GamSetts.PlayerTeams[pInput.PlayerID] = 0;
							}
						}
					}
				}
				UpdatePlayerIcons();
				StartButt.interactable = VerifySettings();
			}
			else if (_gameState == GameState.GamSetties)
			{
				UpdateGameSettyScreen();
			}
		}

		public void GotoGameSetties()
		{
			ChangeState(GameState.GamSetties);
		}

		public void Quit()
		{
			if (Application.isEditor) Debug.Log("No Quitties");
			else Application.Quit();
		}

		public void GOTOMainMenu()
		{
			ChangeState(GameState.MainMenu);
		}

		public bool VerifySettings()
		{
			bool hasPlayer = false;
			bool allAss = true;
			HashSet<int> teams = new HashSet<int>();
			for (int i = 0; i < 4; i++)
			{
				hasPlayer |= GamSetts.PlayersEnabled[i];
				allAss &= (!GamSetts.PlayersEnabled[i] || GamSetts.PlayerTeams[i] != -1);
				teams.Add(GamSetts.PlayerTeams[i]);
			}
			return hasPlayer && allAss && teams.Count > 1;
		}

		public void StartGame()
		{
			if (!VerifySettings()) return;

			if (!ShouldContinue)
			{
				NewLevel();
			}
			else
			{
				ShouldContinue = false;
			}

			ResetPlayers();
			for (int i = 0; i < 4; i++)
			{
				if (GamSetts.PlayersEnabled[i])
					SpawnPlayerOnTeam(i, GamSetts.PlayerTeams[i]);
			}
			ChangeState(GameState.Playing);
		}

		public void Play1P()
		{
			Debug.Log("TODODODO");
		}

		public void CharacterSelect()
		{
			ChangeState(GameState.CharSel);
		}

		public IEnumerator CountdownTimer()
		{
			CountDown.text = "3";
			yield return new WaitForSecondsRealtime(1);
			CountDown.text = "2";
			yield return new WaitForSecondsRealtime(1);
			CountDown.text = "1";
			yield return new WaitForSecondsRealtime(1);
			CountDown.text = "Go!";
			Time.timeScale = 1;
			yield return new WaitForSecondsRealtime(1);
			CountDown.text = "";
		}

		public void ChangeState(GameState newState)
		{
			_gameState = newState;
			if (_gameState == GameState.MainMenu)
				ResetGame();

			if (_gameState == GameState.Playing)
			{
				CurrentRoom.gameObject.SetActive(true);
				_camera.TransitionToRoom(CurrentRoom);
				StartCoroutine(CountdownTimer());
			}
			else
			{
				Time.timeScale = 0;
				CurrentRoom.gameObject.SetActive(false);
				_camera.transform.position = new Vector3(0, 0, -10);
			}

			MainMenu.gameObject.SetActive(_gameState == GameState.MainMenu);
			CharSelect.gameObject.SetActive(_gameState == GameState.CharSel);
			GameUI.gameObject.SetActive(_gameState == GameState.Playing);
			GamSettiesScreen.gameObject.SetActive(_gameState == GameState.GamSetties);
			PauseMenu.gameObject.SetActive(_gameState == GameState.Paused);
		}

		public IEnumerator OnWinThings()
		{
			yield return new WaitForSecondsRealtime(2);
			ResetGame();
			GOTOMainMenu();
		}

		public void AddScoreFor(PlayerCharacter murderer)
		{
			TeamScores[murderer.TeamID]++;

			if (TeamScores[murderer.TeamID] >= GamSetts.WinningScore)
			{
				for (int i = 0; i < 4; i++)
				{
					if (i == murderer.TeamID)
						ScoreThings[i].Win();
					else
						ScoreThings[i].Lose();
				}
				StartCoroutine(OnWinThings());
			}
		}

		public IEnumerator RespawnPlayerAfterDelay(int playerID, int teamID)
		{
			yield return new WaitForSeconds(2.0f);
			SpawnPlayerOnTeam(playerID, teamID);
		}

		public void RespawnPlayer(PlayerCharacter playerToRespawn)
		{
			StartCoroutine(RespawnPlayerAfterDelay(playerToRespawn.PlayerID, playerToRespawn.TeamID));
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

			var enemyPoolObject = new GameObject("Player Object Pool");
			_playerPool = enemyPoolObject.AddComponent<ObjectPool>();
			_playerPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(PlayerPrefab, _playerPool.transform);
				gObj.name = "Player Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				poolable.ReturnToPool();
				return poolable;
			});
			_playerPool.AllocateObjects(4);

			var particlePoolObject = new GameObject("Particle Object Pool");
			_particlePool = particlePoolObject.AddComponent<ObjectPool>();
			_particlePool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(ParticlePrefab, _particlePool.transform);
				gObj.name = "Particle Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				poolable.ReturnToPool();
				return poolable;
			});
			_particlePool.AllocateObjects(8);
		}

		public IPoolableObject GetBullet()
		{
			return _projectilePool.GetObjectFromPool();
		}

		public void ReturnBullet(IPoolableObject bullet)
		{
			_projectilePool.ReturnObjectToPool(bullet);
		}

		public IPoolableObject GetParticles()
		{
			return _particlePool.GetObjectFromPool();
		}

		public void ReturnParticles(IPoolableObject poolable)
		{
			_particlePool.ReturnObjectToPool(poolable);
		}

		public int[] TeamScores;

		public void ResetGame()
		{
			foreach (var poolable in _roomsObjects)
				_roomPool.ReturnObjectToPool(poolable);
			_roomsObjects.Clear();

			ResetPlayers();

			for (int i = 0; i < 4; i++)
			{
				TeamScores[i] = 0;
			}
		}

		public void ResetPlayers()
		{
			foreach (var player in Players)
				ReturnPlayer(player);
		}

		public void NewLevel()
		{
			RoomInfo roomInfo = new RoomInfo(RoomInfo.RoomTypes.Normal, Vector2Int.zero);
			var detailedRoomInfo = RoomGenerator.GenerateRoom(roomInfo, AllMaps[GamSetts.MapIndex]);
			CurrentRoom.SetRoomInfo(detailedRoomInfo);
			CurrentRoom.ActivateRoom();
			_camera.TransitionToRoom(CurrentRoom);
		}

		public void SpawnPlayerOnTeam(int playerID, int teamID)
		{
			Vector2 spawnPos = CurrentRoom.DetailedRoomInfo.Spawns[Random.Range(0, CurrentRoom.DetailedRoomInfo.Spawns.Count)];
			spawnPos *= CONST.PIXELS_PER_UNIT / 2;
			spawnPos += new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));

			var player = (PlayerCharacter)_playerPool.GetObjectFromPool();
			player.name = $"Player for Team {teamID}";
			player.SetInfo(playerID, teamID, TeamColors[teamID]);
			player.transform.position = (Vector3)spawnPos + CurrentRoom.transform.position;
			player.CurrentHealth = player.MaxHealth;
			Players.Add(player);
		}

		public void ReturnPlayer(IPoolableObject poolable)
		{
			_playerPool.ReturnObjectToPool(poolable);
		}
	}
}

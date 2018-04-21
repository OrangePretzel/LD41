using LD41.Tilemaps;
using UnityEngine;

namespace LD41
{
	public class Room : MonoBehaviour, IPoolableObject
	{
		[SerializeField]
		public Vector2Int?[] DoorPositions = new Vector2Int?[4];
		public LevelTileMap LevelTileMap;

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
			LevelTileMap.Init(width, height);
			DoorPositions = new Vector2Int?[4];
			LevelTileMap.gameObject.SetActive(false);
		}

		public void DeactivateRoom()
		{
			LevelTileMap.gameObject.SetActive(false);
		}

		public void ActivateRoom()
		{
			LevelTileMap.gameObject.SetActive(true);
			LevelTileMap.Render();
		}
	}
}

using UnityEngine;

namespace LD41.Tilemaps
{
	public class LevelTile
	{
		public enum TileTypes
		{
			None,
			Wall
		}

		public TileTypes Type;

		public LevelTile(TileTypes type)
		{
			Type = type;
		}
	}

	public struct SpriteWithTransform
	{
		public int SpritePosition;
		public int Rotation;

		public SpriteWithTransform(int spritePos, int rotation)
		{
			SpritePosition = spritePos;
			Rotation = rotation;
		}

		public SpriteWithTransform(int spritePos)
		{
			SpritePosition = spritePos;
			Rotation = 0;
		}
	}

	[RequireComponent(typeof(GraphicTilemap))]
	public class LevelTileMap : MonoBehaviour
	{
		public LevelTile[,] LevelTiles;
		public int Width => LevelTiles.GetLength(0);
		public int Height => LevelTiles.GetLength(1);

		private GraphicTilemap _graphicTilemap;

		private void OnEnable()
		{
			_graphicTilemap = GetComponent<GraphicTilemap>();
		}

		public void Init(int width, int height)
		{			
			_graphicTilemap.Init();
			LevelTiles = new LevelTile[width, height];
			for (int j = 0; j < Height; j++)
				for (int i = 0; i < Width; i++)
					LevelTiles[i, j] = new LevelTile(LevelTile.TileTypes.None);
		}

		public void Render()
		{
			_graphicTilemap.SetTiles(CalculateTiles(LevelTiles));
			_graphicTilemap.IsDirty = true;
			_graphicTilemap.Render();
		}

		public static GraphicTile[,] CalculateTiles(LevelTile[,] tiles)
		{
			const int SPRITE_SHEET_SIZE = 4;
			var width = tiles.GetLength(0);
			var height = tiles.GetLength(1);
			var gTiles = new GraphicTile[width, height];

			System.Func<int, int, bool> inBounds = (x, y) => x >= 0 && x < width && y >= 0 && y < height;

			for (int j = 0; j < height; j++)
				for (int i = 0; i < width; i++)
				{
					var tile = tiles[i, j];
					if (tile.Type == LevelTile.TileTypes.Wall)
					{
						var wallSpriteInfo = GetWallSpriteInfo(
							inBounds(i + 1, j) ? tiles[i + 1, j].Type == LevelTile.TileTypes.Wall : false, // right
							inBounds(i + 1, j + 1) ? tiles[i + 1, j + 1].Type == LevelTile.TileTypes.Wall : false, // topRight
							inBounds(i, j + 1) ? tiles[i, j + 1].Type == LevelTile.TileTypes.Wall : false, // top
							inBounds(i - 1, j + 1) ? tiles[i - 1, j + 1].Type == LevelTile.TileTypes.Wall : false, // topLeft
							inBounds(i - 1, j) ? tiles[i - 1, j].Type == LevelTile.TileTypes.Wall : false, // left
							inBounds(i - 1, j - 1) ? tiles[i - 1, j - 1].Type == LevelTile.TileTypes.Wall : false, // bottomLeft
							inBounds(i, j - 1) ? tiles[i, j - 1].Type == LevelTile.TileTypes.Wall : false, // bottom
							inBounds(i + 1, j - 1) ? tiles[i + 1, j - 1].Type == LevelTile.TileTypes.Wall : false // bottomRight
							);
						gTiles[i, j] = new GraphicTile
							(wallSpriteInfo.SpritePosition % SPRITE_SHEET_SIZE,
							wallSpriteInfo.SpritePosition / SPRITE_SHEET_SIZE,
							wallSpriteInfo.Rotation * 90);
					}
				}

			return gTiles;
		}

		public static SpriteWithTransform GetWallSpriteInfo(bool right, bool topRight, bool top, bool topLeft, bool left, bool bottomLeft, bool bottom, bool bottomRight)
		{
			int majorCount =
				(top ? 1 : 0) +
				(bottom ? 1 : 0) +
				(left ? 1 : 0) +
				(right ? 1 : 0);

			switch (majorCount)
			{
				case 0:
					return new SpriteWithTransform(0);
				case 1:
					if (right)
						return new SpriteWithTransform(1, 0);
					if (top)
						return new SpriteWithTransform(1, 1);
					if (left)
						return new SpriteWithTransform(1, 2);
					if (bottom)
						return new SpriteWithTransform(1, 3);
					break;
				case 2:
					if (left && right)
						return new SpriteWithTransform(2, 0);
					if (top && bottom)
						return new SpriteWithTransform(2, 1);
					if (top)
					{
						if (right)
							return new SpriteWithTransform(topRight ? 3 : 4, 0);
						if (left)
							return new SpriteWithTransform(topLeft ? 3 : 4, 1);
					}
					if (bottom)
					{
						if (left)
							return new SpriteWithTransform(bottomLeft ? 3 : 4, 2);
						if (right)
							return new SpriteWithTransform(bottomRight ? 3 : 4, 3);
					}
					break;
				case 3:
					if (!left)
					{
						if (topRight && bottomRight)
							return new SpriteWithTransform(5, 0);
						if (topRight)
							return new SpriteWithTransform(6, 0);
						if (bottomRight)
							return new SpriteWithTransform(7, 0);
						return new SpriteWithTransform(8, 0);
					}
					if (!bottom)
					{
						if (topLeft && topRight)
							return new SpriteWithTransform(5, 1);
						if (topLeft)
							return new SpriteWithTransform(6, 1);
						if (topRight)
							return new SpriteWithTransform(7, 1);
						return new SpriteWithTransform(8, 1);
					}
					if (!right)
					{
						if (topLeft && bottomLeft)
							return new SpriteWithTransform(5, 2);
						if (bottomLeft)
							return new SpriteWithTransform(6, 2);
						if (topLeft)
							return new SpriteWithTransform(7, 2);
						return new SpriteWithTransform(8, 2);
					}
					if (!top)
					{
						if (bottomLeft && bottomRight)
							return new SpriteWithTransform(5, 3);
						if (bottomRight)
							return new SpriteWithTransform(6, 3);
						if (bottomLeft)
							return new SpriteWithTransform(7, 3);
						return new SpriteWithTransform(8, 3);
					}
					break;
				case 4:
					int minorCount =
						(topRight ? 1 : 0) +
						(bottomRight ? 1 : 0) +
						(topLeft ? 1 : 0) +
						(bottomLeft ? 1 : 0);

					switch (minorCount)
					{
						case 4:
							return new SpriteWithTransform(9);
						case 3:
							if (!topRight)
								return new SpriteWithTransform(10, 0);
							if (!topLeft)
								return new SpriteWithTransform(10, 1);
							if (!bottomLeft)
								return new SpriteWithTransform(10, 2);
							if (!bottomRight)
								return new SpriteWithTransform(10, 3);
							break;
						case 2:
							if (bottomRight && topLeft)
								return new SpriteWithTransform(11, 0);
							if (topRight && bottomLeft)
								return new SpriteWithTransform(11, 1);

							if (topLeft && bottomLeft)
								return new SpriteWithTransform(12, 0);
							if (bottomLeft && bottomRight)
								return new SpriteWithTransform(12, 1);
							if (bottomRight && topRight)
								return new SpriteWithTransform(12, 2);
							if (topRight && topLeft)
								return new SpriteWithTransform(12, 3);
							break;
						case 1:
							if (topLeft)
								return new SpriteWithTransform(13, 0);
							if (bottomLeft)
								return new SpriteWithTransform(13, 1);
							if (bottomRight)
								return new SpriteWithTransform(13, 2);
							if (topRight)
								return new SpriteWithTransform(13, 3);
							break;
						case 0:
							return new SpriteWithTransform(14);
					}
					break;
			}

			return new SpriteWithTransform(0);
		}
	}
}

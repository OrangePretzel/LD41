using System.Collections.Generic;
using UnityEngine;

namespace LD41.Tilemaps
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshCollider))]
	public class GraphicTilemap : MonoBehaviour
	{
		public float UVTileSize = 0.25f;
		public float TileSize = CONST.PIXELS_PER_UNIT;

		private GraphicTile[,] _tiles;
		public GraphicTile[,] Tiles => _tiles;

		public int Width => _tiles.GetLength(0);
		public int Height => _tiles.GetLength(1);
		public bool IsDirty;

		private MeshRenderer _meshRenderer;
		private MeshFilter _meshFilter;
		private MeshCollider _meshCollider;

		private void OnEnable()
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			_meshFilter = GetComponent<MeshFilter>();
			_meshCollider = GetComponent<MeshCollider>();
			IsDirty = true;
		}

		public void Init()
		{
			_tiles = new GraphicTile[16, 16];

			for (int j = 0; j < Height; j++)
				for (int i = 0; i < Width; i++)
				{
					_tiles[i, j] = new GraphicTile(i == j ? 1 : 0, 0);
				}
		}

		public void SetTiles(GraphicTile[,] graphicTiles)
		{
			_tiles = graphicTiles;
		}

		public void Render()
		{
			if (!IsDirty) return;
			IsDirty = false;

			Mesh mesh = new Mesh();

			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			List<Vector2> uvs = new List<Vector2>();

			int vertOrig;
			for (int j = 0; j < Height; j++)
				for (int i = 0; i < Width; i++)
				{
					var tile = _tiles[i, j];
					if (tile == null) continue;

					vertOrig = vertices.Count;

					vertices.Add(new Vector3(i, j) * TileSize);
					vertices.Add(new Vector3(i + 1, j) * TileSize);
					vertices.Add(new Vector3(i + 1, j + 1) * TileSize);
					vertices.Add(new Vector3(i, j + 1) * TileSize);

					triangles.Add(vertOrig + 0);
					triangles.Add(vertOrig + 1);
					triangles.Add(vertOrig + 2);
					triangles.Add(vertOrig + 0);
					triangles.Add(vertOrig + 2);
					triangles.Add(vertOrig + 3);

					var uv1 = new Vector2(UVTileSize * tile.GraphicX, UVTileSize * tile.GraphicY);
					var uv2 = new Vector2(UVTileSize * tile.GraphicX + UVTileSize, UVTileSize * tile.GraphicY);
					var uv3 = new Vector2(UVTileSize * tile.GraphicX + UVTileSize, UVTileSize * tile.GraphicY + UVTileSize);
					var uv4 = new Vector2(UVTileSize * tile.GraphicX, UVTileSize * tile.GraphicY + UVTileSize);

					switch (tile.Rotation)
					{
						case 0:
							uvs.AddRange(new Vector2[] { uv1, uv2, uv3, uv4 });
							break;
						case 90:
							uvs.AddRange(new Vector2[] { uv4, uv1, uv2, uv3 });
							break;
						case 180:
							uvs.AddRange(new Vector2[] { uv3, uv4, uv1, uv2 });
							break;
						case 270:
							uvs.AddRange(new Vector2[] { uv2, uv3, uv4, uv1 });
							break;
						default:
							break;
					}
				}

			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetUVs(0, uvs);

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			_meshFilter.sharedMesh = mesh;
			_meshCollider.sharedMesh = mesh;
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace LD41.Tilemaps
{
	public class GraphicTile
	{
		public int GraphicX;
		public int GraphicY;

		public GraphicTile(int graphicX, int graphicY)
		{
			GraphicX = graphicX;
			GraphicY = graphicY;
		}
	}

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshCollider))]
	public class GraphicTilemap : MonoBehaviour
	{
		public float TileSize = 0.25f;

		public GraphicTile[,] Tiles;
		public int Width => Tiles.GetLength(0);
		public int Height => Tiles.GetLength(1);
		public bool IsDirty;

		private MeshRenderer _meshRenderer;
		private MeshFilter _meshFilter;
		private MeshCollider _meshCollider;

		private void OnEnable()
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			_meshFilter = GetComponent<MeshFilter>();
			_meshCollider = GetComponent<MeshCollider>();
			Init();
			IsDirty = true;
		}

		private float frame = 0;
		private void FixedUpdate()
		{
			frame += Time.deltaTime;
			if (frame >= 2)
			{
				frame = 0;
				Tiles[0, 0].GraphicX = (Tiles[0, 0].GraphicX + 1) % 4;
				IsDirty = true;
			}
			Render();
		}

		public void Init()
		{
			Tiles = new GraphicTile[1, 1];

			for (int j = 0; j < Height; j++)
				for (int i = 0; i < Width; i++)
				{
					Tiles[i, j] = new GraphicTile(0, 0);
				}
		}

		public void Render()
		{
			if (!IsDirty) return;
			IsDirty = false;

			Mesh mesh = new Mesh();

			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			List<Vector2> uvs = new List<Vector2>();

			int triOrig;
			for (int j = 0; j < Height; j++)
				for (int i = 0; i < Width; i++)
				{
					var tile = Tiles[i, j];

					vertices.Add(new Vector3(i, j));
					vertices.Add(new Vector3(i + 1, j));
					vertices.Add(new Vector3(i, j + 1));
					vertices.Add(new Vector3(i + 1, j + 1));

					triOrig = triangles.Count;
					triangles.Add(triOrig + 0);
					triangles.Add(triOrig + 2);
					triangles.Add(triOrig + 1);
					triangles.Add(triOrig + 3);
					triangles.Add(triOrig + 1);
					triangles.Add(triOrig + 2);

					uvs.Add(new Vector2(TileSize * tile.GraphicX, TileSize * tile.GraphicY));
					uvs.Add(new Vector2(TileSize * tile.GraphicX + TileSize, TileSize * tile.GraphicY));
					uvs.Add(new Vector2(TileSize * tile.GraphicX, TileSize * tile.GraphicY + TileSize));
					uvs.Add(new Vector2(TileSize * tile.GraphicX + TileSize, TileSize * tile.GraphicY + TileSize));
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

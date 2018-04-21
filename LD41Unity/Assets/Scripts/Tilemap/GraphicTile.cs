namespace LD41.Tilemaps
{
	public class GraphicTile
	{
		public int GraphicX;
		public int GraphicY;
		public int Rotation;

		public GraphicTile(int graphicX, int graphicY, int rotation=0)
		{
			GraphicX = graphicX;
			GraphicY = graphicY;
			Rotation = rotation;
		}
	}
}

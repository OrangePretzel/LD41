using UnityEngine;

namespace LD41
{
	public static class MathHelper
	{
		public static Vector2 RotateVector(this Vector2 vec, float degrees)
		{
			var sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			var cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
			var tx = vec.x;
			var ty = vec.y;
			vec.x = (cos * tx) - (sin * ty);
			vec.y = (sin * tx) + (cos * ty);
			return vec;
		}
	}
}

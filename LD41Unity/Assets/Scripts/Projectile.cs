using UnityEngine;

namespace LD41
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Projectile : MonoBehaviour, IPoolableObject
	{
		private Rigidbody2D _rigidbody;

		public PlayerCharacter Player;

		private void OnEnable()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		public void ActivateFromPool()
		{
			gameObject.SetActive(true);
		}

		public void ReturnToPool()
		{
			gameObject.SetActive(false);
		}

		public void SetDir(Vector2 dir)
		{
			_rigidbody.velocity = dir * CONST.PIXELS_PER_UNIT * 10;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.tag == "BadBoy")
			{
				var badboy = collision.gameObject.GetComponent<Enemy>();
				badboy.Die();
				GameManager.Instance.ReturnBullet(this);
			}
			else if (collision.gameObject == Player.gameObject)
			{
				// Ignore
			}
			else if(collision.gameObject.tag == "PlayerBoy")
			{
				collision.gameObject.GetComponent<PlayerCharacter>().TakeDamageFrom(Player);
				GameManager.Instance.ReturnBullet(this);
			}
			else
			{
				GameManager.Instance.ReturnBullet(this);
			}
		}
	}
}

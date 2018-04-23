using System.Collections.Generic;
using UnityEngine;

namespace LD41
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Enemy : MonoBehaviour, IPoolableObject
	{
		public PlayerCharacter TargetPlayer;

		public float MaxSpeed = 3.5f;

		private Rigidbody2D _rigidbody;

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

		private void Update()
		{
			if (TargetPlayer == null) return;

			var dir = TargetPlayer.transform.position - transform.position;
			_rigidbody.velocity += (Vector2)dir * 0.5f;
			var maxSpeed = MaxSpeed * CONST.PIXELS_PER_UNIT;
			if (_rigidbody.velocity.sqrMagnitude > maxSpeed * maxSpeed)
				_rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed;
		}

		public void Die()
		{
			//GameManager.Instance.ReturnEnemy(this);
		}
	}
}

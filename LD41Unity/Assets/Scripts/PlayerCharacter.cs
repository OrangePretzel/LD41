using UnityEngine;

namespace LD41
{
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(Animator))]
	public class PlayerCharacter : MonoBehaviour
	{
		[SerializeField]
		private SpriteRenderer GunArmSprite;
		[SerializeField]
		private SpriteRenderer BodySprite;

		public Transform ShootiePoint;

		public float RotationPerSecond = 2;
		public float AccelerationPerSecond = 0.5f;
		public float MaxSpeed = 2;

		public int PlayerID = 0;
		private PlayerInput playerInput;

		private Rigidbody2D _rigidbody;
		private ParticleSystem _particleSystem;
		private Animator _animator;

		public float ShotCoolDown = 0.5f;
		private float _lastShot = 0;

		private void OnEnable()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
			_particleSystem = GetComponentInChildren<ParticleSystem>();
			_animator = GetComponent<Animator>();
		}

		private void Update()
		{
			if (GameManager.IsGamePaused) return;

			// Get input associated with the player's controller
			playerInput = InputHelper.GetPlayerInput(PlayerID);

			if (playerInput.HorizontalMovement != 0)
			{
				_rigidbody.angularVelocity = 0;
				var amountToRotate = -playerInput.HorizontalMovement * RotationPerSecond * Time.deltaTime;
				var newAngle = transform.localRotation.eulerAngles.z + amountToRotate;
				transform.localRotation = Quaternion.Euler(new Vector3(0, 0, newAngle));
			}

			if (playerInput.Jump)
			{
				var velocity = _rigidbody.velocity;
				velocity += (Vector2)transform.up * AccelerationPerSecond * CONST.PIXELS_PER_UNIT;

				var maxSpeed = MaxSpeed * CONST.PIXELS_PER_UNIT;
				if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
				{
					velocity = velocity.normalized * maxSpeed;
				}

				_rigidbody.velocity = velocity;

				_particleSystem.Emit(1);
				_animator.SetBool("moving", true);
			}
			else
			{
				_animator.SetBool("moving", false);
			}

			var aimVec = playerInput.GetNormalizedAim(transform.position, Camera.main);
			if (aimVec.x != 0 || aimVec.y != 0)
			{
				GunArmSprite.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(aimVec.y, aimVec.x) * Mathf.Rad2Deg);
				var gangle = GunArmSprite.transform.localEulerAngles.z;
				if (gangle >= 90 && gangle <= 270)
				{
					BodySprite.transform.localScale = new Vector3(-1, 1, 1);
					GunArmSprite.transform.localScale = new Vector3(1, -1, 1);
				}
				else
				{
					BodySprite.transform.localScale = new Vector3(1, 1, 1);
					GunArmSprite.transform.localScale = new Vector3(1, 1, 1);
				}
			}

			if (playerInput.Shooting)
			{
				if (Time.time - _lastShot > ShotCoolDown)
				{
					_lastShot = Time.time;
					var bullet = (Projectile)GameManager.Instance.GetBullet();
					bullet.transform.position = ShootiePoint.position;
					bullet.SetDir(GunArmSprite.transform.right);
				}
			}
		}
	}
}

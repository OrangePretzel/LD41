using UnityEngine;

namespace LD41
{
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Collider2D))]
	public class PlayerCharacter : MonoBehaviour
	{
		public float RotationPerSecond = 2;
		public float AccelerationPerSecond = 0.5f;
		public float MaxSpeed = 2;

		private int PlayerID = 0;
		private PlayerInput playerInput;

		private Rigidbody2D _rigidbody;

		private void OnEnable()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
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
			}
		}
	}
}

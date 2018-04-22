using UnityEngine;

namespace LD41
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Collider2D))]
	public class Door : MonoBehaviour
	{
		private Animator _animator;
		private Collider2D _collider;

		private void OnEnable()
		{
			_animator = GetComponent<Animator>();
			_collider = GetComponent<Collider2D>();
		}

		public void OpenDoor()
		{
			//_animator.SetBool("closed", false);
			//_collider.enabled = false;
		}

		public void CloseDoor()
		{
			//_animator.SetBool("closed", true);
			//_collider.enabled = true;
		}
	}
}

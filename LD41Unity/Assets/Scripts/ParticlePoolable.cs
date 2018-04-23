using System.Collections;
using UnityEngine;

namespace LD41
{
	public class ParticlePoolable : MonoBehaviour, IPoolableObject
	{
		public void ActivateFromPool()
		{
			gameObject.SetActive(true);
			var partSys = GetComponent<ParticleSystem>();
			partSys.Play();
			StartCoroutine(WaitForDone(partSys));
		}

		public IEnumerator WaitForDone(ParticleSystem partSys)
		{
			while(partSys.isPlaying)
			{
				yield return new WaitForEndOfFrame();
			}
			GameManager.Instance.ReturnParticles(this);
		}

		public void ReturnToPool()
		{
			gameObject.SetActive(false);
		}
	}
}

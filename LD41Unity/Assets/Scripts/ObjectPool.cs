using System;
using System.Collections.Generic;
using UnityEngine;

namespace LD41
{
	public interface IPoolableObject
	{
		void ReturnToPool();
		void ActivateFromPool();
	}

	public class ObjectPool : MonoBehaviour
	{
		private const int ALLOCATION_INCREMENT = 5;

		private Queue<IPoolableObject> _objectPool = new Queue<IPoolableObject>();

		private Func<IPoolableObject> _createPoolableFunc;

		public void SetAllocationFunction(Func<IPoolableObject> allocationFunction)
		{
			_createPoolableFunc = allocationFunction;
		}

		public void AllocateObjects(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var obj = _createPoolableFunc?.Invoke();
				if (obj == null)
				{
					throw new Exception("Allocation function failed to return a valid object");
				}

				_objectPool.Enqueue(obj);
			}
		}

		public IPoolableObject GetObjectFromPool()
		{
			if (_objectPool.Count < 1)
				AllocateObjects(ALLOCATION_INCREMENT);

			var obj = _objectPool.Dequeue();
			obj.ActivateFromPool();
			return obj;
		}

		public void ReturnObjectToPool(IPoolableObject poolableObject)
		{
			poolableObject.ReturnToPool();

			_objectPool.Enqueue(poolableObject);
		}
	}
}
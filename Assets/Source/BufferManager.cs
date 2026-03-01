using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn
{
	public class BufferManager
	{
		record BufferInstance
		{
			public Action Callback;
			public Func<bool> Predicate;
			public float TimeoutTime;
		}

		private readonly Dictionary<object, BufferInstance> _buffers = new();

		public void Buffer(object key, float timeout, Action callback, Func<bool> predicate)
		{
			if (_buffers.ContainsKey(key))
			{
				_buffers.Remove(key);
			}

			var instance = new BufferInstance()
			{
				Callback = callback,
				Predicate = predicate,
				TimeoutTime = Time.time + timeout
			};

			_buffers.Add(key, instance);
		}
		public void Buffer(float timeout, Action callback, Func<bool> predicate)
		{
			Buffer(new object(), timeout, callback, predicate);
		}

		public void Update()
		{
			var toRemove = new HashSet<object>();

			foreach (var buffer in _buffers)
			{
				if (Time.time > buffer.Value.TimeoutTime)
				{
					toRemove.Add(buffer.Key);
					continue;
				}

				if (buffer.Value.Predicate())
				{
					buffer.Value.Callback();
					toRemove.Add(buffer.Key);
				}
			}

			// Remove timed-out or completed buffers.
			foreach (var key in toRemove)
			{
				_buffers.Remove(key);
			}
		}

		public void Clear()
		{
			_buffers.Clear();
		}
	}
}

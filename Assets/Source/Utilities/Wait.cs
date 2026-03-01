using System;
using System.Threading;
using UnityEngine;

namespace Quinn
{
	public static class Wait
	{
		public static async Awaitable Seconds(float seconds, CancellationToken token = default)
		{
			try
			{
				await Awaitable.WaitForSecondsAsync(seconds, token);
			}
			catch (OperationCanceledException) { }
		}

		public static async Awaitable NextFrame(CancellationToken token = default)
		{
			try
			{
				await Awaitable.NextFrameAsync(token);
			}
			catch (OperationCanceledException) { }
		}

		public static async Awaitable Until(Func<bool> predicate, CancellationToken token = default)
		{
			try
			{
				while (!predicate() && !token.IsCancellationRequested)
				{
					await NextFrame(token);
				}
			}
			catch (OperationCanceledException) { }
		}

		public static async Awaitable While(Func<bool> predicate, CancellationToken token = default)
		{
			try
			{
				while (predicate() && !token.IsCancellationRequested)
				{
					await NextFrame(token);
				}
			}
			catch (OperationCanceledException) { }
		}
	}
}

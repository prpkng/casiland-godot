using Godot;

namespace UnityHFSM
{
	/// <summary>
	/// Default timer that calculates the elapsed time based on <c>Time.time</c>.
	/// </summary>
	public class Timer : ITimer
	{
		public float startTime;
		public float Elapsed => Time.GetTicksMsec() / 1000f - startTime;

		public void Reset()
		{
			startTime = Time.GetTicksMsec() / 1000f;
		}

		public static bool operator >(Timer timer, float duration)
			=> timer.Elapsed > duration;

		public static bool operator <(Timer timer, float duration)
			=> timer.Elapsed < duration;

		public static bool operator >=(Timer timer, float duration)
			=> timer.Elapsed >= duration;

		public static bool operator <=(Timer timer, float duration)
			=> timer.Elapsed <= duration;
	}
}

using System;
using Fractural.Tasks;
using Godot;

namespace Casiland.Common
{
    /// <summary>
    /// A VERY cheap UNSCALED countdown system. Used for many purposes, cooldowns, coyote timers, etc.
    /// The behavior depends on the variable: <see cref="Time.GetTicksMsec"/>
    /// </summary>
    public struct Countdown
    {
        private double _finishTime;
        private static double TimeAsDouble => Time.GetTicksMsec() / 1000.0;
        /// <summary>
        /// Whether the Countdown has finished or not
        /// </summary>
        public bool IsFinished => TimeAsDouble > _finishTime;
        /// <summary>
        /// The amount of time in double until the countdown is finished
        /// </summary>
        public double RemainingTime => IsFinished ? 0 : _finishTime - TimeAsDouble;
        /// <summary>
        /// The amount of time in float until the countdown is finished
        /// </summary>
        public float RemainingTimeF => (float)RemainingTime;

        /// <summary>
        /// Runs a given action after the countdown reaches its end.
        /// NOTE that it does not consider the case of the countdown being set again after this being called
        /// </summary>
        public void OnFinish(Action action)
        {
            var self = this;
            _ = GDTask.WaitUntil(() => self.IsFinished).ContinueWith(action);
        }
        
        public void SetCountdown(double duration) => _finishTime = TimeAsDouble + duration;

        public void Reset() => _finishTime = -1;
    }
}
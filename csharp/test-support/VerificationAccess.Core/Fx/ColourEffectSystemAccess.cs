using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="ColourEffectSystem"/> members production does not use.</summary>
internal static class ColourEffectSystemAccess
{
    /// <summary>Exposes <see cref="ColourEffectSystem"/> state that only verification reads.</summary>
    extension(ColourEffectSystem self)
    {
        /// <summary>The running effects, in the order they take their turns, which a recorded state's import replaces.</summary>
        internal List<RoomColourEffect> RunningEffects => self.effects;

        /// <summary>Whether <paramref name="effect"/> is running.</summary>
        public bool IsRunning(ColourEffectId effect) => self.RunningEffects.Exists(running => running.Effect == effect);
    }
}

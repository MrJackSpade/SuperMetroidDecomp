using static SuperMetroid.Core.Bosses.Phantoon.PhantoonSoundDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Steps Phantoon's materialization cycle.</summary>
internal static class PhantoonSounds
{
    /// <summary>
    /// The materialization sound that follows <paramref name="sound"/> in the cycle: the next of
    /// <see cref="PhantoonSoundDefinitions.MaterializationSounds"/>, the first again after the last.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sound"/> is not one of the cycle's sounds.</exception>
    internal static SoundEffectId MaterializationSoundAfter(SoundEffectId sound)
    {
        int place = Array.IndexOf(MaterializationSounds, sound);
        return place >= 0
            ? MaterializationSounds[(place + 1) % MaterializationSounds.Length]
            : throw new ArgumentOutOfRangeException(nameof(sound), sound, "Not a sound of Phantoon's materialization cycle.");
    }
}


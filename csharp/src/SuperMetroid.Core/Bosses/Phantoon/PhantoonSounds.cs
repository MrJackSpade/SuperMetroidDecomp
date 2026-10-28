using static SuperMetroid.Core.Bosses.Phantoon.PhantoonSoundDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Chooses the sound of each step of Phantoon's materialization cycle.</summary>
internal static class PhantoonSounds
{
    /// <summary>Returns the materialization sound at a zero-through-two cycle position.</summary>
    internal static SoundEffectId MaterializationSound(int index) =>
        (uint)index < (uint)MaterializationSounds.Length
            ? MaterializationSounds[index]
            : throw new ArgumentOutOfRangeException(
                nameof(index), index, "Phantoon materialization sound index must be zero through two.");
}

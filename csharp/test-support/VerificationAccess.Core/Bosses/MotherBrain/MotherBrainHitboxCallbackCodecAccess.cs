using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cartridge words of <see cref="MotherBrainHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class MotherBrainHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="MotherBrainHitboxCallbackCodec"/>.</summary>
    extension(MotherBrainHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(MotherBrainHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(MotherBrainHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(MotherBrainHitboxShot shot) => EnemyInteractionCallbackAccess.Word(MotherBrainHitboxCallbackCodec.ToCallback(shot));
    }
}

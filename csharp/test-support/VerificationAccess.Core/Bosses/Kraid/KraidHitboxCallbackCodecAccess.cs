using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The cartridge words of <see cref="KraidHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class KraidHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="KraidHitboxCallbackCodec"/>.</summary>
    extension(KraidHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(KraidHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(KraidHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(KraidHitboxShot shot) => EnemyInteractionCallbackAccess.Word(KraidHitboxCallbackCodec.ToCallback(shot));
    }
}

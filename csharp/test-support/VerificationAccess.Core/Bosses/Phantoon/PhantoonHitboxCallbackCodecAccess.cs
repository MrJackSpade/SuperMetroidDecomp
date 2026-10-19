using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The cartridge words of <see cref="PhantoonHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class PhantoonHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="PhantoonHitboxCallbackCodec"/>.</summary>
    extension(PhantoonHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(PhantoonHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(PhantoonHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(PhantoonHitboxShot shot) => EnemyInteractionCallbackAccess.Word(PhantoonHitboxCallbackCodec.ToCallback(shot));
    }
}

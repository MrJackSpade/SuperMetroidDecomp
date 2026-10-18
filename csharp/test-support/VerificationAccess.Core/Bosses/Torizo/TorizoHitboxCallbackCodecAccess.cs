using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The cartridge words of <see cref="TorizoHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class TorizoHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="TorizoHitboxCallbackCodec"/>.</summary>
    extension(TorizoHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(TorizoHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(TorizoHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(TorizoHitboxShot shot) => EnemyInteractionCallbackAccess.Word(TorizoHitboxCallbackCodec.ToCallback(shot));
    }
}

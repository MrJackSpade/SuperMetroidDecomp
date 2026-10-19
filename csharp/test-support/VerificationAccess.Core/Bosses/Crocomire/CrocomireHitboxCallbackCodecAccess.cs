using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The cartridge words of <see cref="CrocomireHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class CrocomireHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="CrocomireHitboxCallbackCodec"/>.</summary>
    extension(CrocomireHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(CrocomireHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(CrocomireHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(CrocomireHitboxShot shot) => EnemyInteractionCallbackAccess.Word(CrocomireHitboxCallbackCodec.ToCallback(shot));
    }
}

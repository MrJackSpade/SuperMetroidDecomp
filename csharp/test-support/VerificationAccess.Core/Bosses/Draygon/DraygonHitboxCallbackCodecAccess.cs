using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The cartridge words of <see cref="DraygonHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class DraygonHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="DraygonHitboxCallbackCodec"/>.</summary>
    extension(DraygonHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(DraygonHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(DraygonHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(DraygonHitboxShot shot) => EnemyInteractionCallbackAccess.Word(DraygonHitboxCallbackCodec.ToCallback(shot));
    }
}

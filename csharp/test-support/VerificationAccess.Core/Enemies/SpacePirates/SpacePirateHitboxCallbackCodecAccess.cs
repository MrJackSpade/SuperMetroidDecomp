namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The cartridge words of <see cref="SpacePirateHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class SpacePirateHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="SpacePirateHitboxCallbackCodec"/>.</summary>
    extension(SpacePirateHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(SpacePirateHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(SpacePirateHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(SpacePirateHitboxShot shot) => EnemyInteractionCallbackAccess.Word(SpacePirateHitboxCallbackCodec.ToCallback(shot));
    }
}

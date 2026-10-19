using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The cartridge words of <see cref="SporeSpawnHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class SporeSpawnHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="SporeSpawnHitboxCallbackCodec"/>.</summary>
    extension(SporeSpawnHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="touch"/> selects.</summary>
        internal static ushort ToCallbackWord(SporeSpawnHitboxTouch touch) => EnemyInteractionCallbackAccess.Word(SporeSpawnHitboxCallbackCodec.ToCallback(touch));

        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(SporeSpawnHitboxShot shot) => EnemyInteractionCallbackAccess.Word(SporeSpawnHitboxCallbackCodec.ToCallback(shot));
    }
}

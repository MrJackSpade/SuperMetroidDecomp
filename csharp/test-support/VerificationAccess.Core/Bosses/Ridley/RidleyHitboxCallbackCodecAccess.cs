using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The cartridge words of <see cref="RidleyHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class RidleyHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="RidleyHitboxCallbackCodec"/>.</summary>
    extension(RidleyHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(bool shot) => EnemyInteractionCallbackAccess.Word(RidleyHitboxCallbackCodec.ToCallback(shot));

        /// <summary>The cartridge word of the touch callback.</summary>
        internal static ushort TouchCallbackWord => EnemyInteractionCallbackAccess.Word(RidleyHitboxCallbackCodec.TouchCallback);

        /// <summary>The cartridge word of the shot callback.</summary>
        internal static ushort ShotCallbackWord => EnemyInteractionCallbackAccess.Word(RidleyHitboxCallbackCodec.ShotCallback);
    }
}

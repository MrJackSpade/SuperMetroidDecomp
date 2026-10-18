using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The cartridge words of <see cref="CeresSteamHitboxCallbackCodec"/>'s callbacks, for parity oracles.</summary>
internal static class CeresSteamHitboxCallbackCodecAccess
{
    /// <summary>The word views, read as members of <see cref="CeresSteamHitboxCallbackCodec"/>.</summary>
    extension(CeresSteamHitboxCallbackCodec)
    {
        /// <summary>The cartridge word of the callback <paramref name="shot"/> selects.</summary>
        internal static ushort ToCallbackWord(bool shot) => EnemyInteractionCallbackAccess.Word(CeresSteamHitboxCallbackCodec.ToCallback(shot));

        /// <summary>The cartridge word of the touch callback.</summary>
        internal static ushort TouchCallbackWord => EnemyInteractionCallbackAccess.Word(CeresSteamHitboxCallbackCodec.TouchCallback);

        /// <summary>The cartridge word of the shot callback.</summary>
        internal static ushort ShotCallbackWord => EnemyInteractionCallbackAccess.Word(CeresSteamHitboxCallbackCodec.ShotCallback);
    }
}

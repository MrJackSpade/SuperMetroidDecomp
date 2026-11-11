
namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Cartridge values of <see cref="CommonEnemyEmptyFrameDefinitions"/> that only verification reads.</summary>
internal static class CommonEnemyEmptyFrameDefinitionsConstants
{
    /// <summary>Bank-local one-point hitbox list at $8059.</summary>
    public const ushort PointHitboxList = 0x8059;

    /// <summary>Provides the point-hitbox list used to verify the common enemy empty frame representation.</summary>
    extension(CommonEnemyEmptyFrameDefinitions)
    {
        /// <inheritdoc cref="CommonEnemyEmptyFrameDefinitionsConstants.PointHitboxList"/>
        internal static ushort PointHitboxList => CommonEnemyEmptyFrameDefinitionsConstants.PointHitboxList;
    }
    /// <summary>Selects the shared bank-$A0 shot (<c>NormalEnemyShotAI</c>) or touch (<c>NormalEnemyTouchAI</c>) callback used by the empty frame.</summary>
    internal static ushort Callback(bool selectShot) => selectShot ? (ushort)0x802d : (ushort)0x8023;
}

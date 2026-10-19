
namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Cartridge values of <see cref="CommonEnemyEmptyExtendedFrameDefinitions"/> that only verification reads.</summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitionsConstants
{
    /// <summary>Bank-local one-point hitbox list at $8059.</summary>
    public const ushort PointHitboxList = 0x8059;

    /// <summary>Provides the point-hitbox list used to verify the common enemy empty frame representation.</summary>
    extension(CommonEnemyEmptyExtendedFrameDefinitions)
    {
        /// <inheritdoc cref="CommonEnemyEmptyExtendedFrameDefinitionsConstants.PointHitboxList"/>
        internal static ushort PointHitboxList => CommonEnemyEmptyExtendedFrameDefinitionsConstants.PointHitboxList;
    }
    /// <summary>Selects the shared bank-$A0 shot or touch callback used by the empty frame.</summary>
    internal static ushort Callback(bool selectShot) => selectShot
        ? EnemyAiCodePointers.BankA0.NormalEnemyShot
        : EnemyAiCodePointers.BankA0.NormalEnemyTouch;
}

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>Cartridge values and helpers of <see cref="BossBodyParts"/> that only verification reads.</summary>
internal static class BossBodyPartsConstants
{
    /// <summary>
    /// A body of <paramref name="owner"/>'s no part shows a frame on yet, which the room shows only once it is revealed, as
    /// Mother Brain's second phase begins.
    /// </summary>
    public static BossBodyParts Hidden(BossBodyOwner owner) => BossBodyParts.Hidden(owner, null);
}

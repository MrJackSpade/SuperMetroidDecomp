using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Verification's native view of the Alcoon identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class AlcoonNativeWords
{
    /// <summary>The routine word naming each <see cref="AlcoonAction"/>.</summary>
    internal static readonly NativeWordIdentities<AlcoonAction> AlcoonActions = new(new Dictionary<AlcoonAction, ushort>
    {
        [AlcoonAction.WaitingForSamus] = 0xdd71,
        [AlcoonAction.EmergingRising] = 0xddc6,
        [AlcoonAction.EmergingFalling] = 0xde05,
        [AlcoonAction.WalkingAndFiring] = 0xde4b,
        [AlcoonAction.WaitingForFireAnimation] = 0xdecc,
        [AlcoonAction.HidingRising] = 0xdecd,
        [AlcoonAction.HidingFalling] = 0xdeec,
    });
}

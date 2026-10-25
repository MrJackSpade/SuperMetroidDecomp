using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Verification's native view of the Chozo identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ChozoNativeWords
{
    /// <summary>The routine word naming each <see cref="ChozoStatuePreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<ChozoStatuePreInstruction> ChozoStatuePreInstructions = new(new Dictionary<ChozoStatuePreInstruction, ushort>
    {
        [ChozoStatuePreInstruction.Idle] = 0xe7a6,
        [ChozoStatuePreInstruction.Cleared] = 0x807b,
        [ChozoStatuePreInstruction.WaitForLowerNorfairHandTrigger] = 0xe445,
        [ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger] = 0xe7ae,
        [ChozoStatuePreInstruction.Walking] = 0xe7da,
    });
}

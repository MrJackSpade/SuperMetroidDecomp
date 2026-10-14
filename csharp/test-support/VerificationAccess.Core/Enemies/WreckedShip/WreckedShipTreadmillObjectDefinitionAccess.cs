
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Verification access to <see cref="WreckedShipTreadmillObjectDefinition"/> members production does not use.</summary>
internal static class WreckedShipTreadmillObjectDefinitionAccess
{
    /// <summary>Reads the four timed frame-control pointers for one Wrecked Ship treadmill direction.</summary>
    extension(WreckedShipTreadmillObjectDefinition self)
    {
        /// <summary>The four timed frame-control addresses in execution order.</summary>
        internal IReadOnlyList<ushort> FrameInstructionPointers =>
            self.frameInstructionPointers;
    }
}

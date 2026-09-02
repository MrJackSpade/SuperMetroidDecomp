namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A9 instruction-list entry points used by the Tourian Shitroid.</summary>
internal static class ShitroidInstructionLists
{
    /// <summary>Calm/normal animation target reached by instruction three at $A9:F920.</summary>
    public const ushort Normal = 0xf90e;
    /// <summary><c>InstList_BabyMetroid_LatchedOn</c> at $A9:F924.</summary>
    public const ushort LatchedOn = 0xf924;
    /// <summary>Departure/remorse animation target reached by instruction six.</summary>
    public const ushort Remorse = 0xf93a;
}

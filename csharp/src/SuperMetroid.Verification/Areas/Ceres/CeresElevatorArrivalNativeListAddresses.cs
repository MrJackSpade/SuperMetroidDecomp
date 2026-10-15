using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The native bank-$86 list each typed Ceres elevator arrival program replaces, for the #641 ROM oracle.</summary>
internal static class CeresElevatorArrivalNativeListAddresses
{
    /// <summary>Each program's native list: the pad's at $86:A28D, the platform's at $A299 and the shared delete at $A28B.</summary>
    internal static readonly (CeresElevatorArrivalProgram Program, ushort Entry)[] Entries =
    [
        (CeresElevatorArrivalProgram.Pad, 0xa28d),
        (CeresElevatorArrivalProgram.Platform, 0xa299),
        (CeresElevatorArrivalProgram.Delete, 0xa28b),
    ];
}

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A decoded frame or control transfer from the compiled bank-$86 program.</summary>
internal readonly record struct CeresElevatorProjectileInstruction(
    CeresElevatorProjectileOperation Operation,
    ushort NextInstruction = 0,
    ushort Duration = 0,
    ushort SpritemapPointer = 0);

using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye door's wakes.</summary>
internal static class EyeDoorPreInstructions
{
    /// <summary><c>$84:D753</c>: the eye wakes once its door's opened bit is set.</summary>
    internal static PlmPreInstruction WakeWhenDoorBitSet { get; } = Of(EyeDoorPreInstruction.WakeWhenDoorBitSet);

    /// <summary><c>$84:BD50</c> on an eye component: a missile hit wakes it, a Super Missile seeds its counter.</summary>
    internal static PlmPreInstruction MissileHit { get; } = Of(EyeDoorPreInstruction.MissileHit);

    /// <summary>The behavior object for <paramref name="routine"/>.</summary>
    private static PlmPreInstruction Of(EyeDoorPreInstruction routine) =>
        new($"eye door {routine}", (slot, context) => context.Plms.EyeDoors.RunEyeDoorPreInstruction(slot, routine));
}

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon's animations turned to a side: the same animation, or the body's attack or idle, facing left or right.</summary>
internal static class DragonPhases
{
    /// <summary>The same animation as <paramref name="animation"/>, facing left or right.</summary>
    internal static DragonSegment WithFacing(
        DragonSegment animation,
        bool facingLeft) => animation switch
    {
        DragonSegment.IdleFacingLeft or DragonSegment.IdleFacingRight => facingLeft
            ? DragonSegment.IdleFacingLeft
            : DragonSegment.IdleFacingRight,
        DragonSegment.WingsFacingLeft or DragonSegment.WingsFacingRight => facingLeft
            ? DragonSegment.WingsFacingLeft
            : DragonSegment.WingsFacingRight,
        DragonSegment.AttackingFacingLeft or DragonSegment.AttackingFacingRight => facingLeft
            ? DragonSegment.AttackingFacingLeft
            : DragonSegment.AttackingFacingRight,
        _ => throw NotLive(animation),
    };

    /// <summary>The body's attack, facing the way <paramref name="animation"/> faces.</summary>
    internal static DragonSegment AttackingWithSameFacing(
        DragonSegment animation) => FacesLeft(animation)
        ? DragonSegment.AttackingFacingLeft
        : DragonSegment.AttackingFacingRight;

    /// <summary>The body's idle pose, facing the way <paramref name="animation"/> faces.</summary>
    internal static DragonSegment IdleWithSameFacing(
        DragonSegment animation) => FacesLeft(animation)
        ? DragonSegment.IdleFacingLeft
        : DragonSegment.IdleFacingRight;

    /// <summary>Whether an animation faces left.</summary>
    private static bool FacesLeft(DragonSegment animation) => animation switch
    {
        DragonSegment.IdleFacingLeft or DragonSegment.WingsFacingLeft or
            DragonSegment.AttackingFacingLeft => true,
        DragonSegment.IdleFacingRight or DragonSegment.WingsFacingRight or
            DragonSegment.AttackingFacingRight => false,
        _ => throw NotLive(animation),
    };

    /// <summary>The failure for a value that is not one of Dragon's six animations.</summary>
    private static InvalidDataException NotLive(DragonSegment animation) =>
        new($"Dragon animation {animation} is not one of its six animations.");
}

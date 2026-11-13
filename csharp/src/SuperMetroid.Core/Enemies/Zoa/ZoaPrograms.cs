namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Chooses a Zoa's animation by its facing and movement (#1624), and the program each animation plays.</summary>
internal static class ZoaPrograms
{
    /// <summary>The program <paramref name="selector"/> plays.</summary>
    internal static ZoaProgram Program(ZoaAnimationSelector selector) =>
        ZoaAnimationDefinitions.Programs.TryGetValue(selector, out ZoaProgram program)
            ? program
            : throw new ArgumentOutOfRangeException(nameof(selector), selector, $"Undefined {nameof(ZoaAnimationSelector)}.");

    /// <summary>The rising animation facing right or left.</summary>
    internal static ZoaAnimationSelector Rising(bool facingRight) =>
        facingRight ? ZoaAnimationSelector.RisingFacingRight : ZoaAnimationSelector.RisingFacingLeft;

    /// <summary>The shooting animation with <paramref name="selector"/>'s facing.</summary>
    internal static ZoaAnimationSelector ShootingWithSameFacing(ZoaAnimationSelector selector) => selector switch
    {
        ZoaAnimationSelector.RisingFacingLeft or ZoaAnimationSelector.ShootingFacingLeft => ZoaAnimationSelector.ShootingFacingLeft,
        ZoaAnimationSelector.RisingFacingRight or ZoaAnimationSelector.ShootingFacingRight => ZoaAnimationSelector.ShootingFacingRight,
        _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, $"Undefined {nameof(ZoaAnimationSelector)}."),
    };
}

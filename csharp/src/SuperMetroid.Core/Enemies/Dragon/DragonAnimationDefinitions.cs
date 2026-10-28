namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Compiled fixed animation-program selectors and phase mappings for Dragon.</summary>
internal static class DragonAnimationDefinitions
{
    /// <summary>$A2:E5EF InstListPointers_Dragon: idle, wing and attacking body programs by facing.</summary>
    internal static readonly Dictionary<DragonAnimationSelector, DragonProgram> Programs = new()
    {
        [DragonAnimationSelector.IdleFacingLeft] = DragonProgram.IdleFacingLeft,
        [DragonAnimationSelector.IdleFacingRight] = DragonProgram.IdleFacingRight,
        [DragonAnimationSelector.WingsFacingLeft] = DragonProgram.WingsFacingLeft,
        [DragonAnimationSelector.WingsFacingRight] = DragonProgram.WingsFacingRight,
        [DragonAnimationSelector.AttackingFacingLeft] = DragonProgram.AttackingFacingLeft,
        [DragonAnimationSelector.AttackingFacingRight] = DragonProgram.AttackingFacingRight,
    };
}

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The updates on which a ninja Space Pirate considers its claw attack (#1606).</summary>
internal static class NinjaSpacePirateCadenceDefinitions
{
    /// <summary>
    /// The updates on which a ninja Space Pirate considers a claw attack: every 64th update of its own update count, on
    /// the period's first place.
    /// </summary>
    internal static readonly UpdateCadence ClawAttackEverySixtyFourthUpdate = new(64, 0);
}

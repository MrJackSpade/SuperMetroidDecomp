namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The updates on which a ninja Space Pirate considers its claw attack (#1606).</summary>
internal static class NinjaSpacePirateCadenceDefinitions
{
    /// <summary>
    /// The updates on which a ninja Space Pirate considers a claw attack, counted on its own update count at the period's
    /// first place.
    /// </summary>
    internal static readonly UpdateCadence ClawAttack = new(64, 0);
}

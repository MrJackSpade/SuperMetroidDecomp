namespace SuperMetroid.Core.Hud;

/// <summary>One energy tank, full or empty (#1466).</summary>
/// <param name="Tank">Whether the tank shows full or empty.</param>
public sealed record HudEnergyTankImage(HudEnergyTank Tank) : HudImage
{
    /// <inheritdoc/>
    public override string FrameName => $"Hud.EnergyTank.{Tank}";
}

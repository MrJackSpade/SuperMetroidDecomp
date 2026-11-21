namespace SuperMetroid.Core.Liquids;

/// <summary>A room's liquid, whose motion starts at the default every new motion holds.</summary>
public sealed class Liquid
{
    /// <summary>The liquid's motion: awaiting its first pass until <see cref="Begin"/>.</summary>
    public LiquidMotion Motion { get; private set; }

    /// <summary>Begins the motion.</summary>
    public void Begin() => Motion = LiquidMotion.Moving;
}

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>How a Fireflea flies (#2023): it bobs up and down at a speed, or circles at a turn per update.</summary>
internal abstract partial record FirefleaFlight
{
    /// <summary>Closes the hierarchy to the flights declared here.</summary>
    private FirefleaFlight()
    {
    }

    /// <summary>Bobs up and down about where it starts, reversing at either end.</summary>
    /// <param name="Speed">Its speed, in pixels per update.</param>
    /// <param name="StartDirection">The way it starts moving (#2369).</param>
    internal sealed partial record Bobbing(SignedSixteenSixteen Speed, VerticalDirection StartDirection) : FirefleaFlight;

    /// <summary>Circles about where it starts.</summary>
    /// <param name="TurnPerUpdate">How far, and which way, it turns around its circle each update.</param>
    internal sealed partial record Circling(AngularSpeed TurnPerUpdate) : FirefleaFlight;
}

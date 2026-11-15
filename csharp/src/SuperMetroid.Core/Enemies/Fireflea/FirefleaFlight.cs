namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>How a Fireflea flies (#2023): it bobs up and down at a speed, or circles at a turn per update.</summary>
internal abstract record FirefleaFlight
{
    /// <summary>Closes the hierarchy to the flights declared here.</summary>
    private FirefleaFlight()
    {
    }

    /// <summary>Bobs up and down about where it starts, reversing at either end.</summary>
    /// <param name="Speed">Its speed, in pixels per update.</param>
    /// <param name="StartSign">Whether it starts moving down, toward increasing Y, or up.</param>
    internal sealed record Bobbing(SignedSixteenSixteen Speed, EnemySpeedSign StartSign) : FirefleaFlight;

    /// <summary>Circles about where it starts.</summary>
    /// <param name="TurnPerUpdate">The turn it makes around its circle each update.</param>
    internal sealed record Circling(RotationAngle TurnPerUpdate) : FirefleaFlight;
}

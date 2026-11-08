namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How far the Speed Booster escape has progressed: the lava rises Samus has triggered, in
/// <see cref="SpeedBoosterEscapeStageDefinitions.Stages"/> order. Setup starts it before the first rise; the lava's advance
/// passes one rise per update at most, and once every rise has passed the escape is over.
/// </summary>
internal sealed class SpeedBoosterEscapeObjectState : RoomObjectFamilyState
{
    /// <inheritdoc/>
    internal override RoomObjectFamily Family => RoomObjectFamily.SpeedBoosterEscape;

    /// <summary>How many of the escape's rises Samus has triggered, 0 through the number of rises.</summary>
    internal int RisesPassed { get; private set; }

    /// <summary>Whether Samus has triggered every rise, which ends the escape.</summary>
    internal bool IsEscaped => RisesPassed == SpeedBoosterEscapeStageDefinitions.Stages.Length;

    /// <summary>The rise Samus triggers next.</summary>
    /// <exception cref="InvalidOperationException">The escape is over and has no rise left.</exception>
    internal SpeedBoosterEscapeStageDefinition NextRise => IsEscaped
        ? throw new InvalidOperationException("The Speed Booster escape is over and has no rise left.")
        : SpeedBoosterEscapeStageDefinitions.Stages[RisesPassed];

    /// <summary>Records that Samus triggered <see cref="NextRise"/>.</summary>
    /// <exception cref="InvalidOperationException">The escape is over and has no rise left.</exception>
    internal void PassRise()
    {
        if (IsEscaped)
            throw new InvalidOperationException("The Speed Booster escape is over and has no rise left to pass.");
        RisesPassed++;
    }
}

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's body runs while he grows into his second phase (<see cref="KraidBodyStage.Growing"/>).</summary>
public enum KraidGrowthPhase
{
    /// <summary>Releases the room camera restrictions for the enlarged second phase.</summary>
    ReleaseCamera,
    /// <summary>Breaks ceiling platforms into their authored room mutations as Kraid grows.</summary>
    BreakCeilingPlatforms,
    /// <summary>Brings Kraid's grown body in front of the room's sprites, shows it whole and lets his arm touch Samus.</summary>
    BringBodyInFront,
    /// <summary>Settles his grown parts: his foot stands neutral and his lints take their grown pose.</summary>
    SettleParts,
    /// <summary>Shows the room's backdrop behind the grown Kraid and starts its fade in from the first step.</summary>
    ShowRoomBackdrop,
    /// <summary>Fades the room background into the enlarged encounter.</summary>
    FadeInRoomBackground,
}

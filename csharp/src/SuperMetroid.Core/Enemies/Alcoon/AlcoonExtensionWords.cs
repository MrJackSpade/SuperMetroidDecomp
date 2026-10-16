namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class AlcoonExtensionWords
{
    /// <summary>Whole halves of vertical acceleration for native WRAM compatibility.</summary>
    internal ushort YAcceleration { get; set; }

    /// <summary>Fractional halves of vertical acceleration.</summary>
    internal ushort YSubacceleration { get; set; }

    /// <summary>Spawn X coordinates used to decide when Alcoon hides again.</summary>
    internal ushort SpawnXPosition { get; set; }

    /// <summary>Predicted floor Y coordinates used for the activation-height check.</summary>
    internal ushort LandingYPosition { get; set; }

    /// <summary>Walking steps remaining before Alcoon may fire at Samus.</summary>
    internal ushort StepCounter { get; set; }
}

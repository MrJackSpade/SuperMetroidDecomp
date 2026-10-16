namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Platforms words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class PlatformsExtensionWords
{
    /// <summary>Native horizontal and vertical dispatch indices.</summary>
    internal PlatformMovementFunctions MovementFunction { get; set; }

    /// <summary>Pre-move whole position used to carry Samus by the platform's accepted displacement.</summary>
    internal ushort PreviousPosition { get; set; }

    /// <summary>Whether a moving vertical animation has been installed.</summary>
    internal ushort VerticallyMovingFlag { get; set; }

    /// <summary>Whether a stationary vertical animation has been installed.</summary>
    internal ushort VerticallyStillFlag { get; set; }

    /// <summary>Quadratic-speed table caps selected by the platform variant.</summary>
    internal ushort MaximumYSpeedTableIndex { get; set; }

    /// <summary>Previous vertical dispatcher words used when rise and sink states change.</summary>
    internal ushort PreviousYMovementFunction { get; set; }

    /// <summary>selector for the suspensor platform animation family.</summary>
    internal ushort SuspensorPlatformFlag { get; set; }
}

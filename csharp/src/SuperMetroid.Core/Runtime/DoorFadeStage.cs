namespace SuperMetroid.Core.Runtime;

/// <summary>Where a door's fade is (#1512).</summary>
internal enum DoorFadeStage
{
    /// <summary>No door is fading: the room shows its own colours.</summary>
    None,

    /// <summary>The room being left fades out to black.</summary>
    FadingOut,

    /// <summary>The room has faded out, and stays dark while the door loads and scrolls.</summary>
    Dark,

    /// <summary>The room being entered fades in from black.</summary>
    FadingIn,
}

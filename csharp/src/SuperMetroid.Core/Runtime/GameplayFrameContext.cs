namespace SuperMetroid.Core.Runtime;

/// <summary>The values one gameplay frame's phases hand to later phases of the same frame.</summary>
/// <param name="samusCameraPointAtFrameStart">Samus's position sampled before the frame's movement.</param>
internal sealed class GameplayFrameContext(FixedPosition<RoomPixels>? samusCameraPointAtFrameStart)
{
    /// <summary>Samus's position sampled before the frame's movement; the camera fallback for a new camera.</summary>
    internal FixedPosition<RoomPixels>? SamusCameraPointAtFrameStart { get; } = samusCameraPointAtFrameStart;

    /// <summary>Samus's control for this frame: her modes, who moves her and what her input and movement phases sampled.</summary>
    internal Samus.SamusControlFrame Samus { get; } = new();

    /// <summary>The tail a room object's message box suspends; set once Samus's section has run.</summary>
    internal SuperMetroidRuntime.SuspendedGameplayFrameTail? Tail { get; set; }
}

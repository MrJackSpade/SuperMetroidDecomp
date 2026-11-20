namespace SuperMetroid.Core.Fx;

/// <summary>What one kind of room effect does (<see cref="RoomFxTypeDefinitions.Types"/>).</summary>
/// <param name="Overlay">What the effect shows on the effect overlay and how it moves there.</param>
/// <param name="DarkensWithFirefleas">Whether the effect starts the Firefleas' darkness.</param>
/// <param name="BubblesAmbiently">Whether the effect's liquid has its ambient bubbling sound: lava's.</param>
/// <param name="WavesBackground">Whether the effect's water moves the background's wave as well as its surface.</param>
/// <param name="SpawnsSkyBands">Whether the effect spawns the scrolling sky's drifting bands.</param>
[IndependentFlag("independent facts of one effect kind: whether it starts the Firefleas' darkness, bubbles, waves the background and spawns the sky's bands")]
internal sealed record RoomFxTypeDefinition(RoomFxOverlayMotion Overlay, bool DarkensWithFirefleas = false, bool BubblesAmbiently = false,
    bool WavesBackground = false, bool SpawnsSkyBands = false);

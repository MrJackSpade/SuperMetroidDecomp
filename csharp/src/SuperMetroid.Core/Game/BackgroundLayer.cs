namespace SuperMetroid.Core.Game;

/// <summary>Which native tilemap producer must satisfy a streaming request.</summary>
public enum BackgroundLayer
{
    /// <summary>The room's collision-backed level tilemap rendered on BG1.</summary>
    Level,

    /// <summary>The independently streamed room-background tilemap rendered on BG2.</summary>
    Background,
}

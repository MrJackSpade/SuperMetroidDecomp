namespace SuperMetroid.Core.Game;

/// <summary>One timer/hack-handler pass at `$90:E2A1`.</summary>
public readonly record struct DraygonEscapeResult(
    bool SuppressProspectivePose,
    bool Released);

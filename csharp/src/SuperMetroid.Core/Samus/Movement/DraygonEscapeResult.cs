namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One escape check of Draygon's hold, run after Samus's movement.</summary>
[IndependentFlag("independent outcomes of one escape step: the prospective pose is suppressed, and Samus is released")]
public readonly record struct DraygonEscapeResult(
    bool SuppressProspectivePose,
    bool Released);

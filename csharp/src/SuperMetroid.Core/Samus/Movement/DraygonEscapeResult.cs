namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One timer/hack-handler pass of the Draygon escape.</summary>
[IndependentFlag("independent outcomes of one escape step: the prospective pose is suppressed, and Samus is released")]
public readonly record struct DraygonEscapeResult(
    bool SuppressProspectivePose,
    bool Released);

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One timer/hack-handler pass at `$90:E2A1`.</summary>
[IndependentFlag("independent outcomes of one escape step: the prospective pose is suppressed, and Samus is released")]
public readonly record struct DraygonEscapeResult(
    bool SuppressProspectivePose,
    bool Released);

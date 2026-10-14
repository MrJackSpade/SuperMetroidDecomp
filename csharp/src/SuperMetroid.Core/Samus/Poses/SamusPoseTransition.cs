namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Debugger-readable winning six-byte transition-table record.</summary>
public readonly record struct SamusPoseTransition(
    SamusPoseId ProspectivePose);

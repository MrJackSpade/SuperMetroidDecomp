namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Mutually exclusive escape-warning identities, independent of the editable English wording.</summary>
public enum EscapeTypewriterProgramId
{
    /// <summary>No installed warning identity; used by a typewriter initialized only with a native text pointer and rejected by presentation lookup.</summary>
    None,
    /// <summary>Ceres colony self-destruct warning, native <c>TypewriterText_CeresEscapeTimer</c> at <c>$A6:C450</c>, following Ridley's retreat.</summary>
    Ceres,
    /// <summary>Zebes time-bomb warning, native <c>TypewriterText_ZebesEscapeTimer</c> at <c>$A6:C49C</c>, following Mother Brain's defeat.</summary>
    Zebes,
}

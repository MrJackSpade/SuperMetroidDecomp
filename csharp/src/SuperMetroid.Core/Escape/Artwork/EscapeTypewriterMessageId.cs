namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Mutually exclusive escape-warning identities, independent of the editable English wording.</summary>
public enum EscapeTypewriterMessageId
{
    /// <summary>No installed warning identity; used by a typewriter initialized only with its text and rejected by presentation lookup.</summary>
    None,
    /// <summary>Ceres colony self-destruct warning, following Ridley's retreat.</summary>
    Ceres,
    /// <summary>Zebes time-bomb warning, following Mother Brain's defeat.</summary>
    Zebes,
}

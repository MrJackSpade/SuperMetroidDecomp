namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Stable visual bindings for native equipment selectors; no editable navigation table.</summary>
public static class PauseSelectorDefinitions
{
    /// <summary>Supported revision of the equipment-selector anchor, composition, animation, and palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename for the editable pause-equipment selector presentation.</summary>
    public const string FileName = "pause-selectors.json";
    /// <summary>Presentation limits: sixteen required equipment anchors, at most 255 named visual frames and 255 cyclic phases, and initial/phase dwell times from 1 through 254 accepted update ticks.</summary>
    public const int AnchorCount = 16, MaximumFrames = 255, MaximumPhases = 255, MaximumDuration = 254;

    /// <summary>$46: the pause cursor's single cell.</summary>
    internal const int CursorCell = 0x46;

    /// <summary>$5B: a selector grid's top-left corner cell.</summary>
    internal const int GridTopLeftCell = 0x5b;

    /// <summary>$5C: a selector grid's interior and left-edge cell.</summary>
    internal const int GridCell = 0x5c;

    /// <summary>$5D: a selector grid's top-right corner cell.</summary>
    internal const int GridTopRightCell = 0x5d;

    /// <summary>$5E: a selector grid's right-edge cell below the top row.</summary>
    internal const int GridRightCell = 0x5e;
}

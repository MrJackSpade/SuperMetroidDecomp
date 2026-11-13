namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The seven ways the X-ray scope reveals a block, as the X-ray reveal tables name them.</summary>
public enum XrayRevealShape
{
    /// <summary>A vertical extension: reveals as the block it extends does.</summary>
    VerticalExtension,
    /// <summary>A horizontal extension: reveals as the block it extends does.</summary>
    HorizontalExtension,
    /// <summary>Reveals one block.</summary>
    CopyOne,
    /// <summary>Reveals one block, in Brinstar only.</summary>
    CopyBrinstar,
    /// <summary>Reveals two blocks side by side.</summary>
    CopyWide,
    /// <summary>Reveals two blocks one above the other.</summary>
    CopyTall,
    /// <summary>Reveals a square of four blocks.</summary>
    CopySquare,
}

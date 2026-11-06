namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The seven revealed-block commands named by the X-ray reveal tables.</summary>
public enum XrayRevealCommand
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

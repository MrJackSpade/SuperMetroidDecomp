namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The five X-ray reveal shapes that copy blocks (#2553): each shows the blocks it copies, as its visual lays them out; the
/// two extension shapes traverse an extension instead and copy none (<see cref="XrayRevealShape"/>).
/// </summary>
public enum XrayRevealCopyShape
{
    /// <summary>Copies one block.</summary>
    CopyOne,

    /// <summary>Copies one block, as Brinstar's own copy does.</summary>
    CopyBrinstar,

    /// <summary>Copies two blocks side by side.</summary>
    CopyWide,

    /// <summary>Copies two blocks one above the other.</summary>
    CopyTall,

    /// <summary>Copies a square of four blocks.</summary>
    CopySquare,
}

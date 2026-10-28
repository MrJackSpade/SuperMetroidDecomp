namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// What the bank-$94 grapple block-reaction dispatcher reports for the block at the beam's endpoint. The cartridge
/// returns it as carry (a collision) and overflow (a connection); carry clear with overflow set is never produced.
/// </summary>
internal enum GrappleBlockReaction
{
    /// <summary>The beam passes through (carry clear).</summary>
    Passes,

    /// <summary>The block is solid to the beam, which stops it (carry set, overflow clear).</summary>
    Solid,

    /// <summary>The beam latches onto the block (carry and overflow set).</summary>
    Connects,
}

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// What the grapple block reaction reports for the block at the beam's endpoint: the beam passes, stops, or latches.
/// A block that latches the beam always stops it too; a latch without a stop is never produced.
/// </summary>
internal enum GrappleBlockReaction
{
    /// <summary>The beam passes through.</summary>
    Passes,

    /// <summary>The block is solid to the beam, which stops it without latching.</summary>
    Solid,

    /// <summary>The beam latches onto the block, which also stops it.</summary>
    Connects,
}

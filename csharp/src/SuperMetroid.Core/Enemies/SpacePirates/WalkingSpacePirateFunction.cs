namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Literal 16-bit function identities stored in walking Space Pirate variable A. These are
/// addresses inside bank $B2, not host-selected behavior labels; exposing the raw identities
/// makes an instruction-list breakpoint directly comparable with the cartridge state.
/// </summary>
public enum WalkingSpacePirateFunction : ushort
{
    /// <summary>The shared bank-$B2 RTS copied by initialization and flinch lists.</summary>
    NoOperation = 0x804b,

    /// <summary>Walk left, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingLeft = 0xfd44,

    /// <summary>Walk right, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingRight = 0xfdce,

    /// <summary>The second RTS used while a firing/look-around list owns presentation.</summary>
    AnimationOwnedNoOperation = 0xfe4a,
}

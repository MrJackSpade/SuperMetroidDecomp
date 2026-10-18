using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One semantic view over a physical WRAM bomb slot.</summary>
public sealed class SamusBombProjectileSlot
{
    /// <summary>Creates one view over the specified logical index in the five-slot bomb half.</summary>
    internal SamusBombProjectileSlot(int index) => Index = index;

    /// <summary>Logical bomb index zero through four; physical projectile index is 5+Index.</summary>
    public int Index { get; }

    /// <summary>
    /// Room-pixel 16.16 position: whole words <c>SamusProjectile_X/YPositions</c> at
    /// <c>$0B6E/$0B82 + 2*Index</c>, subpixels <c>SamusProjectile_X/YSubPositions</c> at
    /// <c>$0B96/$0BAA + 2*Index</c>.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Raw native <c>SamusProjectile_BombDirections</c> word at <c>$0C0E + 2*Index</c>; any <c>$F0</c> kill-request bit clears the slot before its pre-instruction, and the <c>$10</c> collision marker also excludes Samus overlap.</summary>
    public int Direction { get; internal set; }
    /// <summary>Packed native projectile family/type word at <c>$0C22 + 2*Index</c>, preserving bomb, Power Bomb, spread, and status bits; use <see cref="PackedType"/> for semantic decoding.</summary>
    public int Type { get; internal set; }
    /// <summary>Lossless semantic view of the native projectile family/type word.</summary>
    public SamusProjectileTypeWord PackedType => new(checked((ushort)Type));
    /// <summary>Lossless semantic view of the native direction/lifecycle word.</summary>
    public SamusProjectileDirectionWord PackedDirection => new(checked((ushort)Direction));
    /// <summary>Native damage word at <c>$0C36 + 2*Index</c>, initialized from bank-$93 projectile data; zero excludes the slot from damage-bearing overlap tests.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Where the slot's projectile program resumes; null marks an inactive slot, and a consumed timed frame advances it past that record.</summary>
    internal AnimationProgramCursor<SamusProjectileProgram>? Cursor { get; set; }
    /// <summary>Wrapping sixteen-bit duration countdown at <c>$0C5E + 2*Index</c>, decremented once per unfrozen projectile update; reaching zero runs the next bank-$93 instruction.</summary>
    public NativeCountdown InstructionTimer { get; internal set; }
    /// <summary>Current bank-$93 spritemap identity at <c>$0CC2 + 2*Index</c>, selected by the timed-frame interpreter or installed frame binding and resolved through the sprite catalog when drawing.</summary>
    public int SpritemapPointer { get; internal set; }
    /// <summary>Collision half-extents in whole pixels, loaded from timed-frame bytes and stored in native <c>SamusProjectile_X/YRadii</c> at <c>$0BBE/$0BD2 + 2*Index</c>; not the global Power Bomb blast radius.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }

    /// <summary>Whether bank-$90 installed <c>ProjectilePreInstruction_BombSpread</c>.</summary>
    public bool IsBombSpread { get; internal set; }

    /// <summary>The spread bomb's horizontal travel; a wall reverses its direction.</summary>
    public BombSpreadHorizontalVelocity BombSpreadHorizontal { get; internal set; }

    /// <summary>
    /// The spread bomb's horizontal travel in its launch definition's packed form, 0 through 65535, for traces that
    /// print it.
    /// </summary>
    public int BombSpreadDirectionMagnitude => BombSpreadHorizontal.LaunchWord;

    /// <summary>The spread bomb's signed 16.16 vertical velocity; Samus's gravity accelerates it every frame.</summary>
    public SnesSignedSixteenSixteen BombSpreadVerticalVelocity { get; internal set; }

    /// <summary>The whole word of <see cref="BombSpreadVerticalVelocity"/>, 0 through 65535, for traces that print it.</summary>
    public int BombSpreadYVelocity => BombSpreadVerticalVelocity.WholeWord;

    /// <summary>The fraction word of <see cref="BombSpreadVerticalVelocity"/>, 0 through 65535, for traces that print it.</summary>
    public int BombSpreadYSubvelocity => BombSpreadVerticalVelocity.Fraction;

    /// <summary>The upward launch velocity, signed 16.16, that a floor bounce restores.</summary>
    public SnesSignedSixteenSixteen BombSpreadLaunchVelocity { get; internal set; }

    /// <summary>
    /// The word called <c>projectile_variables[5+Index]</c> by bank $90 and
    /// <c>bomb_timers[Index]</c> by bank $A0 due to the original overlapping arrays.
    /// </summary>
    public NativeCountdown BombTimer { get; internal set; }

    /// <summary>Instruction pointer is the native active-slot sentinel.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>Zeroes the complete per-slot native projectile state before reuse or deletion.</summary>
    internal void ClearFields()
    {
        Position = FixedPosition<RoomPixels>.Zero;
        Direction = 0;
        Type = 0;
        Damage = 0;
        Cursor = null;
        InstructionTimer = NativeCountdown.Start(0);
        SpritemapPointer = 0;
        Radius = Extent16<RoomPixels>.Zero;
        BombTimer = NativeCountdown.Start(0);
        IsBombSpread = false;
        BombSpreadHorizontal = default;
        BombSpreadVerticalVelocity = SnesSignedSixteenSixteen.Zero;
        BombSpreadLaunchVelocity = SnesSignedSixteenSixteen.Zero;
    }
}

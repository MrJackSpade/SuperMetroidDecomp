using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One of Samus's five bomb slots: a placed bomb, a Power Bomb, or a bomb-spread bomb.</summary>
public sealed class SamusBombProjectileSlot
{
    /// <summary>Creates the bomb slot at its position among the five.</summary>
    internal SamusBombProjectileSlot(int index) => Index = index;

    /// <summary>The slot's position among the bomb slots, zero through four.</summary>
    public int Index { get; }

    /// <summary>Room-pixel 16.16 position.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>
    /// Whether the bomb has already struck an enemy: its explosion keeps running, but it no longer launches Samus and
    /// later enemy collision skips it.
    /// </summary>
    public bool IsSpent { get; internal set; }

    /// <summary>
    /// The direction the shared projectile reactions read for a bomb. Bombs are placed without an aim, so they read as
    /// fired upward facing right.
    /// </summary>
    public static SamusProjectileDirection Direction => SamusProjectileDirection.UpFacingRight;

    /// <summary>What the slot holds; <see cref="SamusProjectileType.None"/> marks an empty slot.</summary>
    public SamusProjectileType Type { get; internal set; }

    /// <summary>Whether a placed bomb's explosion has reacted with the five-block cross of terrain around it; it does so once.</summary>
    public bool HasReactedToTerrain { get; internal set; }

    /// <summary>Damage from the bomb's projectile data; zero excludes the slot from damage-bearing overlap tests.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Where the slot's projectile program resumes; null marks an inactive slot, and a consumed timed frame advances it past that record.</summary>
    internal AnimationProgramCursor<SamusProjectileProgram>? Cursor { get; set; }
    /// <summary>Wrapping duration countdown, decremented once per unfrozen projectile update; reaching zero runs the next program instruction.</summary>
    public WrappingCountdown InstructionTimer { get; internal set; }
    /// <summary>The composition the bomb shows, selected by the timed-frame interpreter or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Collision half-extents in whole pixels, loaded from the timed frames; not the global Power Bomb blast radius.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }

    /// <summary>Whether the bomb is one of a bomb spread's five.</summary>
    public bool IsBombSpread { get; internal set; }

    /// <summary>The spread bomb's horizontal travel; a wall reverses its direction.</summary>
    public BombSpreadHorizontalVelocity BombSpreadHorizontal { get; internal set; }

    /// <summary>The spread bomb's signed 16.16 vertical velocity; Samus's gravity accelerates it every frame.</summary>
    public SignedSixteenSixteen BombSpreadVerticalVelocity { get; internal set; }

    /// <summary>The upward launch velocity, signed 16.16, that a floor bounce restores.</summary>
    public SignedSixteenSixteen BombSpreadLaunchVelocity { get; internal set; }

    /// <summary>The bomb's fuse, and after detonation its explosion phase counter.</summary>
    public WrappingCountdown BombTimer { get; internal set; }

    /// <summary>A live program marks the slot active.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>Clears the slot's state before reuse or deletion.</summary>
    internal void ClearFields()
    {
        Position = FixedPosition<RoomPixels>.Zero;
        IsSpent = false;
        Type = SamusProjectileType.None;
        HasReactedToTerrain = false;
        Damage = 0;
        Cursor = null;
        InstructionTimer = WrappingCountdown.Start(0);
        Sprite = null;
        Radius = Extent16<RoomPixels>.Zero;
        BombTimer = WrappingCountdown.Start(0);
        IsBombSpread = false;
        BombSpreadHorizontal = default;
        BombSpreadVerticalVelocity = SignedSixteenSixteen.Zero;
        BombSpreadLaunchVelocity = SignedSixteenSixteen.Zero;
    }
}

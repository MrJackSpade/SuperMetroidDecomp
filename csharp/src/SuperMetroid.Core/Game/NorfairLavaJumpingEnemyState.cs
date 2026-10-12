namespace SuperMetroid.Core.Game;

/// <summary>
/// Exact bank-$A2 function words stored in variable F by the Norfair lava-jumping enemy.
/// The retail source is commonly labelled <c>NorfairLavajumpingEnemy</c>; the ROM name
/// record spells the internal name <c>SQUEEWPT</c>.
/// </summary>
public enum NorfairLavaJumpingEnemyFunction : ushort
{
    /// <summary>$A2:BEDC, Function_Squeept_Flame: the immediately following cosmetic slot copies parent freeze/Y state, is visible only during unfrozen ascent, and deletes itself when parent health is zero.</summary>
    FollowParent = 0xbedc,
    /// <summary>$A2:BF1A, Function_Squeept_Jump: choose one of four signed 8.8 launch velocities from the current RNG word, enable offscreen processing, and request library-two jump sound $0D.</summary>
    BeginJump = 0xbf1a,
    /// <summary>$A2:BF3E, Function_Squeept_Rising: integrate Y and add NTSC gravity $0038 (56/256 pixel per update squared); unsigned velocity reaching $FC00 starts the flip list while still ascending.</summary>
    RiseBeforeAnimationSwitch = 0xbf3e,
    /// <summary>$A2:BF7C, Function_Squeept_Flipping: continue the signed 8.8 arc until animation instruction $BE8E signals completion, then clear the handshake and enter final falling.</summary>
    MoveUntilAnimationSignal = 0xbf7c,
    /// <summary>$A2:BFBC, Function_Squeept_Falling: continue descent until Y bits 4..7 equal $F, then restore spawn whole coordinates, install the hidden/rising list, and disable offscreen processing without clearing subpositions.</summary>
    FallBackIntoLava = 0xbfbc,
}

/// <summary>
/// Typed projection of the common C-F enemy words and the family's two extra-RAM words used
/// by $A2:BE99-$C02B. Common values remain in the physical slot; only the two words which
/// genuinely live in family-extra WRAM receive host fields.
/// </summary>
public sealed class NorfairLavaJumpingEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal NorfairLavaJumpingEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Native family-extra variable $00. Instruction $BE8E sets this handshake after the
    /// jump animation reaches the frame where main AI may begin the final falling phase.
    /// It is deliberately host-backed: this word lives outside the six common A-F words.
    /// </summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>
    /// Native family-extra variable $01. Function six remembers the last installed list so
    /// it does not restart an animation which is already running. Like variable $00, this
    /// is separate from the ordinary A-F words and therefore has explicit host storage.
    /// </summary>
    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>Signed 8.8 vertical velocity, stored in native enemy variable C.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Parent variant's immutable population position (Y at the lava surface), stored in native
    /// enemy variables D (X) and E (Y).
    /// </summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => new(_slot.VariableD, _slot.VariableE);
        internal set
        {
            _slot.VariableD = value.X;
            _slot.VariableE = value.Y;
        }
    }

    /// <summary>Same-bank dispatch word in common variable F: parent slots traverse the four jump phases, while parameter-one bit 15 selects the persistent follower function; every main call advances RNG before dispatch.</summary>
    public NorfairLavaJumpingEnemyFunction Function
    {
        get => (NorfairLavaJumpingEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>
    /// Population parameter-one bit 15 selects the visual follower half. It is not a
    /// freely combinable runtime flag, so expose it as a predicate rather than an enum.
    /// </summary>
    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}

using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// The role encoded by Magdollite population parameter one. Retail always allocates these
/// records consecutively as head, rising body, then tracking overlay. The names describe
/// ownership; they do not collapse the three physical enemy slots into one host actor.
/// </summary>
public enum MagdollitePart : ushort
{
    /// <summary>Population parameter 0: the collision-enabled head/master that chooses facing and initiates the three-slot attack.</summary>
    Head = 0,
    /// <summary>Population parameter 1: the following arm/slave-1 slot, which grows and shrinks the vertical pillar.</summary>
    RisingBody = 1,
    /// <summary>Population parameter 2: the hand/slave-2 slot, which follows the pillar cap, animates the throw, and spawns lava projectiles.</summary>
    TrackingOverlay = 2,
}

/// <summary>Bank-$A8 function pointers stored in native Magdollite variable F.</summary>
public enum MagdolliteEnemyFunction : ushort
{
    /// <summary>$A8:B11A, Function_Magdollite_WaitForSamusToGetNear: faces Samus and submerges when the hand cooldown has expired and Samus is within the configured horizontal range.</summary>
    HeadWaiting = 0xb11a,
    /// <summary>$A8:B175, Function_Magdollite_WaitUntilBasePillarFormed: waits for the head's submerge animation to finish, then releases the arm to grow.</summary>
    HeadWaitingForAttackAnimation = 0xb175,
    /// <summary>$A8:B193, Function_Magdollite_WaitForOtherPartsToFinish: waits for the arm to land, then selects the facing-dependent head emergence animation.</summary>
    HeadWaitingForBodyLanding = 0xb193,
    /// <summary>$A8:B1B8, Function_Magdollite_UnformBasePillarBackToHeadIdling: waits for emergence to finish before returning the head to idle.</summary>
    HeadWaitingForReturnAnimation = 0xb1b8,
    /// <summary>$A8:B1DD, Function_Magdollite_Slave1_WaitForAttackToBeTriggered: holds the arm dormant until the head clears its not-emerged flag, then initializes growth at its spawn Y.</summary>
    BodyDormant = 0xb1dd,
    /// <summary>$A8:B204, Function_Magdollite_Slave1_HandlePillarGrowth: adds the preceding head's upward 16.16 velocity and extends pillar segments until reaching Samus's height or the maximum rise.</summary>
    BodyRising = 0xb204,
    /// <summary>$A8:B291, Function_Magdollite_Slave1_GetEnemyIndex: native index-load/return stub that holds the grown arm stationary while the hand throws.</summary>
    NoOpAtApex = 0xb291,
    /// <summary>$A8:B295, Function_Magdollite_Slave1_HandlePillarShrinking: adds the head's downward 16.16 velocity, removes pillar segments, and releases the head when the arm lands.</summary>
    BodyFalling = 0xb295,
    /// <summary>$A8:B30D, Function_Magdollite_Slave2_Idling_WaitingForTrigger: waits for the head to begin forming the base pillar before tracking arm growth.</summary>
    OverlayWaitingForAttack = 0xb30d,
    /// <summary>$A8:B31F, Function_Magdollite_Slave2_SetSlave1ToShrinkAfterFireballs: waits for the throw animation to finish, restores the captured hand position, and starts arm shrinkage.</summary>
    OverlayWaitingForThrowAnimation = 0xb31f,
    /// <summary>$A8:B356, Function_Magdollite_Slave2_SetToThrowFireballsAfterGrowing: follows the growing arm cap and, at the apex, selects a Samus-facing throw animation and captures its origin.</summary>
    OverlayTrackingRisingBody = 0xb356,
    /// <summary>$A8:B3A7, Function_Magdollite_Slave2_GoBackToIdlingAfterPillarShrinks: follows the shrinking arm cap until the head returns to idle.</summary>
    OverlayTrackingFallingBody = 0xb3a7,
}

/// <summary>
/// Typed projection of Magdollite's common slot words and bank-$7E:7800 extension. The
/// ordinary A-F words remain backed by <see cref="RoomEnemySlot"/> so debugger values match
/// the cartridge. Extension words are named here instead of reproducing native's deliberately
/// obscure drawing-queue/spawn-snapshot aliases between three neighboring records.
/// </summary>
public sealed class MagdolliteEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MagdolliteEnemyState(RoomEnemySlot slot, MagdollitePart part)
    {
        _slot = slot;
        Part = part;
    }

    /// <summary>The immutable head, arm, or hand role decoded from this physical enemy slot's population parameter one.</summary>
    public MagdollitePart Part { get; }
    /// <summary>Native variable F ($0FB2 plus the slot's byte index): the current bank-$A8 AI function pointer.</summary>
    public MagdolliteEnemyFunction Function
    {
        get => (MagdolliteEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Native variable B: the arm's even byte offset into the nine-entry rise tables, aliased by the hand as throw direction 0 for left or 1 for right.</summary>
    public ushort BodyPhaseOffset
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: last instruction list actually installed.</summary>
    public ushort InstalledInstructionList
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: desired instruction list.</summary>
    public ushort DesiredInstructionList
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: signed whole-pixel displacement accumulated by the body.</summary>
    public ushort VerticalWholeDisplacement
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native headDirection at extension offset $00: main AI uses 0 for left and 1 for right; head initialization briefly uses the opposite convention.</summary>
    public ushort FacingMarker { get; internal set; }
    /// <summary>Native animationActiveFlag at extension offset $02, set and cleared by instruction callbacks to gate head transitions and completion of the hand's throw.</summary>
    public bool AnimationBusy { get; internal set; }
    /// <summary>Native emergeNotReadyFlag at extension offset $04: set by the head after submerging and cleared by the arm on landing so the head may emerge.</summary>
    public bool HeadWaitsForBodyLanding { get; internal set; }
    /// <summary>Native YSpawnPosition at extension offset $06, captured in whole room pixels before submerging offsets and used to reset the head and arm.</summary>
    public ushort OriginY { get; internal set; }

    /// <summary>
    /// Native extension variable four. Only the overlay's instance is polled by the head;
    /// decrementing it in every part remains observable and matches main AI exactly.
    /// The wrapping word counts enemy updates, is reset to $0100 by the throw list,
    /// and expires when the head observes its sign bit after decrementing through zero.
    /// </summary>
    public NativeCountdown AttackTimer { get; internal set; }

    /// <summary>Native downSubVelocity at extension offset $0E: unsigned fractional word of downward 16.16 pixels per enemy update; the arm consumes the head's copy.</summary>
    public ushort PositiveSpeedFraction { get; internal set; }
    /// <summary>Native downVelocity at extension offset $10: signed whole-pixel word of the downward 16.16 velocity, stored as raw unsigned bits.</summary>
    public ushort PositiveSpeedWhole { get; internal set; }
    /// <summary>Native upSubVelocity at extension offset $12: fractional word of upward 16.16 pixels per enemy update, imported independently from the negative speed-table record.</summary>
    public ushort NegativeSpeedFraction { get; internal set; }
    /// <summary>Native upVelocity at extension offset $14: signed whole-pixel word of the upward 16.16 velocity, stored as raw unsigned bits; the arm consumes the head's copy.</summary>
    public ushort NegativeSpeedWhole { get; internal set; }
    /// <summary>Native finishedGrowingFlag at extension offset $18, set by the arm at maximum or Samus-relative height and cleared by the hand when shrinking begins.</summary>
    public bool BodyReachedApex { get; internal set; }
    /// <summary>Native notEmergedFlag at extension offset $1A: despite the host name, true holds the arm dormant; the head clears it to start growth and landing sets it again.</summary>
    public bool BodyWakeRequested { get; internal set; }
    /// <summary>The hand's initial whole-pixel room X, captured by $A8:B02D at native extension offset $22, separately from its later throw origin.</summary>
    public ushort InitializationOriginX { get; internal set; }
    /// <summary>Native throwXPosition/throwYPosition at extension offsets $24/$26: whole-pixel room position captured at the apex and used as the base for hand-animation shifts and restoration.</summary>
    public Position16<RoomPixels> ThrowOrigin { get; internal set; }
}

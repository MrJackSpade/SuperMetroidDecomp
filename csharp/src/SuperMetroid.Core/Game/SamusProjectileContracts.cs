namespace SuperMetroid.Core.Game;

/// <summary>One debugger-readable ordinary slot corresponding to a native even byte index.</summary>
public sealed class SamusProjectileSlot
{
    /// <summary>Creates the host view for one ordinary projectile slot and records its native slot index.</summary>
    /// <param name="slotIndex">Zero-based host index; the corresponding native byte index is twice this value.</param>
    internal SamusProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Host slot 0..4; native byte index is this value times two.</summary>
    public int SlotIndex { get; }

    /// <summary>Native byte index used by the parallel WRAM arrays.</summary>
    public int NativeByteIndex => SlotIndex * 2;

    /// <summary>Damage doubles as the producer's free-slot sentinel.</summary>
    public ushort Damage { get; internal set; }

    /// <summary>Projectile family/beam flags from WRAM <c>$0C18</c>.</summary>
    public ushort Type { get; internal set; }

    /// <summary>
    /// Lossless semantic view of <see cref="Type"/>. Raw storage remains public for exact
    /// WRAM comparison and debugging; consumers no longer need to repeat packed masks.
    /// </summary>
    public SamusProjectileTypeWord PackedType => new(Type);

    /// <summary>Low nibble is direction 0..9; upper bits are lifecycle flags.</summary>
    public ushort Direction { get; internal set; }

    /// <summary>Lossless semantic view of <see cref="Direction"/>.</summary>
    public SamusProjectileDirectionWord PackedDirection => new(Direction);

    /// <summary>Integer horizontal position in pixels.</summary>
    public ushort XPosition { get; internal set; }
    /// <summary>Integer vertical position in pixels.</summary>
    public ushort YPosition { get; internal set; }
    /// <summary>Fractional horizontal position word.</summary>
    public ushort XSubposition { get; internal set; }
    /// <summary>Fractional vertical position word.</summary>
    public ushort YSubposition { get; internal set; }
    /// <summary>Signed horizontal velocity component.</summary>
    public short XVelocity { get; internal set; }
    /// <summary>Signed vertical velocity component.</summary>
    public short YVelocity { get; internal set; }
    /// <summary>Horizontal collision radius in pixels.</summary>
    public ushort XRadius { get; internal set; }
    /// <summary>Vertical collision radius in pixels.</summary>
    public ushort YRadius { get; internal set; }
    /// <summary>Bank-$93 instruction-list pointer; zero marks the slot inactive for animation.</summary>
    public ushort InstructionPointer { get; internal set; }
    /// <summary>Updates remaining before the current projectile instruction advances.</summary>
    public ushort InstructionTimer { get; internal set; }
    /// <summary>Bank-relative pointer to the projectile's current spritemap.</summary>
    public ushort SpritemapPointer { get; internal set; }
    /// <summary>Current animation frame index.</summary>
    public ushort AnimationFrame { get; internal set; }
    /// <summary>Countdown controlling projectile-trail emission.</summary>
    public ushort TrailTimer { get; internal set; }
    /// <summary>WRAM <c>$0C7C</c>; missile ignition/acceleration state in the high byte.</summary>
    public ushort Variable { get; internal set; }
    /// <summary>WRAM $0CA4: auxiliary phase word used by Spazer special-attack particles.</summary>
    public ushort AuxiliaryPhase { get; internal set; }
    /// <summary>Gets the semantic identity of the slot's bank-$90 pre-instruction.</summary>
    public SamusProjectilePreInstruction PreInstruction { get; internal set; }

    /// <summary>Bank-$93 considers a nonzero instruction pointer allocated and drawable.</summary>
    public bool IsActive => InstructionPointer != 0;

    /// <summary>
    /// Bank-$A0 enemy collision admits a nonzero beam, missile, or Super Missile type even
    /// when bank $93 has no instruction list to animate. This distinction is observable for
    /// the left-facing Murder Beam, whose type and damage remain live while its list is zero.
    /// </summary>
    public bool HasEnemyCollisionPayload =>
        Type != 0 &&
        PackedType.Family is not (
            SamusProjectileFamily.PowerBomb or
            SamusProjectileFamily.Bomb) &&
        PackedType.FamilyValue < (ushort)SamusProjectileFamily.BeamExplosion;

    /// <summary>Clears the words owned by $90:ADB7; trail and auxiliary words survive reuse.</summary>
    internal void ClearFields()
    {
        Damage = 0;
        Type = 0;
        Direction = 0;
        XPosition = 0;
        YPosition = 0;
        XSubposition = 0;
        YSubposition = 0;
        XVelocity = 0;
        YVelocity = 0;
        XRadius = 0;
        YRadius = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        SpritemapPointer = 0;
        AnimationFrame = 0;
        Variable = 0;
        PreInstruction = SamusProjectilePreInstruction.None;
    }

    /// <summary>
    /// The per-slot words <c>ResetProjectileData</c> ($90:AD22) clears on a door load or
    /// elevator ride. Unlike deletion it keeps both subpixels, the animation frame and
    /// $0CA4, which a later shot in the slot inherits.
    /// </summary>
    internal void ClearForProjectileDataReset()
    {
        TrailTimer = 0;
        XPosition = 0;
        YPosition = 0;
        Direction = 0;
        XVelocity = 0;
        YVelocity = 0;
        XRadius = 0;
        YRadius = 0;
        Type = 0;
        Damage = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        Variable = 0;
        SpritemapPointer = 0;
        PreInstruction = SamusProjectilePreInstruction.None;
    }
}

/// <summary>Semantic identities for the bank-$90 function pointers stored per slot.</summary>
public enum SamusProjectilePreInstruction : byte
{
    /// <summary>No pre-instruction is assigned.</summary>
    None,
    /// <summary>Moves an ordinary beam that does not have the Wave Beam trail.</summary>
    NoWaveBeam,
    /// <summary>Moves a Wave Beam using its three-frame trail phase.</summary>
    WaveBeamThreeFrameTrail,
    /// <summary>Moves a Wave Beam using its four-frame trail phase.</summary>
    WaveBeamFourFrameTrail,
    /// <summary>Moves a Hyper Beam projectile.</summary>
    HyperBeam,
    /// <summary>Moves and accelerates an ordinary missile.</summary>
    Missile,
    /// <summary>Moves and accelerates a Super Missile.</summary>
    SuperMissile,
    /// <summary>Updates the linked Super Missile auxiliary projectile.</summary>
    SuperMissileLink,
    /// <summary>Updates the main Ice Beam special-attack projectile.</summary>
    IceCombo,
    /// <summary>Moves an outward Ice Beam special-attack particle.</summary>
    IceComboOutward,
    /// <summary>Updates the Wave Beam special attack.</summary>
    WaveCombo,
    /// <summary>Updates the main Spazer special-attack phase.</summary>
    SpazerCombo,
    /// <summary>Updates the Plasma Beam special attack.</summary>
    PlasmaCombo,
    /// <summary>Moves a falling Spazer special-attack particle.</summary>
    SpazerComboFalling,
    /// <summary>Moves a Shinespark echo projectile.</summary>
    ShinesparkEcho,
    /// <summary>Represents execution that reaches the spacetime palette-copy tail.</summary>
    SpacetimePaletteCopyTail,
    /// <summary>Represents the chainsaw window-store path before Power Bomb execution.</summary>
    ChainsawWindowStoreThenPowerBomb,
    /// <summary>Represents charged chainsaw execution through low WRAM.</summary>
    ChargedChainsawLowWramExecution,
    /// <summary>Represents the Murder Beam's misaligned native execution path.</summary>
    MurderBeamMisalignedExecution,
}

/// <summary>
/// One of the eighteen independent projectile-trail allocations. The left timer is the
/// native free-slot sentinel even though both sides otherwise animate independently.
/// </summary>
public sealed class SamusProjectileTrailSlot
{
    /// <summary>Creates one trail allocation with independent left- and right-side animation state.</summary>
    /// <param name="slotIndex">Zero-based index of this trail allocation.</param>
    internal SamusProjectileTrailSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    /// <summary>Gets the zero-based host trail-slot index.</summary>
    public int SlotIndex { get; }
    /// <summary>Gets the left-side trail animation state and free-slot timer.</summary>
    public SamusProjectileTrailSide Left { get; }
    /// <summary>Gets the right-side trail animation state.</summary>
    public SamusProjectileTrailSide Right { get; }

    /// <summary>Clears both trail sides when this allocation is released.</summary>
    internal void ClearFields()
    {
        Left.ClearFields();
        Right.ClearFields();
    }
}

/// <summary>One side of a two-stream bank-$90 projectile-trail animation.</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>Horizontal trail-sprite position in pixels.</summary>
    public ushort XPosition { get; internal set; }
    /// <summary>Vertical trail-sprite position in pixels.</summary>
    public ushort YPosition { get; internal set; }
    /// <summary>Updates remaining before the current trail instruction advances.</summary>
    public ushort InstructionTimer { get; internal set; }
    /// <summary>Bank-$90 pointer to the current trail instruction.</summary>
    public ushort InstructionPointer { get; internal set; }
    /// <summary>Packed tile number and OBJ attributes drawn for this trail side.</summary>
    public ushort TileNumberAttributes { get; internal set; }

    /// <summary>Resets the position, animation cursor, and tile attributes for this trail side.</summary>
    internal void ClearFields()
    {
        XPosition = 0;
        YPosition = 0;
        InstructionTimer = 0;
        InstructionPointer = 0;
        TileNumberAttributes = 0;
    }
}

/// <summary>Immutable summary of one ordinary-projectile alpha pass.</summary>
/// <param name="QueuedSoundEffect">The primary sound effect queued during the pass, if any.</param>
/// <param name="QueuedSoundMaximum">The maximum number of simultaneous sounds accepted for the primary request.</param>
/// <param name="AdditionalSoundRequests">Additional ordered sound requests emitted by the pass.</param>
/// <param name="QueuedSoundSuppressed">Whether the primary sound request was deliberately suppressed.</param>
/// <param name="PersistentMemoryCorrupted">Whether the modeled projectile path corrupted persistent memory.</param>
public readonly record struct SamusProjectileFrameResult(
    SoundEffectId? QueuedSoundEffect,
    byte QueuedSoundMaximum,
    IReadOnlyList<SamusSoundRequest>? AdditionalSoundRequests = null,
    bool QueuedSoundSuppressed = false,
    bool PersistentMemoryCorrupted = false);

/// <summary>
/// Immutable producer-phase evidence captured before the new projectile's first movement.
/// A beam may collide or leave the native movement window in that same alpha pass, so the
/// mutable slot can already be clear by the time a debugger inspects the frame result.
/// </summary>
public readonly record struct SamusProjectileSpawnSnapshot();

/// <summary>Semantic branch and raw table/timer evidence from one `$91:D743` call.</summary>
/// <param name="Action">The palette action selected by the charge-state branch.</param>
public readonly record struct SamusBeamChargePaletteStepResult(
    SamusBeamChargePaletteAction Action);

/// <summary>Named outcomes of the nonzero charged-shot glow branches.</summary>
public enum SamusBeamChargePaletteAction : byte
{
    /// <summary>No charge-palette effect is active.</summary>
    Inactive,
    /// <summary>Advances the ordinary charged-shot glow cycle.</summary>
    ChargeCycle,
    /// <summary>Advances the pseudo-Screw Attack glow cycle.</summary>
    PseudoScrewCycle,
    /// <summary>Applies the ordinary white charged-shot palette.</summary>
    OrdinaryWhite,
    /// <summary>Applies the next Hyper Beam palette entry.</summary>
    HyperPalette,
    /// <summary>Holds the current Hyper Beam palette.</summary>
    HyperHold,
    /// <summary>Restores the normal suit palette.</summary>
    RestoredNormalSuit,
}

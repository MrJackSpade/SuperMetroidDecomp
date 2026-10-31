using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam combination attacks (charge plus special weapon): activation, and the ice, plasma, spazer and wave combo steps with the Spacetime palette copy tail.</summary>
internal sealed class BeamCombos
{
    /// <summary>Clears expired combo slots through the same projectile lifecycle as ordinary beams.</summary>
    private readonly ProjectileAnimation _animation;
    /// <summary>Resets Samus's charge flare while combo particles control shared firing state.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Holds the four projectile slots populated together when a special beam attack starts.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>Spawns periodic Wave and other combo trails from their live particle slots.</summary>
    private readonly ProjectileTrails _trails;
    /// <summary>What the SpaceTime shot's runaway palette copy overwrites in saved progression.</summary>
    private readonly SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> _spaceTimeOverwrite;

    /// <summary>Connects special beam activation and particle updates to shared projectile, charge, and trail owners.</summary>
    internal BeamCombos(ProjectileAnimation animation, BeamChargeState charge, ProjectileSlotTable slotTable, ProjectileTrails trails,
        SpaceTimeProgressionOverwrite<AnimationProgramCursor<SamusProjectileProgram>, ProgressionOverwrite> spaceTimeOverwrite)
    {
        _animation = animation;
        _charge = charge;
        _slotTable = slotTable;
        _trails = trails;
        _spaceTimeOverwrite = Ensure.NotNull(spaceTimeOverwrite);
    }
    /// <summary>
    /// Angle step the Ice and Plasma special attacks add to every orbiting slot's angle each update: four, or minus
    /// four when a left-facing Samus starts the attack; activation also stores four for Wave and zero for Spazer,
    /// which never read it. Each slot keeps only the low byte of angle plus step, so minus four turns the 256-step
    /// angle the opposite way by the same four steps.
    /// </summary>
    public int ComboState { get; internal set; }

    /// <summary>
    /// FireSBA ($90:CCC0): debit the selected beam cost before attempting its family.
    /// This is the allocation boundary; the normal producer must call it only after
    /// charge/input eligibility, and particles require their own pre-instruction updates.
    /// </summary>
    internal bool TryActivateCombo(SamusState samus,
        SamusBombProjectileSystem shared, out SoundEffectId? sound)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(shared);
        sound = null;
        if (samus.SelectedHudItem != HudItem.PowerBomb) return false;
        SamusBeamCombination beam = SamusBeamCombinations.Of(samus.EquippedBeams);
        if (!beam.IsRetail)
            throw new NotSupportedException("Out-of-table Spazer/Plasma combo dispatcher requires emulated ROM execution.");
        int remaining = samus.PowerBombs - SamusComboMechanics.GetPowerBombCost(beam);
        samus.PowerBombs = remaining < 0 ? 0 : remaining;
        SamusComboKind? combo = SamusComboMechanics.ComboKind(beam);
        // A live Ice or Plasma combo is not restarted.
        bool activated = combo is not null &&
            !(combo == SamusComboKind.Ice && _slotTable.Slots[0].PreInstruction is SamusProjectilePreInstruction.IceCombo or SamusProjectilePreInstruction.IceComboOutward ||
              combo == SamusComboKind.Plasma && _slotTable.Slots[0].PreInstruction == SamusProjectilePreInstruction.PlasmaCombo);
        if (activated && combo is SamusComboKind kind)
        {
            for (int i = 3; i >= 0; i--)
            {
                var slot = _slotTable.Slots[i];
                slot.Type = SamusProjectileType.Beam(beam, charged: true);
                slot.Direction = kind == SamusComboKind.Spazer ? SamusProjectileDirection.DownFacingLeft : SamusProjectileDirection.UpFacingRight;
                slot.IsSpent = false;
                slot.PreInstruction = kind switch
                {
                    SamusComboKind.Wave => SamusProjectilePreInstruction.WaveCombo,
                    SamusComboKind.Ice => SamusProjectilePreInstruction.IceCombo,
                    SamusComboKind.Spazer => SamusProjectilePreInstruction.SpazerCombo,
                    SamusComboKind.Plasma => SamusProjectilePreInstruction.PlasmaCombo,
                    _ => throw new InvalidOperationException($"Undefined combo {kind}."),
                };
                if (kind != SamusComboKind.Plasma) slot.TrailTimer = WrappingCountdown.FromRaw(kind == SamusComboKind.Spazer && i < 2 ? (ushort)0 : (ushort)4);
                slot.ComboAngle = kind is SamusComboKind.Ice or SamusComboKind.Plasma
                    ? SamusComboMechanics.GetOriginAngle(i)
                    : default;
                slot.WaveComboVerticalSpeed = default;
                if (kind is SamusComboKind.Wave or SamusComboKind.Spazer)
                {
                    slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
                    slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(kind == SamusComboKind.Wave ? (short)0 : (short)40));
                }
                if (kind == SamusComboKind.Spazer) slot.AuxiliaryPhase = SpazerComboPhase.Circling;
                if (kind == SamusComboKind.Plasma) slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(40));
                slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw(kind is SamusComboKind.Wave or SamusComboKind.Ice ? (short)600
                    : kind == SamusComboKind.Spazer ? (short)((i & 1) == 0 ? 4 : -4) : (short)0));
                if (kind == SamusComboKind.Wave)
                {
                    slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + (i < 2 ? 128 : -128))), unchecked((ushort)(samus.Position.Y.Whole + (i is 0 or 3 ? 128 : -128)))));
                }
                if (kind == SamusComboKind.Spazer && i >= 2) slot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
                InitializeComboData(slot, kind == SamusComboKind.Ice ? BeamComboData.Charged
                    : kind == SamusComboKind.Spazer && i >= 2 ? BeamComboData.Echo : BeamComboData.Combo);
            }
            _slotTable.ProjectileCounter = 4;
            shared.SetSharedCooldown(SamusProjectileCooldowns.ReadByte(SamusFiringTableDefinitions.UnchargedCooldowns + _slotTable.Slots[0].Type.BeamTableRow));
            ComboState = kind == SamusComboKind.Spazer ? 0
                : kind == SamusComboKind.Wave || SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Right) ? 4 : -4;
            sound = kind switch
            {
                SamusComboKind.Wave => Library1Sound.WaveSpecialAttack,
                SamusComboKind.Ice => Library1Sound.IceSpecialAttack,
                SamusComboKind.Spazer => Library1Sound.SpazerSpecialAttack,
                SamusComboKind.Plasma => Library1Sound.PlasmaSpecialAttack,
                _ => throw new InvalidOperationException($"Undefined combo {kind}."),
            };
        }
        if (samus.PowerBombs == 0)
            samus.SelectedHudItem = samus.AutoCancelHudItemIndex = HudItem.Beam;
        return activated;
    }

    /// <summary>Selects damage and instruction data from the charged, combo, or Spazer echo tables and initializes its first frame.</summary>
    internal static void InitializeComboData(SamusProjectileSlot slot, BeamComboData comboData)
    {
        bool echo = comboData == BeamComboData.Echo;
        SamusProjectileSelectorTable table = comboData == BeamComboData.Charged ? SamusProjectileSelectorTable.ChargedBeam :
            echo ? SamusProjectileSelectorTable.EchoAndTrail : SamusProjectileSelectorTable.SpecialAttack;
        int index = echo ? EchoTableIndex(slot.Type) : slot.Type.BeamCombination.TableIndex;
        SamusProjectileHeader header = SamusProjectileSelection.Select(table, index);
        slot.Damage = SamusProjectileDamage.Read(header);
        if (unchecked((short)slot.Damage) < 0) throw new InvalidDataException("Negative native combo projectile damage.");
        SamusProjectileProgram? program = SamusProjectileHeaders.Program(header, comboData != BeamComboData.Combo ? slot.Direction.ToAimIndex() : 0);
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        if (comboData == BeamComboData.Charged)
            slot.Radius = SamusProjectilePrograms.InitialRadius(program);
    }
    /// <summary>The echo-and-trail selector entry an echo reads: its beam-table row past the table's two leading rows.</summary>
    private static int EchoTableIndex(SamusProjectileType echo) =>
        echo.BeamTableRow - SamusFiringTableDefinitions.EchoRowOffset - BeamComboDefinitions.EchoTableLeadingRows;

    /// <summary>Runs $90:CF09/$CF7A for one Ice combo slot and returns its optional library-one sound request.</summary>
    internal SoundEffectId? StepIceCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        bool outward = slot.PreInstruction == SamusProjectilePreInstruction.IceComboOutward;
        if (!outward && slot.PreInstruction != SamusProjectilePreInstruction.IceCombo)
            throw new InvalidOperationException("Ice combo update requires its own pre-instruction.");
        if (slot.IsSpent)
        {
            _animation.ClearProjectile(slot);
            return outward ? null : Library1Sound.IceSpecialAttackEnd;
        }
        int oldTimer = slot.TrailTimer.RawValue;
        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (oldTimer == 1)
        {
            slot.TrailTimer = WrappingCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        int distance = outward ? unchecked((ushort)slot.Velocity.X.SignedRawValue) : 32;
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(slot.ComboAngle, distance);
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(samus.Position.X.Whole + orbit.X)));
        if (outward && (unchecked((short)(slot.Position.X.Whole - camera.X + 32)) < 0 ||
                        unchecked((short)(slot.Position.X.Whole - camera.X - 288)) >= 0))
        {
            _animation.ClearProjectile(slot);
            return null;
        }
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole + orbit.Y)));
        if (outward && (unchecked((short)(slot.Position.Y.Whole - camera.Y - 16)) < 0 ||
                        unchecked((short)(slot.Position.Y.Whole - camera.Y - 256)) >= 0))
        {
            _animation.ClearProjectile(slot);
            return null;
        }
        slot.ComboAngle = slot.ComboAngle.AddTableUnits(ComboState);
        SoundEffectId? sound = null;
        if (outward)
            slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue + 8))));
        else
        {
            int oldLifetime = slot.Velocity.Y.SignedRawValue;
            slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw(unchecked((short)(oldLifetime - 1))));
            if (oldLifetime == 1)
            {
                slot.PreInstruction = SamusProjectilePreInstruction.IceComboOutward;
                slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(40));
                sound = Library1Sound.IceSpecialAttackEnd;
            }
        }
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        return sound;
    }
    /// <summary>Original $90:D793 Plasma ring update; phase is stored in the slot's native Y-speed word.</summary>
    internal void StepPlasmaCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.PlasmaCombo)
            throw new InvalidOperationException("Plasma update requires its own pre-instruction.");
        if (slot.IsSpent)
        {
            _animation.ClearProjectile(slot);
            return;
        }
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(
            slot.ComboAngle, unchecked((ushort)slot.Velocity.X.SignedRawValue));
        slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(orbit.X + samus.Position.X.Whole)), unchecked((ushort)(orbit.Y + samus.Position.Y.Whole))));
        slot.ComboAngle = slot.ComboAngle.AddTableUnits(ComboState);
        switch (slot.Velocity.Y.SignedRawValue)
        {
            case 0:
                slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue + BeamComboDefinitions.PlasmaRadiusStep))));
                if (slot.Velocity.X.SignedRawValue >= BeamComboDefinitions.PlasmaOuterRadius) slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw(1));
                break;
            case 1:
                slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue - BeamComboDefinitions.PlasmaRadiusStep))));
                if (slot.Velocity.X.SignedRawValue < BeamComboDefinitions.PlasmaInnerThreshold) slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw(2));
                break;
            case 2:
                // Only the final expansion checks visibility. The first two phases
                // intentionally keep rings alive beyond the viewport, unlike Ice.
                if (unchecked((short)(slot.Position.X.Whole - camera.X + 32)) < 0 ||
                    unchecked((short)(slot.Position.X.Whole - camera.X - 288)) >= 0 ||
                    unchecked((short)(slot.Position.Y.Whole - camera.Y - 16)) < 0 ||
                    unchecked((short)(slot.Position.Y.Whole - camera.Y - 256)) >= 0)
                    _animation.ClearProjectile(slot);
                else slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue + BeamComboDefinitions.PlasmaRadiusStep))));
                break;
            default: throw new InvalidDataException("Plasma ring reached an invalid native phase.");
        }
    }
    /// <summary>
    /// Runs the SpaceTime shot's callback, which falls back into the beam palette's copy loop and, from an animation that
    /// sends the copy far enough, copies on through saved progression (<see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/>). Before the
    /// shot's first frame is installed, or with no animation, the copy starts low and ends long before progression, and
    /// only the progression part of the copy is kept (owner decision on #1318).
    /// </summary>
    /// <param name="progression">The session's progression, which the copy overwrites; required when it reaches it.</param>
    /// <param name="slot">The SpaceTime shot.</param>
    internal void RunSpacetimePaletteCopyTail(SharedGameState? progression, SamusProjectileSlot slot)
    {
        Ensure.NotNull(slot);
        if (slot.Sprite is null || slot.Cursor is not { } cursor)
            return;
        if (_spaceTimeOverwrite.For(cursor, slot.SlotIndex) is { } overwrite)
            (progression ?? throw new InvalidOperationException(
                "SpaceTime's palette-copy overwrite writes progression; the frame must pass it.")).Overwrite(overwrite);
    }
    /// <summary>Runs $90:DB06/$DC9C, including paired-hit cleanup and retagging the primary particles as falling trails.</summary>
    internal SoundEffectId? StepSpazerCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, int cameraY)
    {
        bool falling = slot.PreInstruction == SamusProjectilePreInstruction.SpazerComboFalling;
        if (!falling && slot.PreInstruction != SamusProjectilePreInstruction.SpazerCombo)
            throw new InvalidOperationException("Spazer update requires its own pre-instruction.");
        if (slot.IsSpent)
        {
            int partner = slot.SlotIndex ^ 2;
            _animation.ClearProjectile(slot);
            if (!falling) _animation.ClearProjectile(_slotTable.Slots[partner]);
            return null;
        }
        int previousTimer = slot.TrailTimer.RawValue;
        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (previousTimer == 1)
        {
            slot.TrailTimer = WrappingCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        if (falling)
        {
            slot.Position += new Offset16<RoomPixels>(0, 8);
            if (unchecked((short)(slot.Position.Y.Whole - cameraY - 248)) >= 0)
            {
                _animation.ClearProjectile(slot);
                return null;
            }
        }
        else
        {
            Offset16<RoomPixels> orbit = SamusComboMechanics.GetSineOffset(
                slot.ComboAngle, unchecked((ushort)slot.Velocity.X.SignedRawValue));
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + orbit.X)), unchecked((ushort)(samus.Position.Y.Whole + orbit.Y - (slot.AuxiliaryPhase == SpazerComboPhase.Circling ? 0 : 114)))));
            switch (slot.AuxiliaryPhase)
            {
                case SpazerComboPhase.Circling:
                    AdvanceSpazerAngle(slot);
                    if (slot.ComboAngle == ByteAngle.HalfTurn)
                    {
                        slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(160));
                        slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw((short)((slot.SlotIndex & 1) == 0 ? 2 : -2)));
                        slot.Direction = SamusProjectileDirection.UpFacingRight;
                        slot.IsSpent = false;
                        slot.AuxiliaryPhase = SpazerComboPhase.FlyingUpTowardsPoint;
                    }
                    break;
                case SpazerComboPhase.FlyingUpTowardsPoint:
                    if (unchecked((short)(slot.Position.Y.Whole - cameraY - 16)) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue - 5))));
                        if (slot.Velocity.X.SignedRawValue == 0)
                        {
                            slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw((short)((slot.SlotIndex & 1) == 0 ? -2 : 2)));
                            slot.ComboAngle = slot.ComboAngle.AddTableUnits(ByteAngle.HalfTurn.RawValue);
                            slot.AuxiliaryPhase = SpazerComboPhase.FlyingUpAwayFromPoint;
                            shared.SetSharedCooldown(2);
                            _charge.FlareCounter = 0;
                            return slot.SlotIndex == 0 ? Library1Sound.SpazerSpecialAttackEnd : null;
                        }
                    }
                    break;
                case SpazerComboPhase.FlyingUpAwayFromPoint:
                    if (unchecked((short)(slot.Position.Y.Whole - cameraY - 16)) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue + 5))));
                        if (slot.Velocity.X.SignedRawValue >= 96) BeginSpazerFall(slot);
                    }
                    break;
                default: throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {slot.AuxiliaryPhase}.");
            }
        }
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        return null;
    }

    /// <summary>Advances the Spazer particle's byte-sized angular phase by its signed native step.</summary>
    private static void AdvanceSpazerAngle(SamusProjectileSlot slot) =>
        slot.ComboAngle = slot.ComboAngle.AddTableUnits(slot.Velocity.Y.SignedRawValue);

    /// <summary>Moves a circling Spazer particle outward and switches its primary pair to falling echo artwork.</summary>
    private static void BeginSpazerFall(SamusProjectileSlot slot)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (slot.SlotIndex < 2 ? 16 : -16))));
        slot.Direction = SamusProjectileDirection.DownFacingLeft;
        slot.IsSpent = false;
        slot.TrailTimer = WrappingCountdown.Start(4);
        slot.PreInstruction = SamusProjectilePreInstruction.SpazerComboFalling;
        if (slot.SlotIndex < 2)
        {
            slot.Type = SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail);
            InitializeComboData(slot, BeamComboData.Echo);
        }
    }
    /// <summary>Runs $90:DA08's Wave combo homing oscillator, preserving 8.8 acceleration and 16.16 position carry.</summary>
    internal SoundEffectId? StepWaveCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.WaveCombo)
            throw new InvalidOperationException("Wave update requires its own pre-instruction.");
        // A hit skips even the lifetime decrement. Expiry and collision both queue
        // the terminal sound, unlike Ice's silent outward-phase removal.
        if (slot.IsSpent)
        {
            _animation.ClearProjectile(slot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        slot.Velocity = slot.Velocity.WithY(SignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue - 1))));
        if (slot.Velocity.Y.SignedRawValue <= 0)
        {
            _animation.ClearProjectile(slot);
            return BeamComboDefinitions.WaveRemovalSound;
        }
        int oldXSpeed = slot.Velocity.X.SignedRawValue;
        int oldTimer = slot.TrailTimer.RawValue;
        slot.TrailTimer = slot.TrailTimer.Decrement().Remaining;
        if (oldTimer == 1)
        {
            slot.TrailTimer = WrappingCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        slot.Velocity = slot.Velocity.WithX(SignedEightEight.FromSignedRaw(AccelerateWaveAxis(slot.Velocity.X.SignedRawValue, unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)))));
        uint x = unchecked(new FixedCoordinate(slot.Position.X.Whole, slot.Position.X.Fraction).RawValue + (uint)(SixteenSixteen.FromEightEight(slot.Velocity.X.SignedRawValue)));
        slot.Position = slot.Position.WithWholeX(FixedCoordinate.WholeOf(unchecked((int)x))).WithFractionX(unchecked((ushort)x));
        int ySpeed = AccelerateWaveAxis(slot.WaveComboVerticalSpeed.SignedRawValue, unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)));
        slot.WaveComboVerticalSpeed = SignedEightEight.FromSignedRaw(unchecked((short)ySpeed));
        uint y = unchecked(new FixedCoordinate(slot.Position.Y.Whole, slot.Position.Y.Fraction).RawValue + (uint)(SixteenSixteen.FromEightEight(ySpeed)));
        slot.Position = slot.Position.WithWholeY(FixedCoordinate.WholeOf(unchecked((int)y))).WithFractionY(unchecked((ushort)y));
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        return slot.SlotIndex == 3 && (oldXSpeed < 0) != (slot.Velocity.X.SignedRawValue < 0) ? BeamComboDefinitions.WavePulseSound : null;
    }

    /// <summary>Moves one Wave velocity component toward Samus, respecting the native acceleration and signed speed bound.</summary>
    private static short AccelerateWaveAxis(int speed, int targetDelta)
    {
        if (targetDelta < 0)
            return unchecked((short)(speed + BeamComboDefinitions.WaveSpeedLimit - 1)) >= 0 ?
                unchecked((short)(speed - BeamComboDefinitions.WaveAcceleration)) : checked((short)speed);
        return unchecked((short)(speed - BeamComboDefinitions.WaveSpeedLimit)) < 0 ?
            unchecked((short)(speed + BeamComboDefinitions.WaveAcceleration)) : checked((short)speed);
    }
}

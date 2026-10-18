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

    /// <summary>Connects special beam activation and particle updates to shared projectile, charge, and trail owners.</summary>
    internal BeamCombos(ProjectileAnimation animation, BeamChargeState charge, ProjectileSlotTable slotTable, ProjectileTrails trails)
    {
        _animation = animation;
        _charge = charge;
        _slotTable = slotTable;
        _trails = trails;
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
        SamusBombProjectileSystem shared, out ushort sound)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(shared);
        sound = 0;
        if (samus.SelectedHudItem != HudItem.PowerBomb) return false;
        SamusBeamCombination beam = new SamusBeamLoadoutWord(samus.EquippedBeams).LowNibbleCombination;
        if (!beam.IsRetail)
            throw new NotSupportedException("Out-of-table Spazer/Plasma combo dispatcher requires emulated ROM execution.");
        int remaining = samus.PowerBombs - SamusComboMechanicsDefinitions.GetPowerBombCost(beam);
        samus.PowerBombs = remaining < 0 ? 0 : remaining;
        SamusComboKind? combo = SamusComboMechanicsDefinitions.ComboKind(beam);
        // A live Ice or Plasma combo is not restarted.
        bool activated = combo is not null &&
            !(combo == SamusComboKind.Ice && _slotTable._slots[0].PreInstruction is SamusProjectilePreInstruction.IceCombo or SamusProjectilePreInstruction.IceComboOutward ||
              combo == SamusComboKind.Plasma && _slotTable._slots[0].PreInstruction == SamusProjectilePreInstruction.PlasmaCombo);
        if (activated && combo is SamusComboKind kind)
        {
            for (int i = 3; i >= 0; i--)
            {
                var slot = _slotTable._slots[i];
                slot.Type = (ushort)((samus.EquippedBeams & SamusProjectileTypeWord.EquippedBeamBits).ToNativeWord() | SamusComboRomData.ProjectileTag);
                slot.Direction = kind == SamusComboKind.Spazer ? (ushort)5 : (ushort)0;
                slot.PreInstruction = kind switch
                {
                    SamusComboKind.Wave => SamusProjectilePreInstruction.WaveCombo,
                    SamusComboKind.Ice => SamusProjectilePreInstruction.IceCombo,
                    SamusComboKind.Spazer => SamusProjectilePreInstruction.SpazerCombo,
                    SamusComboKind.Plasma => SamusProjectilePreInstruction.PlasmaCombo,
                    _ => throw new InvalidOperationException($"Undefined combo {kind}."),
                };
                if (kind != SamusComboKind.Plasma) slot.TrailTimer = NativeCountdown.FromRaw(kind == SamusComboKind.Spazer && i < 2 ? (ushort)0 : (ushort)4);
                slot.Variable = kind is SamusComboKind.Ice or SamusComboKind.Plasma
                    ? SamusComboMechanicsDefinitions.GetOriginAngle(i)
                    : (ushort)0;
                if (kind is SamusComboKind.Wave or SamusComboKind.Spazer)
                {
                    slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
                    slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(kind == SamusComboKind.Wave ? (short)0 : (short)40));
                }
                if (kind == SamusComboKind.Spazer) slot.AuxiliaryPhase = SpazerComboPhase.Circling;
                if (kind == SamusComboKind.Plasma) slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(40));
                slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(kind is SamusComboKind.Wave or SamusComboKind.Ice ? (short)600
                    : kind == SamusComboKind.Spazer ? (short)((i & 1) == 0 ? 4 : -4) : (short)0));
                if (kind == SamusComboKind.Wave)
                {
                    slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + (i < 2 ? 128 : -128))), unchecked((ushort)(samus.Position.Y.Whole + (i is 0 or 3 ? 128 : -128)))));
                }
                if (kind == SamusComboKind.Spazer && i >= 2) slot.Type = SamusComboRomData.SpazerTrailType;
                InitializeComboData(slot, kind == SamusComboKind.Ice, kind == SamusComboKind.Spazer && i >= 2);
            }
            _slotTable.ProjectileCounter = 4;
            shared.SetSharedCooldown(SamusProjectileCooldownDefinitions.ReadByte(SamusProjectileRomData.Beams.UnchargedCooldowns + (_slotTable._slots[0].Type & 63)));
            ComboState = kind == SamusComboKind.Spazer ? 0
                : kind == SamusComboKind.Wave || samus.PoseMetadata.IsFacingRight() ? 4 : -4;
            sound = kind switch
            {
                SamusComboKind.Wave => 40,
                SamusComboKind.Ice => 35,
                SamusComboKind.Spazer => 37,
                SamusComboKind.Plasma => 39,
                _ => throw new InvalidOperationException($"Undefined combo {kind}."),
            };
        }
        if (samus.PowerBombs == 0)
            samus.SelectedHudItem = samus.AutoCancelHudItemIndex = HudItem.Beam;
        return activated;
    }

    /// <summary>Selects damage and instruction data from the charged, combo, or Spazer echo tables and initializes its first frame.</summary>
    internal static void InitializeComboData(SamusProjectileSlot slot, bool ordinary, bool echo)
    {
        int table = ordinary ? SamusProjectileRomData.Beams.ChargedDataPointers :
            echo ? SamusComboRomData.EchoDataPointers : SamusComboRomData.DataPointers;
        int index = echo ? (byte)slot.Type - 34 : slot.Type & 15;
        int data = SamusProjectileRomData.Banks.Projectile | SamusProjectileSelectionDefinitions.ReadWord(table + index * 2);
        slot.Damage = SamusProjectileDamageDefinitions.Read(data);
        if (unchecked((short)slot.Damage) < 0) throw new InvalidDataException("Negative native combo projectile damage.");
        SamusProjectileProgram? program = SamusProjectileHeaderDefinitions.Program((ushort)data, ordinary || echo ? slot.Direction & 15 : 0);
        slot.Cursor = SamusProjectilePrograms.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        if (ordinary)
            slot.Radius = SamusProjectilePrograms.InitialRadius(program);
    }
    /// <summary>Runs $90:CF09/$CF7A for one Ice combo slot and returns its optional library-one sound request.</summary>
    internal ushort StepIceCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, Position16<RoomPixels> camera)
    {
        bool outward = slot.PreInstruction == SamusProjectilePreInstruction.IceComboOutward;
        if (!outward && slot.PreInstruction != SamusProjectilePreInstruction.IceCombo)
            throw new InvalidOperationException("Ice combo update requires its own pre-instruction.");
        if ((slot.Direction & SamusProjectileDirectionWord.LifecycleBits) != 0)
        {
            _animation.ClearProjectile(slot);
            return outward ? (ushort)0 : (ushort)36;
        }
        ushort oldTimer = slot.TrailTimer.RawValue;
        slot.TrailTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer == 1)
        {
            slot.TrailTimer = NativeCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        ushort distance = outward ? unchecked((ushort)slot.Velocity.X.SignedRawValue) : (ushort)32;
        var (x, y) = SamusComboMechanicsDefinitions.GetSineOffset(checked((ushort)slot.Variable), distance);
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(samus.Position.X.Whole + x)));
        if (outward && (unchecked((short)(slot.Position.X.Whole - camera.X + 32)) < 0 ||
                        unchecked((short)(slot.Position.X.Whole - camera.X - 288)) >= 0))
        {
            _animation.ClearProjectile(slot);
            return 0;
        }
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole + y)));
        if (outward && (unchecked((short)(slot.Position.Y.Whole - camera.Y - 16)) < 0 ||
                        unchecked((short)(slot.Position.Y.Whole - camera.Y - 256)) >= 0))
        {
            _animation.ClearProjectile(slot);
            return 0;
        }
        slot.Variable = unchecked((byte)(slot.Variable + ComboState));
        ushort sound = 0;
        if (outward)
            slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue + 8))));
        else
        {
            short oldLifetime = slot.Velocity.Y.SignedRawValue;
            slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(oldLifetime - 1))));
            if (oldLifetime == 1)
            {
                slot.PreInstruction = SamusProjectilePreInstruction.IceComboOutward;
                slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(40));
                sound = 36;
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
        if ((slot.Direction & SamusProjectileDirectionWord.LifecycleBits) != 0)
        {
            _animation.ClearProjectile(slot);
            return;
        }
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        var (x, y) = SamusComboMechanicsDefinitions.GetSineOffset(
            checked((ushort)slot.Variable), unchecked((ushort)slot.Velocity.X.SignedRawValue));
        slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(x + samus.Position.X.Whole)), unchecked((ushort)(y + samus.Position.Y.Whole))));
        slot.Variable = unchecked((byte)(slot.Variable + ComboState));
        switch (slot.Velocity.Y.SignedRawValue)
        {
            case 0:
                slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue + SamusComboRomData.PlasmaRadiusStep))));
                if (slot.Velocity.X.SignedRawValue >= SamusComboRomData.PlasmaOuterRadius) slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(1));
                break;
            case 1:
                slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue - SamusComboRomData.PlasmaRadiusStep))));
                if (slot.Velocity.X.SignedRawValue < SamusComboRomData.PlasmaInnerThreshold) slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(2));
                break;
            case 2:
                // Only the final expansion checks visibility. The first two phases
                // intentionally keep rings alive beyond the viewport, unlike Ice.
                if (unchecked((short)(slot.Position.X.Whole - camera.X + 32)) < 0 ||
                    unchecked((short)(slot.Position.X.Whole - camera.X - 288)) >= 0 ||
                    unchecked((short)(slot.Position.Y.Whole - camera.Y - 16)) < 0 ||
                    unchecked((short)(slot.Position.Y.Whole - camera.Y - 256)) >= 0)
                    _animation.ClearProjectile(slot);
                else slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((byte)(slot.Velocity.X.SignedRawValue + SamusComboRomData.PlasmaRadiusStep))));
                break;
            default: throw new InvalidDataException("Plasma ring reached an invalid native phase.");
        }
    }
    /// <summary>
    /// Executes the observable tail of the malformed $90:AD16 callback. The callback
    /// starts on the second operand byte of a long store, so $C1 $7E is first decoded as
    /// an irrelevant compare. It then increments the projectile loop's X and inherited Y
    /// before conditionally falling back into LoadBeamPalette's copy loop at $90:AD12.
    /// </summary>
    /// <returns>
    /// True when the copy touched a translated persistent-progression or saved-entry byte.
    /// </returns>
    internal static bool RunSpacetimePaletteCopyTail(
        SuperMetroid.Core.Saves.SaveMirrorImage? progressionMirror,
        SamusOverreadCatalog? overreads,
        SamusProjectileSlot slot)
    {
        ushort inheritedY = slot.SpritemapPointer == 0
            ? unchecked((ushort)(slot.PackedType.BeamCombination.TableIndex *
                SamusProjectileRomData.Beams.InitialSpeedRowBytes))
            : unchecked((ushort)(slot.InstructionPointer -
                SpacetimeBeamCorruptionLayout.AnimationRecordByteCount));
        ushort x = unchecked((ushort)(slot.NativeByteIndex + sizeof(ushort)));
        ushort y = unchecked((ushort)(inheritedY + sizeof(ushort)));
        bool persistentMemoryCorrupted = false;

        while (unchecked((short)(y - SpacetimeBeamCorruptionLayout.PaletteByteCount)) < 0)
        {
            persistentMemoryCorrupted |= new SpacetimeBeamCorruptionMemory(
                progressionMirror ?? throw new InvalidOperationException(
                    "SpaceTime's palette-copy overwrite writes the progression mirror; the frame must pass it."),
                overreads ?? throw new InvalidOperationException(
                    "SpaceTime's palette-copy overwrite reads cartridge bytes; the Samus overread catalog must be installed.")).CopyPaletteWord(x, y);
            x = unchecked((ushort)(x + sizeof(ushort)));
            y = unchecked((ushort)(y + sizeof(ushort)));
        }

        return persistentMemoryCorrupted;
    }
    /// <summary>Runs $90:DB06/$DC9C, including paired-hit cleanup and retagging the primary particles as falling trails.</summary>
    internal ushort StepSpazerCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared, ushort cameraY)
    {
        bool falling = slot.PreInstruction == SamusProjectilePreInstruction.SpazerComboFalling;
        if (!falling && slot.PreInstruction != SamusProjectilePreInstruction.SpazerCombo)
            throw new InvalidOperationException("Spazer update requires its own pre-instruction.");
        if ((slot.Direction & SamusProjectileDirectionWord.LifecycleBits) != 0)
        {
            int partner = slot.SlotIndex ^ 2;
            _animation.ClearProjectile(slot);
            if (!falling) _animation.ClearProjectile(_slotTable._slots[partner]);
            return 0;
        }
        ushort previousTimer = slot.TrailTimer.RawValue;
        slot.TrailTimer = NativeCountdown.FromRaw(unchecked((ushort)(previousTimer - 1)));
        if (previousTimer == 1)
        {
            slot.TrailTimer = NativeCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        if (falling)
        {
            slot.Position += new Offset16<RoomPixels>(0, 8);
            if (unchecked((short)(slot.Position.Y.Whole - cameraY - 248)) >= 0)
            {
                _animation.ClearProjectile(slot);
                return 0;
            }
        }
        else
        {
            var (x, y) = SamusComboMechanicsDefinitions.GetSineOffset(
                checked((ushort)slot.Variable), unchecked((ushort)slot.Velocity.X.SignedRawValue));
            slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + x)), unchecked((ushort)(samus.Position.Y.Whole + y - (slot.AuxiliaryPhase == SpazerComboPhase.Circling ? 0 : 114)))));
            switch (slot.AuxiliaryPhase)
            {
                case SpazerComboPhase.Circling:
                    AdvanceSpazerAngle(slot);
                    if (slot.Variable == 128)
                    {
                        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(160));
                        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw((short)((slot.SlotIndex & 1) == 0 ? 2 : -2)));
                        slot.Direction = 0;
                        slot.AuxiliaryPhase = SpazerComboPhase.FlyingUpTowardsPoint;
                    }
                    break;
                case SpazerComboPhase.FlyingUpTowardsPoint:
                    if (unchecked((short)(slot.Position.Y.Whole - cameraY - 16)) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue - 5))));
                        if (slot.Velocity.X.SignedRawValue == 0)
                        {
                            slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw((short)((slot.SlotIndex & 1) == 0 ? -2 : 2)));
                            slot.Variable = unchecked((byte)(slot.Variable + 128));
                            slot.AuxiliaryPhase = SpazerComboPhase.FlyingUpAwayFromPoint;
                            shared.SetSharedCooldown(2);
                            _charge.FlareCounter = 0;
                            return slot.SlotIndex == 0 ? (ushort)38 : (ushort)0;
                        }
                    }
                    break;
                case SpazerComboPhase.FlyingUpAwayFromPoint:
                    if (unchecked((short)(slot.Position.Y.Whole - cameraY - 16)) < 0)
                        BeginSpazerFall(slot);
                    else
                    {
                        AdvanceSpazerAngle(slot);
                        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.X.SignedRawValue + 5))));
                        if (slot.Velocity.X.SignedRawValue >= 96) BeginSpazerFall(slot);
                    }
                    break;
                default: throw new InvalidOperationException($"Undefined {nameof(SpazerComboPhase)} {(int)slot.AuxiliaryPhase}.");
            }
        }
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        return 0;
    }

    /// <summary>Advances the Spazer particle's byte-sized angular phase by its signed native step.</summary>
    private static void AdvanceSpazerAngle(SamusProjectileSlot slot) =>
        slot.Variable = unchecked((byte)(slot.Variable + slot.Velocity.Y.SignedRawValue));

    /// <summary>Moves a circling Spazer particle outward and switches its primary pair to falling echo artwork.</summary>
    private static void BeginSpazerFall(SamusProjectileSlot slot)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (slot.SlotIndex < 2 ? 16 : -16))));
        slot.Direction = 5;
        slot.TrailTimer = NativeCountdown.Start(4);
        slot.PreInstruction = SamusProjectilePreInstruction.SpazerComboFalling;
        if (slot.SlotIndex < 2)
        {
            slot.Type = SamusComboRomData.SpazerTrailType;
            InitializeComboData(slot, ordinary: false, echo: true);
        }
    }
    /// <summary>Runs $90:DA08's Wave combo homing oscillator, preserving 8.8 acceleration and 16.16 position carry.</summary>
    internal ushort StepWaveCombo(SamusState samus,
        SamusProjectileSlot slot, SamusBombProjectileSystem shared)
    {
        if (slot.PreInstruction != SamusProjectilePreInstruction.WaveCombo)
            throw new InvalidOperationException("Wave update requires its own pre-instruction.");
        // A hit skips even the lifetime decrement. Expiry and collision both queue
        // the terminal sound, unlike Ice's silent outward-phase removal.
        if ((slot.Direction & SamusProjectileDirectionWord.LifecycleBits) != 0)
        {
            _animation.ClearProjectile(slot);
            return SamusComboRomData.WaveRemovalSound;
        }
        slot.Velocity = slot.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(unchecked((short)(slot.Velocity.Y.SignedRawValue - 1))));
        if (slot.Velocity.Y.SignedRawValue <= 0)
        {
            _animation.ClearProjectile(slot);
            return SamusComboRomData.WaveRemovalSound;
        }
        short oldXSpeed = slot.Velocity.X.SignedRawValue;
        ushort oldTimer = slot.TrailTimer.RawValue;
        slot.TrailTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer == 1)
        {
            slot.TrailTimer = NativeCountdown.Start(4);
            _trails.SpawnTrail(slot);
        }
        slot.Velocity = slot.Velocity.WithX(SnesSignedEightEight.FromSignedRaw(AccelerateWaveAxis(slot.Velocity.X.SignedRawValue, unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)))));
        uint x = unchecked(new SnesFixedPosition(slot.Position.X.Whole, slot.Position.X.Fraction).RawValue + (uint)(SnesSixteenSixteen.FromEightEight(slot.Velocity.X.SignedRawValue)));
        slot.Position = slot.Position.WithWholeX(SnesFixedPosition.WholeOf(unchecked((int)x))).WithFractionX(unchecked((ushort)x));
        short ySpeed = AccelerateWaveAxis(unchecked((short)slot.Variable), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)));
        slot.Variable = unchecked((ushort)ySpeed);
        uint y = unchecked(new SnesFixedPosition(slot.Position.Y.Whole, slot.Position.Y.Fraction).RawValue + (uint)(SnesSixteenSixteen.FromEightEight(ySpeed)));
        slot.Position = slot.Position.WithWholeY(SnesFixedPosition.WholeOf(unchecked((int)y))).WithFractionY(unchecked((ushort)y));
        shared.SetSharedCooldown(2);
        _charge.FlareCounter = 0;
        return slot.SlotIndex == 3 && (oldXSpeed < 0) != (slot.Velocity.X.SignedRawValue < 0) ? SamusComboRomData.WavePulseSound : (ushort)0;
    }

    /// <summary>Moves one Wave velocity component toward Samus, respecting the native acceleration and signed speed bound.</summary>
    private static short AccelerateWaveAxis(short speed, short targetDelta)
    {
        if (targetDelta < 0)
            return unchecked((short)(speed + SamusComboRomData.WaveSpeedLimit - 1)) >= 0 ?
                unchecked((short)(speed - SamusComboRomData.WaveAcceleration)) : speed;
        return unchecked((short)(speed - SamusComboRomData.WaveSpeedLimit)) < 0 ?
            unchecked((short)(speed + SamusComboRomData.WaveAcceleration)) : speed;
    }
}

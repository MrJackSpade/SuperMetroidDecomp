using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The bank-$93 projectile instruction interpreter and drawing: slot clearing, live projectile and charge-flare OAM, flare animation state and the suit palette the flare restores.</summary>
internal sealed class ProjectileAnimation
{
    /// <summary>Owns charge-flare frame and countdown arrays advanced by the interpreter.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Owns the active projectile count reduced when a delete instruction clears a slot.</summary>
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>Binds projectile instructions to live slot ownership and charge-flare state.</summary>
    internal ProjectileAnimation(BeamChargeState charge, ProjectileSlotTable slotTable)
    {
        _charge = charge;
        _slotTable = slotTable;
    }
    /// <summary>Decodes timed bank-$93 frames, goto, and delete; publishes frame radius and animation metadata when the countdown expires.</summary>
    internal bool RunProjectileInstructionHandler(
        SamusProjectileSlot slot)
    {
        slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
        if (!slot.InstructionTimer.IsZero)
            return false;
        AnimationProgramCursor<SamusProjectileProgram> cursor = slot.Cursor
            ?? throw new InvalidOperationException("An inactive projectile has no list to run.");
        if (SamusProjectilePrograms.RunToNextRecord(ref cursor, out int duration) is not { } record)
        {
            ClearProjectile(slot);
            return true;
        }
        slot.InstructionTimer = NativeCountdown.FromRaw((ushort)duration);
        slot.SpritemapPointer = FrameBindings?.Resolve(record.Address) ?? record.Spritemap;
        slot.Radius = record.Radius;
        slot.AnimationFrame = record.AnimationFrame;
        slot.Cursor = cursor;
        return false;
    }


    /// <summary>Clears a projectile slot and decrements the shared active count without underflow.</summary>
    internal void ClearProjectile(SamusProjectileSlot slot)
    {
        slot.ClearFields();
        _slotTable.ProjectileCounter = _slotTable.ProjectileCounter == 0
            ? (ushort)0
            : unchecked((ushort)(_slotTable.ProjectileCounter - 1));
    }

    /// <summary>Publishes an installed projectile composition when its screen origin passes native horizontal and Y-byte clipping.</summary>
    internal static void DrawSlot(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        SamusProjectileSlot slot,
        Position16<RoomPixels> layer1,
        int? horizontalMargin,
        ProjectileDrawArtwork artwork)
    {
        short screenX = unchecked((short)(slot.Position.X.Whole - layer1.X));
        ushort screenY = unchecked((ushort)(slot.Position.Y.Whole - layer1.Y));
        if (horizontalMargin is int margin && (screenX < -margin || screenX >= 256 + margin) ||
            (SnesLittleEndian.HighByteInPlace(screenY)) != 0)
        {
            return;
        }

        sprites.Add(new SpriteDraw(source, artwork.Sprites.Frame(slot.SpritemapPointer), artwork.Page,
            new Position16<ScreenPixels>(unchecked((ushort)screenX), screenY), default, 0, 0));
    }

    /// <summary>Advances a flare component only after its countdown underflows, honoring restart and rewind delay commands.</summary>
    internal void AdvanceFlareComponent(int component)
    {
        // The assembly advances only when 16-bit DEC crosses zero into `$FFFF`. A timer
        // value of zero therefore survives one visible call; testing equality here would
        // make every ROM-authored delay one frame too short.
        NativeWordCounterStep timer = _charge._flareTimers[component].Decrement();
        _charge._flareTimers[component] = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        ushort frame = unchecked((ushort)(_charge._flareFrames[component] + 1));
        ushort delayList = ChargeFlareAnimationDefinitions.ReadWord(
            SamusProjectileRomData.Beams.ChargeFlareDelayListPointers + component * 2);
        byte delay = ChargeFlareAnimationDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.MovementNumber,
                unchecked((ushort)(delayList + frame))));
        if (ChargeFlareAnimationDefinitions.TryDecodeCommand(delay, out ChargeFlareDelayCommand command))
        {
            switch (command)
            {
                case ChargeFlareDelayCommand.Restart:
                    frame = 0;
                    delay = ChargeFlareAnimationDefinitions.ReadByte((int)new SnesAddress(
                        SamusProjectileRomData.Banks.MovementNumber,
                        delayList));
                    break;
                case ChargeFlareDelayCommand.Rewind:
                    byte rewind = ChargeFlareAnimationDefinitions.ReadByte(
                        (int)new SnesAddress(
                            SamusProjectileRomData.Banks.MovementNumber,
                            unchecked((ushort)(delayList + frame + 1))));
                    frame = unchecked((ushort)(frame - rewind));
                    delay = ChargeFlareAnimationDefinitions.ReadByte(
                        (int)new SnesAddress(
                            SamusProjectileRomData.Banks.MovementNumber,
                            unchecked((ushort)(delayList + frame))));
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(ChargeFlareDelayCommand)} {(int)command}.");
            }
        }

        _charge._flareFrames[component] = frame;
        _charge._flareTimers[component] = NativeCountdown.Start(delay);
    }

    /// <summary>Reads Samus's shot direction and movement metadata before drawing an eligible charge-flare component.</summary>
    internal void DrawFlareComponent(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> layer1, int component, SamusMode7Transform? mode7Transform, ChargeFlareArtwork artwork)
    {
        byte direction = samus.PoseMetadata.ReadShotDirection();
        if (direction >= 16)
            return;

        DrawFlareComponentWithMetadata(sprites, samus, layer1, component, mode7Transform, artwork, new ChargeFlarePose { Direction = direction, Running = samus.PoseMetadata.ReadMovementKind() == SamusMovementType.Running, PoseYOffset = unchecked((byte)samus.PoseMetadata.ReadGraphicsYOffset()), FacingLeft = samus.PoseMetadata.IsFacingLeft() });
    }

    /// <summary>Applies pose muzzle offsets after optional Ceres Mode-7 center transform, then draws the installed component frame.</summary>
    internal void DrawFlareComponentWithMetadata(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> layer1, int component, SamusMode7Transform? mode7Transform, ChargeFlareArtwork artwork, ChargeFlarePose pose)
    {
        var visualOffset = (artwork.Placement ?? throw new InvalidOperationException(
            "Charge flare requires installed placement definitions."))
            .Resolve(pose.Running, pose.Direction & 0x0f); // low nibble of the native shot-direction byte
        short xOffset = visualOffset.X;
        short yOffset = visualOffset.Y;

        // `$90:BBE1` calls `$8B:8A52` under the same Ceres-status high bit used by the
        // body renderer. Only Samus's center is transformed; the pose-selected muzzle
        // offsets are added afterward. Retail then restores `$0AF6/$0AFA`, so this path
        // must never write the public physics coordinates merely to reuse the arithmetic.
        var samusPosition = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = mode7Transform is { } transform
            ? transform.Transform(samusPosition)
            : samusPosition;
        ushort screenX = unchecked((ushort)(renderPoint.X + xOffset - layer1.X));
        ushort screenY = unchecked((ushort)(renderPoint.Y + yOffset - pose.PoseYOffset - layer1.Y));

        // `$90:BC98` clips only by the origin's Y high byte. The shared bank-$81 loader
        // deliberately allows individual entries to wrap, matching charge sparks near an
        // edge instead of applying the generic on-screen-origin parking rule.
        if ((SnesLittleEndian.HighByteInPlace(screenY)) != 0)
            return;

        ushort indexOffset = unchecked((ushort)(pose.FacingLeft
            ? component switch { 0 => 0, 1 => 42, _ => 48 }
            : component switch { 0 => 0, 1 => 30, _ => 36 }));
        // `$90:BBEA` keeps only the low byte of the frame word before adding the offset.
        ushort tableIndex = unchecked((ushort)(indexOffset + (SnesLittleEndian.LowByte(_charge._flareFrames[component]))));
        var catalog = artwork.Compositions ?? throw new InvalidOperationException(
            "Charge flare requires installed sprite compositions.");
        // A frame that wrapped to $FF selects a pointer past the end of $93:A1A1. Native then
        // draws a "spritemap" out of WRAM, I/O registers and open bus (into OAM only; the
        // index is masked to $1FF). That garbage needs full memory emulation and has no
        // gameplay effect, so the port draws nothing for it (#1275, Mother Brain glitch movie).
        if (!SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteCatalog.Contains(tableIndex))
            return;
        sprites.Add(new SpriteDraw(new SpriteSource(SpriteSourceKind.SamusChargeFlare, component), catalog.Frame(tableIndex),
            artwork.Objects.Page, new Position16<ScreenPixels>(screenX, screenY), default, 0, 0));
    }

    /// <summary>Clears all component frames and countdowns when charge-flare animation is retired.</summary>
    internal void ClearFlareAnimationState()
    {
        Array.Clear(_charge._flareFrames);
        Array.Clear(_charge._flareTimers);
    }

    /// <summary>Restores sixteen suit colors from installed artwork and retains the native selection pointer for diagnostics.</summary>
    internal static ushort LoadNormalSuitPalette(
        SnesCgram cgram,
        SamusState samus)
    {
        // Preserve the native pointer as the diagnostic result while the installed
        // color source replaces only the sixteen displayed BGR555 words.
        return SamusNormalSuitPalette.Load(cgram, samus.EquippedItems, samus.Presentation.SuitColors);
    }

    /// <summary>Maps equipped suit bits to the native normal-suit palette table offset.</summary>
    internal static ushort GetSuitPaletteOffset(ushort equippedItems) =>
        equippedItems.GetSuitPaletteTableOffset();

    /// <summary>Installed timed-frame sprite choices; null retains the cartridge diagnostic path.</summary>
    [field: NonSerialized]
    public SuperMetroid.Core.Samus.Artwork.ProjectileFrameBindingCatalog? FrameBindings { get; set; }
}

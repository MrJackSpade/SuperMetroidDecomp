namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile trails: spawning a trail behind a shot, its animation frame, and drawing each side.</summary>
internal sealed class ProjectileTrails
{
    /// <summary>Projectile-owned left and right trail pairs shared with the slot allocator.</summary>
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>Uses the owning projectile table so trail allocation and drawing see the same slots.</summary>
    internal ProjectileTrails(ProjectileSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Selects native trail lists and positions both sides from the shot's current animation record.</summary>
    internal void SpawnTrail(ISnesAddressSpace bus, SamusProjectileSlot projectile)
    {
        int pointerIndex;
        if (projectile.PackedType.IsFamily(SamusProjectileFamily.Beam))
        {
            // Beam indices retain charge/SBA bits in the low six bits. Charged plain power
            // therefore selects entry $10, not ordinary-power entry zero.
            pointerIndex = projectile.Type & 0x003f;
        }
        else
        {
            // Missiles and supers map families $01/$02 to table entries $20/$21. Other
            // projectile families return carry set and do not consume a trail slot.
            int family = SnesLittleEndian.HighByte(projectile.PackedType.FamilyValue);
            if (family >= 3)
                return;
            pointerIndex = family + 0x001f;
        }

        SamusProjectileTrailSlot? trail = null;
        for (int slotIndex = SamusProjectileSystem.TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            // The original tests only the left timer. A right stream can still be alive in
            // malformed/debug-edited state and will be overwritten when the left sentinel is
            // clear; retain that asymmetric allocation contract rather than being "safer".
            if (_slotTable._trailSlots[slotIndex].Left.InstructionTimer.IsZero)
            {
                trail = _slotTable._trailSlots[slotIndex];
                break;
            }
        }
        if (trail is null)
            return;

        trail.Left.InstructionTimer = NativeCountdown.Start(1);
        trail.Right.InstructionTimer = NativeCountdown.Start(1);
        trail.Left.InstructionPointer = ProjectileTrailDefinitions.ReadSelector(
            SamusProjectileRomData.Trails.LeftInstructionPointers + pointerIndex * 2);
        trail.Right.InstructionPointer = ProjectileTrailDefinitions.ReadSelector(
            SamusProjectileRomData.Trails.RightInstructionPointers + pointerIndex * 2);

        // Retail reads the previously installed record, even when the animation timer
        // will expire later this frame. Reading ahead advances Spazer's trail spread early.
        ushort animationFrame = GetTrailAnimationFrame(projectile);
        int direction = projectile.PackedDirection.DirectionIndex;
        int familyTable = (projectile.Type & 0x0020) != 0
            ? SamusProjectileRomData.Trails.SpazerSbaOffsetFamilies
            : projectile.PackedType.IsChargedBeam
                ? SamusProjectileRomData.Trails.ChargedOffsetFamilies
                : SamusProjectileRomData.Trails.UnchargedOffsetFamilies;
        ushort directionTable = ProjectileTrailCoordinateDefinitions.ReadFamilyPointer(
            familyTable + projectile.PackedType.BeamCombination.TableIndex * 2);
        ushort offsetList = ProjectileTrailCoordinateDefinitions.ReadDirectionPointer(
            bus,
            SamusProjectileRomData.Banks.PaletteAndTrailData |
                unchecked((ushort)(directionTable + direction * 2)));
        int offsets = SamusProjectileRomData.Banks.PaletteAndTrailData |
            unchecked((ushort)(offsetList + animationFrame * 4));

        // Native performs WORD reads and consumes their high bytes after XBA.
        // Reflection can expose the word preceding a freshly installed animation
        // list, sending this lookup into reserved B-bus space. Preserve the actual
        // operand-driven open bus there instead of changing trail timing or hiding
        // every unsupported hardware read behind a zero fallback.
        ushort y = (ushort)offsets;
        byte leftY = SnesLittleEndian.HighByte(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 0, y));
        byte leftX = SnesLittleEndian.HighByte(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 0, unchecked((ushort)(y - 1))));
        byte rightY = SnesLittleEndian.HighByte(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 2, y));
        byte rightX = SnesLittleEndian.HighByte(ProjectileTrailCoordinateDefinitions.ReadCoordinateWord(bus, 1, y));
        trail.Left.Position = trail.Left.Position.WithX(AddSignedOffset(projectile.Position.X.Whole, leftX, -4)).WithY(AddSignedOffset(projectile.Position.Y.Whole, leftY, -4));
        trail.Right.Position = trail.Right.Position.WithX(AddSignedOffset(projectile.Position.X.Whole, rightX, -4)).WithY(AddSignedOffset(projectile.Position.Y.Whole, rightY, -4));
    }

    /// <summary>Reads the native word immediately before the shot's next instruction pointer.</summary>
    internal static ushort GetTrailAnimationFrame(SamusProjectileSlot projectile)
    {
        // $93:81D8-$81E3 unconditionally reads (instruction pointer - 8) + 6.
        // The upstream C port's timer-one lookahead is absent from the pinned ROM.
        ushort frameAddress = unchecked((ushort)(projectile.InstructionPointer - 2));
        return SamusProjectileInstructionDefinitions.ReadWord(SamusProjectileRomData.Banks.Projectile | frameAddress);
    }

    /// <summary>Applies a signed byte displacement and fixed bias with 16-bit position wrap.</summary>
    private static ushort AddSignedOffset(ushort origin, byte encodedOffset, int constant) =>
        unchecked((ushort)(origin + unchecked((sbyte)encodedOffset) + constant));

    /// <summary>Advances one side's instruction stream unless frozen, then draws only in-range screen coordinates.</summary>
    internal static void HandleTrailSideAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileTrailSlot pair,
        Position16<RoomPixels> layer1,
        bool timeIsFrozen,
        bool isLeft,
        SuperMetroid.Core.Samus.Artwork.ProjectileTrailCatalog? artwork)
    {
        SamusProjectileTrailSide side = isLeft ? pair.Left : pair.Right;
        if (side.InstructionTimer.IsZero)
            return;

        if (!timeIsFrozen)
        {
            side.InstructionTimer = side.InstructionTimer.Decrement().Remaining;
            if (side.InstructionTimer.IsZero)
            {
                ushort pointer = side.InstructionPointer;
                while (true)
                {
                    ushort instructionOrTimer = ProjectileTrailProgramDefinitions.Read(
                        bus,
                        SamusProjectileRomData.Banks.Movement | pointer);
                    if (!SamusProjectileInstructionDefinitions.IsInstruction(instructionOrTimer))
                    {
                        side.InstructionTimer = NativeCountdown.FromRaw(instructionOrTimer);
                        if (instructionOrTimer == 0)
                            return;

                        side.TileNumberAttributes = artwork is not null ? artwork.Resolve(pointer) :
                            ProjectileTrailProgramDefinitions.Read(bus,
                                SamusProjectileRomData.Banks.Movement |
                                unchecked((ushort)(pointer + 2)));
                        side.InstructionPointer = unchecked((ushort)(pointer + 4));
                        break;
                    }

                    // Bank $90 stores executable instruction addresses inline. Each handler
                    // returns to the parser with X already advanced past its one-word opcode.
                    pointer = unchecked((ushort)(pointer + 2));
                    if (!Enum.IsDefined((ProjectileTrailInstruction)instructionOrTimer))
                    {
                        throw new InvalidOperationException(
                            $"Unsupported {(isLeft ? "left" : "right")} projectile-trail " +
                            $"instruction ${instructionOrTimer:X4} at $90:{unchecked((ushort)(pointer - 2)):X4}.");
                    }
                    switch ((ProjectileTrailInstruction)instructionOrTimer)
                    {
                        // The opcode names its destination array. Both streams dispatch
                        // the same handlers, including writes to their sibling's position.
                        case ProjectileTrailInstruction.MoveLeftDown:
                            pair.Left.Position = pair.Left.Position.WithY(unchecked((ushort)(pair.Left.Position.Y + 1)));
                            break;
                        case ProjectileTrailInstruction.MoveRightDown:
                            pair.Right.Position = pair.Right.Position.WithY(unchecked((ushort)(pair.Right.Position.Y + 1)));
                            break;
                        case ProjectileTrailInstruction.MoveLeftUp:
                            pair.Left.Position = pair.Left.Position.WithY(unchecked((ushort)(pair.Left.Position.Y - 1)));
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined {nameof(ProjectileTrailInstruction)} {instructionOrTimer:X4}.");
                    }
                }
            }
        }

        // `$90:B6F4/$B703` require both complete 16-bit camera-relative coordinates to have
        // a zero high byte. Unlike the generic spritemap path, negative or 256+ coordinates
        // are skipped rather than wrapped or parked.
        ushort screenX = unchecked((ushort)(side.Position.X - layer1.X));
        ushort screenY = unchecked((ushort)(side.Position.Y - layer1.Y));
        if ((SnesLittleEndian.HighByteInPlace(screenX)) != 0 || (SnesLittleEndian.HighByteInPlace(screenY)) != 0)
            return;

        oam.AddProjectileTrailSprite(
            new Position16<OamPixels>(unchecked((byte)screenX), unchecked((byte)screenY)),
            artwork is null ? side.TileNumberAttributes : artwork.ResolveCurrent(side.InstructionPointer, side.TileNumberAttributes));
    }
}

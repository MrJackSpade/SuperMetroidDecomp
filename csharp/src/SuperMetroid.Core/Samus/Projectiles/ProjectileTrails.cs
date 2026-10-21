using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile trails: spawning a trail behind a shot, its animation frame, and drawing each side.</summary>
internal sealed class ProjectileTrails
{
    /// <summary>Projectile-owned left and right trail pairs shared with the slot allocator.</summary>
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>The installed cartridge bytes a trail reads past its selector and coordinate tables; null until bound.</summary>
    [field: NonSerialized]
    internal SamusOverreadCatalog? Overreads { get; set; }

    /// <summary>Uses the owning projectile table so trail allocation and drawing see the same slots.</summary>
    internal ProjectileTrails(ProjectileSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Selects native trail lists and positions both sides from the shot's current animation record.</summary>
    internal void SpawnTrail(SamusProjectileSlot projectile)
    {
        int pointerIndex;
        if (projectile.PackedType.IsFamily(SamusProjectileFamily.Beam))
        {
            // Beam indices retain charge/SBA bits in the low six bits. Charged plain power
            // therefore selects entry $10, not ordinary-power entry zero.
            pointerIndex = projectile.Type & 63;
        }
        else
        {
            // Missiles and supers map families $01/$02 to table entries $20/$21. Other
            // projectile families return carry set and do not consume a trail slot.
            int family = SamusProjectileFamilies.IndexOf(projectile.PackedType.Raw);
            if (family >= 3)
                return;
            pointerIndex = family + 31;
        }

        ProjectileTrailProgram? leftProgram = ProjectileTrailDefinitions.Select(pointerIndex, HorizontalDirection.Left);
        ProjectileTrailProgram? rightProgram = ProjectileTrailDefinitions.Select(pointerIndex, HorizontalDirection.Right);

        // Retail reads the previously installed record, even when the animation timer
        // will expire later this frame. Reading ahead advances Spazer's trail spread early.
        ushort animationFrame = GetTrailAnimationFrame(projectile);
        int direction = projectile.PackedDirection.DirectionIndex;
        int familyTable = (projectile.Type & 1 << 5) != 0
            ? SamusProjectileRomData.Trails.SpazerSbaOffsetFamilies
            : projectile.PackedType.IsChargedBeam
                ? SamusProjectileRomData.Trails.ChargedOffsetFamilies
                : SamusProjectileRomData.Trails.UnchargedOffsetFamilies;
        ushort directionTable = ProjectileTrailCoordinateDefinitions.ReadFamilyPointer(
            familyTable + projectile.PackedType.BeamCombination.TableIndex * 2, Overreads);

        // A selection that names no program draws no trail. An empty trail family (a glitch beam) points its direction
        // list or coordinate operands into the low half of bank $9B, which holds direct-page scratch. Its exact appearance
        // would need direct-page emulation the port does not model, so it is exempt (owner decision on #1318) and draws no
        // trail. Native performs WORD reads and consumes their high bytes after XBA.
        if (leftProgram is not { } left || rightProgram is not { } right ||
            !ProjectileTrailCoordinateDefinitions.TryReadDirectionPointer(
                SamusProjectileRomData.Banks.PaletteAndTrailData | unchecked((ushort)(directionTable + direction * 2)),
                Overreads, out ushort offsetList))
            return;
        ushort y = (ushort)(SamusProjectileRomData.Banks.PaletteAndTrailData | unchecked((ushort)(offsetList + animationFrame * 4)));
        if (!ProjectileTrailCoordinateDefinitions.TryReadCoordinateWord(0, y, Overreads, out ushort leftYWord) ||
            !ProjectileTrailCoordinateDefinitions.TryReadCoordinateWord(0, unchecked((ushort)(y - 1)), Overreads, out ushort leftXWord) ||
            !ProjectileTrailCoordinateDefinitions.TryReadCoordinateWord(2, y, Overreads, out ushort rightYWord) ||
            !ProjectileTrailCoordinateDefinitions.TryReadCoordinateWord(1, y, Overreads, out ushort rightXWord))
            return;
        byte leftY = SnesLittleEndian.HighByte(leftYWord);
        byte leftX = SnesLittleEndian.HighByte(leftXWord);
        byte rightY = SnesLittleEndian.HighByte(rightYWord);
        byte rightX = SnesLittleEndian.HighByte(rightXWord);

        SamusProjectileTrailSlot? trail = null;
        for (int slotIndex = SamusProjectileSystem.TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            // The original tests only the left timer. A right stream can still be alive in
            // malformed/debug-edited state and will be overwritten when the left sentinel is
            // clear; retain that asymmetric allocation contract rather than being "safer".
            if (_slotTable.TrailSlots[slotIndex].Left.InstructionTimer.IsZero)
            {
                trail = _slotTable.TrailSlots[slotIndex];
                break;
            }
        }
        if (trail is null)
            return;

        trail.Left.InstructionTimer = WrappingCountdown.Start(1);
        trail.Right.InstructionTimer = WrappingCountdown.Start(1);
        trail.Left.Cursor = new ProjectileTrailCursor(left, 0);
        trail.Right.Cursor = new ProjectileTrailCursor(right, 0);
        trail.Left.Position = trail.Left.Position.WithX(AddSignedOffset(projectile.Position.X.Whole, leftX, -4)).WithY(AddSignedOffset(projectile.Position.Y.Whole, leftY, -4));
        trail.Right.Position = trail.Right.Position.WithX(AddSignedOffset(projectile.Position.X.Whole, rightX, -4)).WithY(AddSignedOffset(projectile.Position.Y.Whole, rightY, -4));
    }

    /// <summary>Reads the native word immediately before the shot's next instruction pointer.</summary>
    internal static ushort GetTrailAnimationFrame(SamusProjectileSlot projectile)
    {
        // $93:81D8-$81E3 unconditionally reads (instruction pointer - 8) + 6.
        // The upstream C port's timer-one lookahead is absent from the pinned ROM.
        return SamusProjectilePrograms.PrecedingAnimationFrame(projectile.Cursor
            ?? throw new InvalidOperationException("A trail read the animation frame of an inactive projectile."));
    }

    /// <summary>Applies a signed byte displacement and fixed bias with 16-bit position wrap.</summary>
    private static ushort AddSignedOffset(ushort origin, byte encodedOffset, int constant) =>
        unchecked((ushort)(origin + unchecked((sbyte)encodedOffset) + constant));

    /// <summary>Advances one side's instruction stream unless time is frozen.</summary>
    internal static void AdvanceTrailSide(
        SamusProjectileTrailSlot pair,
        bool timeIsFrozen,
        HorizontalDirection direction,
        SuperMetroid.Core.Samus.Artwork.ProjectileTrailCatalog? artwork)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        if (side.InstructionTimer.IsZero)
            return;

        if (!timeIsFrozen)
        {
            side.InstructionTimer = side.InstructionTimer.Decrement().Remaining;
            if (side.InstructionTimer.IsZero)
            {
                ProjectileTrailCursor cursor = side.Cursor;
                while (true)
                {
                    ProjectileTrailStep step = cursor.Current;
                    if (step is ProjectileTrailStep.End)
                    {
                        // The end leaves the timer zero and the cursor on itself.
                        side.InstructionTimer = WrappingCountdown.FromRaw(0);
                        return;
                    }
                    if (step is ProjectileTrailStep.Pose pose)
                    {
                        side.InstructionTimer = WrappingCountdown.FromRaw(pose.Duration);
                        side.TileNumberAttributes = (artwork ?? throw new InvalidOperationException(
                            "A trail pose needs the installed trail artwork.")).Resolve(pose.Frame);
                        side.Cursor = cursor.Next();
                        break;
                    }

                    // A move continues with the next step in the same update.
                    cursor = cursor.Next();
                    ProjectileTrailInstruction movement = ((ProjectileTrailStep.Move)step).Movement;
                    switch (movement)
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
                            throw new InvalidOperationException($"Undefined {nameof(ProjectileTrailInstruction)} {movement}.");
                    }
                }
            }
        }

        side.DisplayedPosition = side.Position;
    }

    /// <summary>Publishes one live side at in-range screen coordinates only.</summary>
    internal static void DrawTrailSide(
        SpriteFrameBuffer sprites,
        SamusProjectileTrailSlot pair,
        int slotIndex,
        Position16<RoomPixels> layer1,
        HorizontalDirection direction,
        SuperMetroid.Core.Samus.Artwork.ProjectileTrailCatalog? artwork,
        ProjectileDrawArtwork objects)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        // A finished side, including one that ended in this frame's advance, shows nothing.
        if (side.InstructionTimer.IsZero)
            return;

        // `$90:B6F4/$B703` require both complete 16-bit camera-relative coordinates to have
        // a zero high byte. Unlike the generic spritemap path, negative or 256+ coordinates
        // are skipped rather than wrapped or parked.
        ushort screenX = unchecked((ushort)(side.DisplayedPosition.X - layer1.X));
        ushort screenY = unchecked((ushort)(side.DisplayedPosition.Y - layer1.Y));
        if ((SnesLittleEndian.HighByteInPlace(screenX)) != 0 || (SnesLittleEndian.HighByteInPlace(screenY)) != 0)
            return;

        SnesObjAttributeWord attributes = artwork is null
            ? side.TileNumberAttributes : artwork.ResolveCurrent(side.Cursor, side.TileNumberAttributes);
        sprites.Add(new SpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectileTrail, slotIndex * 2 + (direction == HorizontalDirection.Left ? 0 : 1)),
            ObjPartFrames.SmallObject(attributes, 0), objects.Page,
            new Position16<ScreenPixels>(screenX, screenY), default, 0, 0));
    }
}

using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile trails: spawning a trail behind a shot, its animation frame, and drawing each side.</summary>
internal sealed class ProjectileTrails
{
    /// <summary>Projectile-owned left and right trail pairs shared with the slot allocator.</summary>
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>The installed placements a trail reads past its placement list's records; null until bound.</summary>
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
        ProjectileTrailFamilyGroup group = (projectile.Type & 1 << 5) != 0
            ? ProjectileTrailFamilyGroup.SpecialBeamAttack
            : projectile.PackedType.IsChargedBeam ? ProjectileTrailFamilyGroup.Charged : ProjectileTrailFamilyGroup.Uncharged;
        ProjectileTrailPlacementList? list = ProjectileTrailPlacements.List(
            group, projectile.PackedType.BeamCombination, projectile.PackedDirection.DirectionIndex);

        // A selection that names no program or placement list draws no trail, and so does a read that reaches direct-page
        // scratch: its exact appearance would need direct-page emulation the port does not model, so it is exempt (owner
        // decision on #1318).
        if (leftProgram is not { } left || rightProgram is not { } right || list is not { } placements ||
            ProjectileTrailPlacements.Start(placements, animationFrame, Overreads) is not { } placement)
            return;

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
        trail.Left.Position = trail.Left.Position.WithX(AddOffset(projectile.Position.X.Whole, placement.Left.X))
            .WithY(AddOffset(projectile.Position.Y.Whole, placement.Left.Y));
        trail.Right.Position = trail.Right.Position.WithX(AddOffset(projectile.Position.X.Whole, placement.Right.X))
            .WithY(AddOffset(projectile.Position.Y.Whole, placement.Right.Y));
    }

    /// <summary>Reads the native word immediately before the shot's next instruction pointer.</summary>
    internal static ushort GetTrailAnimationFrame(SamusProjectileSlot projectile)
    {
        // $93:81D8-$81E3 unconditionally reads (instruction pointer - 8) + 6.
        // The upstream C port's timer-one lookahead is absent from the pinned ROM.
        return SamusProjectilePrograms.PrecedingAnimationFrame(projectile.Cursor
            ?? throw new InvalidOperationException("A trail read the animation frame of an inactive projectile."));
    }

    /// <summary>Places a sparkle <paramref name="offset"/> pixels from <paramref name="origin"/>, drawn four pixels up and left, with 16-bit position wrap.</summary>
    private static ushort AddOffset(ushort origin, short offset) => unchecked((ushort)(origin + offset - 4));

    /// <summary>Advances one side's instruction stream unless time is frozen.</summary>
    internal static void AdvanceTrailSide(
        SamusProjectileTrailSlot pair,
        bool timeIsFrozen,
        HorizontalDirection direction)
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
                        side.ShownPose = pose.Frame;
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

        // A side allocated over an earlier trail keeps showing that trail's last pose until its own first one.
        SpriteSheetFrame image = objects.Sheet.Frame(side.ShownPose is { } pose
            ? ProjectileSheetDefinitions.TrailFrame(pose) : ProjectileSheetDefinitions.UnshownTrailFrame);
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectileTrail, slotIndex * 2 + (direction == HorizontalDirection.Left ? 0 : 1)),
            image, new Position16<ScreenPixels>(screenX, screenY), SpriteMirror.None, image.Priority));
    }
}

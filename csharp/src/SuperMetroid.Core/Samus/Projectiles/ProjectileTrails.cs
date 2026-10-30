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
    internal InstalledSamusOverreads? Overreads { get; set; }

    /// <summary>Uses the owning projectile table so trail allocation and drawing see the same slots.</summary>
    internal ProjectileTrails(ProjectileSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Selects native trail lists and positions both sides from the shot's current animation record.</summary>
    internal void SpawnTrail(SamusProjectileSlot projectile)
    {
        int trailEntry;
        switch (projectile.Type.Family)
        {
            case SamusProjectileFamily.Beam:
                // Beams select by their beam-table row, so charged plain power selects
                // the charged row, not ordinary power's.
                trailEntry = projectile.Type.BeamTableRow;
                break;
            case SamusProjectileFamily.Missile:
                trailEntry = ProjectileTrailDefinitions.FirstSpecialSelection;
                break;
            case SamusProjectileFamily.SuperMissile:
                trailEntry = ProjectileTrailDefinitions.FirstSpecialSelection + 1;
                break;
            case SamusProjectileFamily.PowerBomb:
            case SamusProjectileFamily.Bomb:
            case SamusProjectileFamily.BeamExplosion:
            case SamusProjectileFamily.MissileExplosion:
                // Other families do not consume a trail slot.
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusProjectileFamily)} {projectile.Type.Family}.");
        }

        ProjectileTrailProgram? leftProgram = ProjectileTrailSelection.Select(trailEntry, HorizontalDirection.Left);
        ProjectileTrailProgram? rightProgram = ProjectileTrailSelection.Select(trailEntry, HorizontalDirection.Right);

        // Retail reads the previously installed record, even when the animation timer
        // will expire later this frame. Reading ahead advances Spazer's trail spread early.
        int animationFrame = GetTrailAnimationFrame(projectile, Overreads);
        ProjectileTrailFamilyGroup group = projectile.Type.Echo is not null
            ? ProjectileTrailFamilyGroup.SpecialBeamAttack
            : projectile.Type.IsChargedBeam ? ProjectileTrailFamilyGroup.Charged : ProjectileTrailFamilyGroup.Uncharged;
        ProjectileTrailPlacementList? list = ProjectileTrailPlacements.List(
            group, projectile.Type.BeamCombination, projectile.Direction.ToAimIndex());

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
    internal static ushort GetTrailAnimationFrame(SamusProjectileSlot projectile, InstalledSamusOverreads? overreads)
    {
        // $93:81D8-$81E3 unconditionally reads (instruction pointer - 8) + 6.
        // The upstream C port's timer-one lookahead is absent from the pinned ROM.
        return SamusProjectilePrograms.PrecedingAnimationFrame(projectile.Cursor
            ?? throw new InvalidOperationException("A trail read the animation frame of an inactive projectile."), overreads);
    }

    /// <summary>Places a sparkle <paramref name="offset"/> pixels from <paramref name="origin"/>, drawn four pixels up and left, with 16-bit position wrap.</summary>
    private static ushort AddOffset(int origin, int offset) => unchecked((ushort)(origin + offset - 4));

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
                ProjectileTrailCursor cursor = side.Cursor ?? throw new InvalidOperationException(
                    "A trail side with a running timer has started a program.");
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
        int screenX = unchecked((ushort)(side.DisplayedPosition.X - layer1.X));
        int screenY = unchecked((ushort)(side.DisplayedPosition.Y - layer1.Y));
        if (ScreenConversions.IsOutsideSpriteRange(screenX) || ScreenConversions.IsOutsideSpriteRange(screenY))
            return;

        // A side allocated over an earlier trail keeps showing that trail's last pose until its own first one.
        SpriteSheetFrame image = objects.Sheet.Frame(side.ShownPose is { } pose
            ? ProjectileSheetFrames.TrailFrame(pose) : ProjectileSheetDefinitions.UnshownTrailFrame);
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectileTrail, slotIndex * 2 + (direction == HorizontalDirection.Left ? 0 : 1)),
            image, Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
    }
}

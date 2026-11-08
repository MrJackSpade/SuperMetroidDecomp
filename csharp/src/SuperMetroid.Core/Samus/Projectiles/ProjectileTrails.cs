using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile trails: spawning a trail behind a shot, its animation frame, and drawing each side.</summary>
internal sealed class ProjectileTrails
{
    /// <summary>Projectile-owned left and right trail pairs shared with the slot allocator.</summary>
    private readonly ProjectileSlotTable _slotTable;

    /// <summary>Where a trail starts when its read passes its placement list.</summary>
    private readonly TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> _starts;

    /// <summary>Uses the owning projectile table so trail allocation and drawing see the same slots.</summary>
    internal ProjectileTrails(ProjectileSlotTable slotTable,
        TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> starts)
    {
        _slotTable = slotTable;
        _starts = Ensure.NotNull(starts);
    }

    /// <summary>Selects trail animations and positions both sides from the shot's current animation record.</summary>
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

        ProjectileTrailProgram left = ProjectileTrailSelection.Select(trailEntry, HorizontalDirection.Left);
        ProjectileTrailProgram right = ProjectileTrailSelection.Select(trailEntry, HorizontalDirection.Right);

        // Retail reads the previously installed record, even when the animation timer
        // will expire later this frame. Reading ahead advances Spazer's trail spread early.
        int animationFrame = GetTrailAnimationFrame(projectile, _starts);
        ProjectileTrailFamilyGroup group = projectile.Type.Echo is not null
            ? ProjectileTrailFamilyGroup.SpecialBeamAttack
            : projectile.Type.IsChargedBeam ? ProjectileTrailFamilyGroup.Charged : ProjectileTrailFamilyGroup.Uncharged;
        ProjectileTrailPlacementList? list = ProjectileTrailPlacements.List(
            group, projectile.Type.BeamCombination, projectile.Direction.ToAimIndex());

        // A shot whose direction names no placement list draws no trail, and so does a row whose list would be
        // left-over values: that appearance is not part of the game's design, so it is exempt (owner decision on #1318).
        if (list is not { } placements ||
            ProjectileTrailPlacements.Start(placements, animationFrame, _starts) is not { } placement)
            return;

        // The search tests only the left timer. A right stream can still be alive in malformed or debug-edited state and
        // is overwritten when the left side is free; retain that asymmetric allocation rather than being "safer".
        SamusProjectileTrailSlot? trail = _slotTable.TrailsInUpdateOrder.FirstOrDefault(
            candidate => candidate.Left.FrameHold == 0);
        if (trail is null)
            return;

        trail.Left.FrameHold = 1;
        trail.Right.FrameHold = 1;
        trail.Left.Cursor = new ProjectileTrailCursor(left, 0);
        trail.Right.Cursor = new ProjectileTrailCursor(right, 0);
        trail.Left.Position = trail.Left.Position.WithX(AddOffset(projectile.Position.X.Whole, placement.Left.X))
            .WithY(AddOffset(projectile.Position.Y.Whole, placement.Left.Y));
        trail.Right.Position = trail.Right.Position.WithX(AddOffset(projectile.Position.X.Whole, placement.Right.X))
            .WithY(AddOffset(projectile.Position.Y.Whole, placement.Right.Y));
    }

    /// <summary>
    /// The animation frame a trail reads from <paramref name="projectile"/>: the frame of the record its animation last
    /// installed, or for a list that has just started the frame <paramref name="starts"/> decoded for it.
    /// </summary>
    internal static int GetTrailAnimationFrame(SamusProjectileSlot projectile,
        TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> starts)
    {
        // The trail reads the record before the animation's next position, not a lookahead at the next phase.
        return SamusProjectilePrograms.PrecedingAnimationFrame(projectile.Cursor
            ?? throw new InvalidOperationException("A trail read the animation frame of an inactive projectile."), starts);
    }

    /// <summary>Places a sparkle <paramref name="offset"/> pixels from <paramref name="origin"/>, drawn four pixels up and left, with 16-bit position wrap.</summary>
    private static int AddOffset(int origin, int offset) => CoordinateAxis.Wrap(origin + offset - 4);

    /// <summary>Advances one side's instruction stream unless time is frozen.</summary>
    internal static void AdvanceTrailSide(
        SamusProjectileTrailSlot pair,
        bool timeIsFrozen,
        HorizontalDirection direction)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        if (side.FrameHold == 0)
            return;

        if (!timeIsFrozen)
        {
            side.FrameHold--;
            if (side.FrameHold == 0)
            {
                ProjectileTrailCursor cursor = side.Cursor ?? throw new InvalidOperationException(
                    "A trail side with a running timer has started a program.");
                while (true)
                {
                    ProjectileTrailStep step = cursor.Current;
                    if (step is ProjectileTrailStep.End)
                    {
                        // The end leaves the timer zero and the cursor on itself.
                        side.FrameHold = 0;
                        return;
                    }
                    if (step is ProjectileTrailStep.Pose pose)
                    {
                        side.FrameHold = pose.Duration;
                        side.ShownPose = pose.Frame;
                        side.Cursor = cursor.Next();
                        break;
                    }

                    // A move continues with the next step in the same update.
                    cursor = cursor.Next();
                    ProjectileTrailInstruction movement = ((ProjectileTrailStep.Move)step).Movement;
                    switch (movement)
                    {
                        // The instruction names its destination array. Both streams dispatch
                        // the same handlers, including writes to their sibling's position.
                        case ProjectileTrailInstruction.MoveLeftDown:
                            pair.Left.Position = pair.Left.Position.WithY(CoordinateAxis.Wrap(pair.Left.Position.Y + 1));
                            break;
                        case ProjectileTrailInstruction.MoveRightDown:
                            pair.Right.Position = pair.Right.Position.WithY(CoordinateAxis.Wrap(pair.Right.Position.Y + 1));
                            break;
                        case ProjectileTrailInstruction.MoveLeftUp:
                            pair.Left.Position = pair.Left.Position.WithY(CoordinateAxis.Wrap(pair.Left.Position.Y - 1));
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
        Position16<RoomPixels> cameraPosition,
        HorizontalDirection direction,
        ProjectileDrawArtwork objects)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        // A finished side, including one that ended in this frame's advance, shows nothing.
        if (side.FrameHold == 0)
            return;

        // The trail is published wherever it lies; the display clips it to the view (#1541).
        int screenX = CoordinateAxis.Wrap(side.DisplayedPosition.X - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(side.DisplayedPosition.Y - cameraPosition.Y);

        // A side allocated over an earlier trail keeps showing that trail's last pose until its own first one.
        SpriteSheetFrame image = objects.Sheet.Frame(side.ShownPose is { } pose
            ? ProjectileSheetFrames.TrailFrame(pose) : ProjectileSheetDefinitions.UnshownTrailFrame);
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectileTrail, slotIndex * 2 + (direction == HorizontalDirection.Left ? 0 : 1)),
            image, Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
    }
}

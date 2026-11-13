using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The trails Samus's shots leave (#1625): presentation particles, each a pair of sparkles running its own animations until
/// both end. A new trail never displaces one already shown, and nothing in gameplay reads a trail.
/// </summary>
internal sealed class ProjectileTrails
{
    /// <summary>The live trails, oldest first.</summary>
    internal readonly List<SamusProjectileTrail> trails = [];

    /// <summary>Removes every trail, as clearing Samus's shots does.</summary>
    internal void Clear() => trails.Clear();

    /// <summary>Adds a trail already under way, as a restored game does; a spawned trail is added the same way.</summary>
    internal void Add(SamusProjectileTrail trail) => trails.Add(Ensure.NotNull(trail));

    /// <summary>
    /// Leaves a trail behind <paramref name="projectile"/>: selects both sparkles' animations from the shot's beams and
    /// places them from the placement its shown animation frame selects in its trail list.
    /// </summary>
    internal void SpawnTrail(SamusProjectile projectile)
    {
        switch (projectile.Type.Family)
        {
            case SamusProjectileFamily.Beam:
            case SamusProjectileFamily.Missile:
            case SamusProjectileFamily.SuperMissile:
                break;
            case SamusProjectileFamily.PowerBomb:
            case SamusProjectileFamily.Bomb:
            case SamusProjectileFamily.BeamExplosion:
            case SamusProjectileFamily.MissileExplosion:
                // Other families leave no trail.
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusProjectileFamily)} {projectile.Type.Family}.");
        }

        // Beams select by their beams and charge, so charged plain power selects the charged trail, not ordinary power's.
        ProjectileTrailAnimations animations = ProjectileTrailSelection.Select(projectile.Type);

        ProjectileTrailFamilyGroup group = projectile.Type.Echo is not null
            ? ProjectileTrailFamilyGroup.SpecialBeamAttack
            : projectile.Type.IsChargedBeam ? ProjectileTrailFamilyGroup.Charged : ProjectileTrailFamilyGroup.Uncharged;
        ProjectileTrailPlacementList? list = ProjectileTrailPlacements.List(
            group, projectile.Type.BeamCombination, projectile.Direction);

        // A shot whose direction names no placement list leaves no trail, and neither does a shot whose list has shown no
        // frame yet or whose frame lies past its list's placements: those placements are not part of the game's design
        // (owner decision on #1318, #1625).
        if (list is not { } placements ||
            GetTrailAnimationFrame(projectile) is not { } animationFrame ||
            ProjectileTrailPlacements.Start(placements, animationFrame) is not { } placement)
            return;

        var trail = new SamusProjectileTrail(projectile.PicturedBeams);
        StartSide(trail.Left, animations.Left, projectile.Position.Whole, placement.Left);
        StartSide(trail.Right, animations.Right, projectile.Position.Whole, placement.Right);
        Add(trail);
    }

    /// <summary>
    /// Starts <paramref name="side"/> on <paramref name="segment"/>, which it begins at its next update, placed
    /// <paramref name="offset"/> from the shot at <paramref name="origin"/>.
    /// </summary>
    private static void StartSide(SamusProjectileTrailSide side, ProjectileTrailSegment segment, Position16<RoomPixels> origin, Offset16<RoomPixels> offset)
    {
        side.FrameHold = 1;
        side.Cursor = new ProjectileTrailCursor(segment, 0);
        side.Position = Position16<RoomPixels>.Of(AddOffset(origin.X, offset.X), AddOffset(origin.Y, offset.Y));
    }

    /// <summary>
    /// The animation frame a trail reads from <paramref name="projectile"/>: the frame of the record its animation last
    /// installed, or null before its list has installed one.
    /// </summary>
    internal static int? GetTrailAnimationFrame(SamusProjectile projectile) =>
        SamusProjectilePlayback.ShownAnimationFrame(projectile.Playback
            ?? throw new InvalidOperationException("A trail read the animation frame of an inactive projectile."));

    /// <summary>Places a sparkle <paramref name="offset"/> pixels from <paramref name="origin"/>, drawn four pixels up and left, with the position wrapping at 65536.</summary>
    private static int AddOffset(int origin, int offset) => CoordinateAxis.Wrap(origin + offset - 4);

    /// <summary>
    /// Advances every trail one update, left sparkle before right, unless time is frozen, and removes the trails whose
    /// sparkles have both ended.
    /// </summary>
    internal void Advance(bool timeIsFrozen)
    {
        foreach (SamusProjectileTrail trail in trails)
        {
            AdvanceTrailSide(trail, timeIsFrozen, HorizontalDirection.Left);
            AdvanceTrailSide(trail, timeIsFrozen, HorizontalDirection.Right);
        }
        _ = trails.RemoveAll(static trail => trail.IsFinished);
    }

    /// <summary>Advances one side's cue stream unless time is frozen.</summary>
    private static void AdvanceTrailSide(
        SamusProjectileTrail pair,
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
                    ProjectileTrailCue movement = ((ProjectileTrailStep.Move)step).Movement;
                    switch (movement)
                    {
                        // The cue names the side it moves. Both streams run the same moves, including
                        // moves of their sibling.
                        case ProjectileTrailCue.MoveLeftDown:
                            pair.Left.Position = pair.Left.Position.WithY(CoordinateAxis.Wrap(pair.Left.Position.Y + 1));
                            break;
                        case ProjectileTrailCue.MoveRightDown:
                            pair.Right.Position = pair.Right.Position.WithY(CoordinateAxis.Wrap(pair.Right.Position.Y + 1));
                            break;
                        case ProjectileTrailCue.MoveLeftUp:
                            pair.Left.Position = pair.Left.Position.WithY(CoordinateAxis.Wrap(pair.Left.Position.Y - 1));
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined {nameof(ProjectileTrailCue)} {movement}.");
                    }
                }
            }
        }

        side.DisplayedPosition = side.Position;
    }

    /// <summary>
    /// Publishes every live sparkle, the newest trail first, so a newer trail draws in front where trails overlap, and
    /// each trail's left sparkle before its right.
    /// </summary>
    internal void Draw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork objects)
    {
        Ensure.NotNull(sprites);
        for (int index = trails.Count - 1; index >= 0; index--)
        {
            DrawTrailSide(sprites, trails[index], index, cameraPosition, HorizontalDirection.Left, objects);
            DrawTrailSide(sprites, trails[index], index, cameraPosition, HorizontalDirection.Right, objects);
        }
    }

    /// <summary>Publishes one side wherever it lies, once it shows a pose and until its animation ends.</summary>
    private static void DrawTrailSide(
        SpriteFrameBuffer sprites,
        SamusProjectileTrail pair,
        int trailIndex,
        Position16<RoomPixels> cameraPosition,
        HorizontalDirection direction,
        ProjectileDrawArtwork objects)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        // A finished side, including one that ended in this frame's advance, shows nothing, and so does a side whose
        // animation has shown no pose yet.
        if (side.FrameHold == 0 || side.ShownPose is not { } pose)
            return;

        // The trail is published wherever it lies; the display clips it to the view (#1541).
        int screenX = CoordinateAxis.Wrap(side.DisplayedPosition.X - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(side.DisplayedPosition.Y - cameraPosition.Y);

        SpriteSheetFrame image = objects.SheetOf(pair.PicturedBeams).Frame(ProjectileSheetFrames.TrailFrame(pose));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectileTrail, trailIndex * 2 + (direction == HorizontalDirection.Left ? 0 : 1)),
            image, Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
    }
}

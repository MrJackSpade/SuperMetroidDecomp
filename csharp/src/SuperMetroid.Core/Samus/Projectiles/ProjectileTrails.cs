using SuperMetroid.Core.Samus.Colours;
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
    /// <summary>
    /// The trail lists a shot picks by its beam's charge (#2560): the one declared map between the projectile type's charge
    /// and the trail family group named for it.
    /// </summary>
    private static readonly System.Collections.Frozen.FrozenDictionary<bool, ProjectileTrailFamilyGroup> GroupByCharge =
        System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(new Dictionary<bool, ProjectileTrailFamilyGroup>
        {
            [false] = ProjectileTrailFamilyGroup.Uncharged,
            [true] = ProjectileTrailFamilyGroup.Charged,
        });

    /// <summary>The live trails, oldest first.</summary>
    private readonly List<SamusProjectileTrail> trails = [];

    /// <summary>Removes every trail, as clearing Samus's shots does.</summary>
    internal void Clear() => trails.Clear();

    /// <summary>Adds a trail already under way, as a restored game does; a spawned trail is added the same way.</summary>
    private void Add(SamusProjectileTrail trail) => trails.Add(Ensure.NotNull(trail));

    /// <summary>Whether a projectile of each family leaves a trail (#2553): beams and missiles do; the other families leave none.</summary>
    private static IdentityTable<SamusProjectileFamily, bool> LeavesTrail => field ??= new(new Dictionary<SamusProjectileFamily, bool>
    {
        [SamusProjectileFamily.Beam] = true,
        [SamusProjectileFamily.Missile] = true,
        [SamusProjectileFamily.SuperMissile] = true,
        [SamusProjectileFamily.PowerBomb] = false,
        [SamusProjectileFamily.Bomb] = false,
        [SamusProjectileFamily.BeamExplosion] = false,
        [SamusProjectileFamily.MissileExplosion] = false,
    });

    /// <summary>
    /// Leaves a trail behind <paramref name="projectile"/>: selects both sparkles' animations from the shot's beams and
    /// places them from the placement its trail list gives the frame it shows.
    /// </summary>
    internal void SpawnTrail(SamusProjectileOccupant held)
    {
        SamusProjectile projectile = held.Projectile;
        if (!LeavesTrail[held.Type.Family])
            return;

        // Beams select by their beams and charge, so charged plain power selects the charged trail, not ordinary power's.
        ProjectileTrailAnimations animations = ProjectileTrailSelection.Select(held.Type);

        // The only echo that leaves a trail is the Spazer special attack's trailing pair, whose trail starts from its own
        // lists (#2348); every other trail starts from its beams' family's lists, by charge.
        ProjectileTrailPlacementList? list = held.Type.Echo == SamusBeamEcho.SpazerComboTrail
            ? ProjectileTrailPlacements.SpazerAttackTrailingPairList(projectile.Direction)
            : ProjectileTrailPlacements.List(
                GroupByCharge[held.Type.IsChargedBeam],
                held.Type.BeamCombination, projectile.Direction);

        // A shot whose beams name no trail family leaves no trail, and neither does a shot whose animation has shown
        // no frame yet or whose frame its list places no trail for (#1318, #1625).
        if (list is not { } placements ||
            GetTrailFrame(projectile) is not { } frame ||
            ProjectileTrailPlacements.Start(placements, frame) is not { } placement)
            return;

        var trail = new SamusProjectileTrail(projectile.PicturedBeams, projectile.Position.Whole);
        // Each side stands where the trail list places it and runs the animation its selection names; a side where the shot
        // leaves no sparkle runs the one that ends without a sparkle (#2558).
        trail.Left.Offset = placement.Left;
        trail.Right.Offset = placement.Right;
        StartSide(trail.Left, animations.Left);
        StartSide(trail.Right, animations.Right);
        Add(trail);
    }

    /// <summary>
    /// Starts <paramref name="side"/> on <paramref name="segment"/>, which it begins at its next update where the trail
    /// list placed it from the shot; each trail frame's picture is placed about the sparkle by its own anchor (#2573). The
    /// sparkle keeps its offset from the shot's place rather than a room position of its own, so a sparkle beside a shot
    /// past the room's top or left edge simply lies beyond it.
    /// </summary>
    private static void StartSide(SamusProjectileTrailSide side, ProjectileTrailSegment segment) =>
        side.Animation = ProjectileTrailSparkleAnimations.Start(segment);

    /// <summary>
    /// The frame a trail starts from <paramref name="projectile"/> at: the frame its animation last showed, or null before
    /// its animation has shown one.
    /// </summary>
    private static SamusProjectileFrameId? GetTrailFrame(SamusProjectile projectile) =>
        (projectile.Animation ?? throw new InvalidOperationException("A trail read the frame of an inactive projectile.")).Shown;

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

    /// <summary>
    /// Counts one update off one side's frame unless time is frozen, and once the frame's updates are spent plays its
    /// animation on to the next frame, moving the sparkles its cues name on the way; an animation that ends leaves the
    /// side ended, its last frame shown no more.
    /// </summary>
    private static void AdvanceTrailSide(
        SamusProjectileTrail pair,
        bool timeIsFrozen,
        HorizontalDirection direction)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        if (side.Animation is not { } animation || animation.HasStopped)
            return;

        if (!timeIsFrozen)
        {
            animation.Step(cue => Move(pair, cue));
            if (animation.HasStopped)
                return;
        }

        side.DisplayedOffset = side.Offset;
    }

    /// <summary>
    /// Moves the sparkle <paramref name="cue"/> names a pixel down: a cue names the sparkle it moves, whichever side's
    /// animation raises it.
    /// </summary>
    private static void Move(SamusProjectileTrail pair, ProjectileTrailCue cue)
    {
        SamusProjectileTrailSide moved = cue switch
        {
            ProjectileTrailCue.MoveLeftDown => pair.Left,
            ProjectileTrailCue.MoveRightDown => pair.Right,
            _ => throw new InvalidOperationException($"Undefined {nameof(ProjectileTrailCue)} {cue}."),
        };
        moved.Offset = moved.Offset.WithY(moved.Offset.Y + 1);
    }

    /// <summary>
    /// Publishes every live sparkle, the newest trail first, so a newer trail draws in front where trails overlap, and
    /// each trail's left sparkle before its right.
    /// </summary>
    internal void Draw(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork sheets)
    {
        Ensure.NotNull(sprites);
        for (int index = trails.Count - 1; index >= 0; index--)
        {
            DrawTrailSide(sprites, trails[index], cameraPosition, HorizontalDirection.Left, sheets);
            DrawTrailSide(sprites, trails[index], cameraPosition, HorizontalDirection.Right, sheets);
        }
    }

    /// <summary>Publishes one side wherever it lies, once it shows a pose and until its animation ends.</summary>
    private static void DrawTrailSide(
        SpriteFrameBuffer sprites,
        SamusProjectileTrail pair,
        Position16<RoomPixels> cameraPosition,
        HorizontalDirection direction,
        ProjectileDrawArtwork sheets)
    {
        SamusProjectileTrailSide side = direction == HorizontalDirection.Left ? pair.Left : pair.Right;
        // A finished side, including one that ended in this frame's advance, shows nothing, and so does a side whose
        // animation has shown no frame yet.
        if (side.HasEnded || side.ShownPose is not { } pose)
            return;

        // The trail is published wherever it lies; the display clips it to the view (#1541).
        Offset16<ScreenPixels> place = pair.PlaceOnScreen(side, cameraPosition);

        SpriteSheetFrame image = sheets.SheetOf(pair.PicturedBeams).Frame(ProjectileSheetFrames.TrailFrame(pose));
        sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusProjectileTrail), image, place));
    }
}

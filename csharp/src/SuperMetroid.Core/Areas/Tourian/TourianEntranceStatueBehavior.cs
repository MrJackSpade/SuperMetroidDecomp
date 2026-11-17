using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian entrance statue species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class TourianEntranceStatueBehavior : IEnemySpecies, IEnemyEntityFactory, IGameplayShadingSource
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.TourianStatue];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new TourianEntranceStatueEntity(this, place, common);

    /// <summary>Spawns the scene's decorations and its unlock effects.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The places of the scene's placements the room has set up, in the order they were placed; each shows the scene's look.</summary>
    private readonly List<RoomEnemyPlace> _placements = [];
    /// <summary>The scene's projectiles spawned this room, its decorations and its unlock effects; each still active shows the scene's look.</summary>
    private readonly List<EnemyProjectile> _sceneProjectiles = [];
    /// <summary>The room's services, whose statue room tells the decorations how far the statue has sunk.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>The eye glow the statues' eyes show, the last a released boss lit; none before any.</summary>
    private PictureShade? _eyeGlow;

    /// <summary>
    /// The released sprite statues' blacked-out eyes, grey-out frames and greys, in the order they were shown; a
    /// grey-out's frame replaces the one before it.
    /// </summary>
    private readonly List<PictureShade> _statueLooks = [];

    /// <summary>The released statues drawn in the room's background: their blacked-out eyes and greys, in the order shown.</summary>
    private readonly List<PictureShade> _roomLooks = [];

    /// <summary>Binds projectile allocation and the room's services to the statue species.</summary>
    internal TourianEntranceStatueBehavior(EnemyProjectileEngine projectiles, EnemyRoomBinding room)
    {
        _projectiles = projectiles;
        _room = room;
    }

    /// <summary>
    /// The statue scene's look (#1512): its standard colours until the statue initializes, then its entrance colours,
    /// the last eye glow a released boss lit, and each released statue's blacked-out eyes and grey. Every statue scene
    /// sprite shows it; the game wrote these colours into colour sets the scene's sprites share.
    /// </summary>
    internal PictureShading SceneShading { get; private set; } = PictureShading.Standard;

    /// <summary>The graphics a statue scene projectile spawns with: the shared sheet in the scene's look.</summary>
    internal SpriteGraphics SceneGraphics => SpriteGraphics.SharedIn(SceneShading);

    /// <summary>Lights a released boss statue's eye <paramref name="glow"/>, over the last.</summary>
    internal void ShowEyeGlow(PictureShade glow)
    {
        _eyeGlow = glow;
        ShowScene();
    }

    /// <summary>
    /// Shows a released statue's look, its blacked-out eyes or its grey: over the room's pictures for a statue drawn in the
    /// room's background, otherwise over the scene's sprites.
    /// </summary>
    internal void ShowStatueLook(PictureShade look)
    {
        if (TourianStatueShades.RoomPictureLooks.Contains(look))
        {
            _roomLooks.Add(look);
            return;
        }
        _statueLooks.Add(look);
        ShowScene();
    }

    /// <summary>
    /// Shows <paramref name="frame"/> of a sprite statue's grey-out, <paramref name="colours"/> naming the Ridley or
    /// Phantoon statue, over the scene's sprites (#1512): each frame replaces the one its grey-out showed before.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="colours"/> names no sprite statue.</exception>
    internal void ShowStatueGreyOut(ColourEffectColours colours, ColourEffectFrame frame)
    {
        if (colours is not (ColourEffectColours.RidleyStatue or ColourEffectColours.PhantoonStatue))
            throw new ArgumentOutOfRangeException(nameof(colours), colours, "Only the Ridley and Phantoon statues grey out as sprites.");
        PictureShade shade = ColourEffectShades.Of(colours, frame);
        _statueLooks.RemoveAll(earlier => earlier.Effect == shade.Effect);
        _statueLooks.Add(shade);
        ShowScene();
    }

    /// <summary>Lays the looks of the released statues drawn in the room's background over the room's pictures (#1512).</summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade look in _roomLooks)
            frame.Shade(GameplayPictures.Room, look);
    }

    /// <summary>
    /// Recomposes the scene's look and shows it on the placements the scene set up and on its projectiles still active
    /// (#2356).
    /// </summary>
    private void ShowScene()
    {
        PictureShading shading = PictureShading.Shaded(TourianStatueShades.Entrance);
        if (_eyeGlow is { } glow)
            shading = shading.With(glow);
        foreach (PictureShade look in _statueLooks)
            shading = shading.With(look);
        SceneShading = shading;
        foreach (RoomEnemyPlace placement in _placements)
        {
            // A place an enemy of another kind has taken since shows that enemy's own look.
            if (placement.DefinitionId == EnemyDefinitionId.TourianStatue)
                placement.Shading = shading;
        }
        foreach (EnemyProjectile projectile in _sceneProjectiles)
        {
            if (projectile.IsActive)
                projectile.Graphics = projectile.Graphics.In(shading);
        }
    }

    /// <summary>
    /// Holds <paramref name="projectile"/>, a decoration or an unlock effect just spawned in the scene's look, as one of the
    /// scene's projectiles: the scene shows each later look on it while it lasts.
    /// </summary>
    internal void HoldSceneProjectile(EnemyProjectile projectile)
    {
        Ensure.NotNull(projectile);
        // A projectile that has ended shows no later look; one spawned since in its place is a projectile of its own.
        _sceneProjectiles.RemoveAll(ended => !ended.IsActive);
        _sceneProjectiles.Add(projectile);
    }

    /// <summary>
    /// The statues' shared animation state, set and cleared by the statue animations, the descent and the decorations,
    /// whether the statue's sinking has finished among it.
    /// </summary>
    public TourianStatueAnimation TourianEntranceStatueAnimation { get; } = new();


    /// <summary>
    /// Initializes one of the statue scene's three placements (#2014): the first spawns the scene's decorations, and each
    /// shows the scene's entrance look.
    /// </summary>
    internal void InitializeTourianEntranceStatue(RoomEnemyPlace statue)
    {
        Ensure.NotNull(statue);
        if (!_placements.Contains(statue))
            _placements.Add(statue);
        // The base decoration, Ridley and Phantoon spawn in this order, which gives them their authored draw order.
        if (statue.Spawn.Population.SpawnSettingsOf<TourianEntranceStatueSpawnSettings>().Role == TourianEntranceStatueRole.SpawnsDecorations)
        {
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueBaseDecoration,
                TourianStatueProjectileSegment.BaseDecoration,
                TourianStatueDecorationDefinitions.BaseDecoration);
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueRidley,
                TourianStatueProjectileSegment.Ridley,
                TourianStatueDecorationDefinitions.RidleyStatue);
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatuePhantoon,
                TourianStatueProjectileSegment.Phantoon,
                TourianStatueDecorationDefinitions.PhantoonStatue);
        }

        // Statue initialization selects the common sprite colours, which the import colours the statue's sheets in, and
        // installs the statue's own colours and its decoration colours: the scene's entrance shade (#1512).
        ShowScene();
    }

    /// <summary>
    /// Allocates a decorative projectile of <paramref name="kind"/>, starting its animation at <paramref name="start"/>,
    /// when a place is free tied to its authored position.
    /// </summary>
    internal void SpawnTourianEntranceStatueProjectile(
        RoomEnemyProjectileKind kind,
        TourianStatueProjectileSegment start,
        Position16<RoomPixels> position)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Decorations, graphics: SceneGraphics);
        HoldSceneProjectile(projectile);
        projectile.StartAnimation(new TourianStatueProjectileAnimation(start));
        projectile.Position = projectile.Position.WithWhole(position);
        // The decoration stays tied to its authored spawn point as the statue descends.
        projectile.State = new AnchoredProjectileState(position);
    }

    /// <summary>The statue's base decoration, Ridley and Phantoon (#1760): their spawner starts their animations.</summary>
    internal EnemyProjectileBehavior Decorations => field ??= new DecorationBehavior(TourianEntranceStatueAnimation, _room);

    /// <summary>A decoration: it follows the statue's reveal and descent, and settles at its place.</summary>
    /// <param name="statue">The statues' shared animation state, whose rest finishes the statue's sinking.</param>
    /// <param name="room">The room's services, whose statue room says how far the statue has sunk.</param>
    private sealed class DecorationBehavior(TourianStatueAnimation statue, EnemyRoomBinding room)
        : TourianStatueProjectileBehavior<TourianStatueDecorationMotion>(TourianStatueDecorationMotion.Settled)
    {
        /// <summary>The base decoration's animation starts it following the statue's reveal.</summary>
        /// <param name="projectile">The base decoration.</param>
        private protected override void FollowReveal(EnemyProjectile projectile) =>
            projectile.StartMotion(TourianStatueDecorationMotion.FollowingReveal);

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, TourianStatueDecorationMotion motion)
        {
            switch (motion)
            {
                case TourianStatueDecorationMotion.FollowingReveal:
                case TourianStatueDecorationMotion.Settled:
                    PositionTourianEntranceStatueProjectile(context.Projectile);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TourianStatueDecorationMotion)} {motion}.");
            }
        }

        /// <summary>Follows the statue down as it sinks and, for the base decoration, marks the sinking finished once the statues rest.</summary>
        private void PositionTourianEntranceStatueProjectile(EnemyProjectile projectile)
        {
            if (projectile.Motion is TourianStatueDecorationMotion.FollowingReveal &&
                statue.AtRest)
            {
                statue.Finished = true;
            }

            Position16<RoomPixels> anchor = projectile.StateOf<AnchoredProjectileState>().Anchor;
            projectile.Position = projectile.Position.WithWholeX(anchor.X);

            // The statue base adds the camera Y position and subtracts the reveal effect's camera Y plus displacement.
            // The camera terms cancel here; ordinary sprite drawing subtracts the camera once.
            // This keeps the statue's sprite pictures attached to the descending background statue artwork.
            projectile.Position = projectile.Position.WithWholeY(anchor.Y - room.Services.TourianStatueRoom.StatueOffset);
        }
    }
}

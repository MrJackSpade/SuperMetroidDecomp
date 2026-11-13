using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian entrance statue species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class TourianEntranceStatueBehavior : IEnemySpecies, IEnemyEntityFactory, IGameplayShadingSource
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.TourianStatue];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new TourianEntranceStatueEntity(this, slot, common);

    /// <summary>Allocates the three fixed statue decoration actors in reverse-slot order.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy slots, whose statue records show the scene's look.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>The eye glow the statues' eyes show, the last a released boss lit; none before any.</summary>
    private PictureShade? _eyeGlow;

    /// <summary>
    /// The released sprite statues' blacked-out eyes, grey-out records and greys, in the order they were shown; a
    /// grey-out's record replaces the one before it.
    /// </summary>
    private readonly List<PictureShade> _statueLooks = [];

    /// <summary>The released statues drawn in the room's tiles: their blacked-out eyes and greys, in the order shown.</summary>
    private readonly List<PictureShade> _roomLooks = [];

    /// <summary>Binds projectile allocation and the room's slots to the statue species.</summary>
    internal TourianEntranceStatueBehavior(EnemyProjectileEngine projectiles, EnemySlotTable slotTable)
    {
        _projectiles = projectiles;
        _slotTable = slotTable;
    }

    /// <summary>
    /// The statue scene's look (#1512): its standard colours until the statue initializes, then its entrance colours,
    /// the last eye glow a released boss lit, and each released statue's blacked-out eyes and grey. Every statue scene
    /// sprite shows it; the game wrote these colours into palettes the scene's sprites share.
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
    /// room's tiles, otherwise over the scene's sprites.
    /// </summary>
    internal void ShowStatueLook(PictureShade look)
    {
        if (TourianStatueShades.RoomTileLooks.Contains(look))
        {
            _roomLooks.Add(look);
            return;
        }
        _statueLooks.Add(look);
        ShowScene();
    }

    /// <summary>
    /// Shows record <paramref name="record"/> of a sprite statue's grey-out, <paramref name="colours"/> naming the Ridley or
    /// Phantoon statue, over the scene's sprites (#1512): each record replaces the one its grey-out showed before.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="colours"/> names no sprite statue.</exception>
    internal void ShowStatueGreyOut(PaletteFxColours colours, PaletteFxFrame record)
    {
        if (colours is not (PaletteFxColours.RidleyStatue or PaletteFxColours.PhantoonStatue))
            throw new ArgumentOutOfRangeException(nameof(colours), colours, "Only the Ridley and Phantoon statues grey out as sprites.");
        PictureShade shade = PaletteFxShades.Of(colours, record);
        _statueLooks.RemoveAll(earlier => earlier.Effect == shade.Effect);
        _statueLooks.Add(shade);
        ShowScene();
    }

    /// <summary>Lays the looks of the released statues drawn in the room's tiles over the room's pictures (#1512).</summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (PictureShade look in _roomLooks)
            frame.Shade(GameplayPictures.Room, look);
    }

    /// <summary>Recomposes the scene's look and shows it on every statue record and statue scene projectile.</summary>
    private void ShowScene()
    {
        PictureShading shading = PictureShading.Shaded(TourianStatueShades.Entrance);
        if (_eyeGlow is { } glow)
            shading = shading.With(glow);
        foreach (PictureShade look in _statueLooks)
            shading = shading.With(look);
        SceneShading = shading;
        foreach (RoomEnemySlot statue in _slotTable.Slots)
        {
            if (statue.DefinitionId == EnemyDefinitionId.TourianStatue)
                statue.Shading = shading;
        }
        foreach (EnemyProjectile projectile in _projectiles.EnemyProjectiles)
        {
            if (TourianStatueProjectileStarts.ShowsStatueScene(projectile.Kind))
                projectile.Graphics = projectile.Graphics.In(shading);
        }
    }

    /// <summary>
    /// Whether the entrance statue has finished sinking. The statue's base
    /// decoration sets it once the sinking animation has ended.
    /// </summary>
    public bool TourianEntranceStatueFinished { get; internal set; }
    /// <summary>
    /// Signed background displacement owned by the statue's sinking; sprites follow the same sinking statue. The statue sequence only writes
    /// its descent offset, between minus the descent distance and zero.
    /// </summary>
    public int TourianEntranceStatueVerticalOffset { get; internal set; }

    /// <summary>The statues' shared animation state, set and cleared by the statue tile programs and the descent.</summary>
    public TourianStatueAnimation TourianEntranceStatueAnimation { get; } = new();


    /// <summary>Installs one of three animations and lets only the first statue record spawn the fixed decorative actors.</summary>
    internal void InitializeTourianEntranceStatue(RoomEnemySlot statue)
    {
        TourianEntranceStatueProgram part = statue.Spawn.Population.SpawnSettingsOf<TourianEntranceStatueSpawnSettings>().Statue;
        statue.AnimationClock.Restart();
        statue.SetSpeciesState(new TourianEntranceStatueState(new AnimationPlayback<TourianEntranceStatueProgram>(TourianEntranceStatueAnimations.StartingWith(part), AnimationPlace.StartOfAnimation)));

        // Only the first of the three enemy records creates the fixed screen actors. The
        // allocator searches from the last slot down, so this exact call order gives the
        // decoration, Ridley, and Phantoon their authored draw ordering.
        if (part == TourianEntranceStatueProgram.BaseDecoration)
        {
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueBaseDecoration,
                position: new(120, 184));
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueRidley,
                position: new(142, 85));
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatuePhantoon,
                position: new(132, 136));
        }

        // Statue initialization selects the common sprite colours, which the import colours the statue's sheets in, and
        // installs the statue's own colours and its decoration colours: the scene's entrance shade (#1512).
        ShowScene();
    }

    /// <summary>Allocates a decorative projectile when a slot is free tied to its authored position.</summary>
    internal void SpawnTourianEntranceStatueProjectile(
        RoomEnemyProjectileKind kind,
        Position16<RoomPixels> position)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        TourianStatueProjectileStarts.Initialize(projectile, kind, graphics: SceneGraphics);
        projectile.Position = projectile.Position.WithWhole(position);
        // The decoration stays tied to its authored spawn point as the statue descends.
        projectile.State = new AnchoredProjectileState(position);
    }

    /// <summary>Tracks the background statue's vertical displacement and publishes the base decoration's completion latch.</summary>
    internal void PositionTourianEntranceStatueProjectile(EnemyProjectile projectile)
    {
        if (projectile.FrameBehavior == EnemyProjectileFrameBehavior.TourianStatueRevealFollow &&
            TourianEntranceStatueAnimation.AtRest)
        {
            TourianEntranceStatueFinished = true;
        }

        Position16<RoomPixels> anchor = projectile.StateOf<AnchoredProjectileState>().Anchor;
        projectile.Position = projectile.Position.WithWholeX(anchor.X);

        // The statue base adds the camera Y position and subtracts the reveal effect's camera Y plus displacement.
        // The camera terms cancel here; ordinary sprite drawing subtracts the camera once.
        // This keeps all sprite pieces attached to the descending background statue artwork.
        projectile.Position = projectile.Position.WithWrappedWholeY(anchor.Y - TourianEntranceStatueVerticalOffset);
    }

    /// <summary>Registers the shared per-frame behaviour for base, Ridley, and Phantoon decoration actors.</summary>
    internal void AddTourianEntranceStatueProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            PositionTourianEntranceStatueProjectile(context.Projectile);
        }, EnemyProjectileFrameBehavior.TourianStatueRevealFollow, EnemyProjectileFrameBehavior.TourianStatueDecorationSettled);
    }

    /// <summary>Advances TourianEntranceStatue's typed animation.</summary>
    internal static void AdvanceTourianEntranceStatueTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        TourianEntranceStatueState state = slot.RequireSpeciesState<TourianEntranceStatueState>();
        AnimationPlayback<TourianEntranceStatueProgram> playback = state.Playback;
        EnemyProgramAnimation.Play(slot, TourianEntranceStatueAnimations.Segments, ref playback, TourianEntranceStatueFrameDefinitions.ShownFrames.Of,
            static cue => throw new InvalidOperationException($"A Tourian entrance statue animation raised {cue}, though its lists raise no events."));
        state.Playback = playback;
    }
}

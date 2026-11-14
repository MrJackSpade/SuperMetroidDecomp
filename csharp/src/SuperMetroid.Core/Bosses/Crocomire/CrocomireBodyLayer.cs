using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body layer: where it is placed, what his body shows on it, the tongue placed with it and the acid smoke drawn with it.</summary>
[RoomLifetime]
internal sealed class CrocomireBodyLayer
{
    /// <summary>Allocates arena debris and cinematic sprite objects.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBodyLayer(RoomSpriteObjects spriteObjects, EnemyPresentation presentation, CrocomireEncounterAccess access)
    {
        _spriteObjects = spriteObjects;
        _presentation = presentation;
        _access = access;
    }

    /// <summary>
    /// Where Crocomire's body lies in the room (#1617, #1802): the room pixel his body's parts' area's top-left corner lies on,
    /// which follows him. His sinking, hiding and melt reckon his body's rows from it.
    /// </summary>
    public Position16<RoomPixels> BodyOrigin { get; private set; }

    /// <summary>Starts the tongue's <paramref name="segment"/> from its first step after <paramref name="updates"/> updates.</summary>
    internal static void StartCrocomireTongueAnimation(CrocomireEnemyState state, CrocomireTongueSegment segment, int updates)
    {
        RoomEnemySlot tongue = state.Tongue ??
            throw new InvalidOperationException("Crocomire's tongue program started without a tongue.");
        state.TonguePlayback = new AnimationPlayback<CrocomireTongueSegment>(CrocomireAnimations.TongueStartingWith(segment), AnimationPlace.StartOfAnimation);
        tongue.AnimationClock.RestartAfter(updates);
    }

    /// <summary>
    /// His body shows <paramref name="frame"/> from now on in place of whatever it showed (#1355, #1515, #1540), as a melt
    /// image replaces it: his body is the one part he draws on it.
    /// </summary>
    internal void ShowCrocomireBodyFrame(BossBodyFrame frame) =>
        _presentation.RequireBossBody().Show(EnemyDefinitionId.Crocomire, frame);

    /// <summary>His body shows nothing from now on, until a frame is shown on it again (#1515).</summary>
    internal void ClearCrocomireBody() => _presentation.RequireBossBody().Clear();

    /// <summary>
    /// Hides his body from body row <paramref name="top"/> down, until a frame is shown on it again (#1515): his body's
    /// picture shows only above that row (#1540).
    /// </summary>
    internal void HideCrocomireBodyFrom(int top) => _presentation.RequireBossBody().ShowAbove(top);

    /// <summary>Levels the melt's stretch: every line shows his body as it lies now (#1468, #1802).</summary>
    internal void LevelCrocomireMeltDistortion() =>
        _access.RequireCrocomireDeath().MeltDistortion = CrocomireMeltDistortion.Level(BodyOrigin.Y);

    /// <summary>The row of his body's area that lies on room row <paramref name="roomRow"/>, wrapping as the area's rows do.</summary>
    internal int BodyRowAt(int roomRow) => CoordinateAxis.Wrap(roomRow - BodyOrigin.Y);

    /// <summary>
    /// Places his body's area vertically at the body, then falls through into the horizontal placement. His room is one
    /// screen tall and every screen of it keeps the camera at the room's top row, so his body's area lies as far above him in
    /// the room as it lies above him on the screen. The frames some poses show lower on the area carry those rows
    /// themselves, as the import placed them (#1802).
    /// </summary>
    internal void PlaceCrocomireBodyLayer(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        BodyOrigin = BodyOrigin.WithY(CoordinateAxis.Wrap(body.Position.Y.Whole - CrocomireBodyAreaDefinitions.AboveBody));
        PlaceCrocomireTongueAndBodyLayerHorizontally(state);
    }

    /// <summary>Places the tongue and his body layer horizontally at the body.</summary>
    internal void PlaceCrocomireTongueAndBodyLayerHorizontally(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;

        // The tongue is a normal second enemy actor placed at its body-relative X offset.
        // The horizontal placement updates it even while the body's bulk is shown on his body layer.
        if (state.Tongue is { } tongue)
        {
            int offset = (tongue.SpeciesState<CrocomireTongueState>()
                ?? throw new InvalidOperationException("Crocomire's tongue has no state.")).BodyXOffset;
            tongue.Position = tongue.Position.WithWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + offset), body.Position.Y.Whole));
        }

        // His body's area lies 51 pixels left of him (#1802); it shows only while he is near the screen, within 128 pixels of
        // either edge and 284 of the screen's left edge plus 51; farther away it shows nothing (#1617).
        bool onScreen = CoordinateAxis.Difference(body.Position.X.Whole, state.Camera.X) >= 0
            ? CoordinateAxis.Difference(body.Position.X.Whole - 128, (state.Camera.X + 256)) < 0
            : CoordinateAxis.AsSigned(body.Position.X.Whole + 128 - state.Camera.X) >= 0;
        BodyOrigin = BodyOrigin.WithX(CoordinateAxis.Wrap(body.Position.X.Whole - CrocomireBodyAreaDefinitions.LeftOfBody));
        ShowsBody = onScreen && CoordinateAxis.Distance(state.Camera.X + CrocomireBodyAreaDefinitions.LeftOfBody, body.Position.X.Whole) < 284;
    }

    /// <summary>Whether his body shows: only while he is near the screen (#1617); farther away it shows nothing.</summary>
    public bool ShowsBody { get; private set; } = true;

    /// <summary>
    /// Pins his body's area's top-left corner to the screen's top-left, where the defeated room shows it: the room pixel the
    /// camera <paramref name="state"/>'s main update saw shows there (#1802).
    /// </summary>
    internal void PinToScreen(CrocomireEnemyState state)
    {
        BodyOrigin = state.Camera;
        ShowsBody = true;
    }

    /// <summary>Emits smoke at the acid surface during Crocomire's death presentation.</summary>
    internal void SpawnCrocomireAcidSmoke(CrocomireEnemyState state, SamusState? samus)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (--death.AcidSmokeTimer != 0)
            return;
        death.AcidSmokeTimer = 6;

        int random = _access.ReadCrocomireRandom();
        // Smoke on the left rises one pixel further from the body than its mirror on the right; the smoke's X keeps
        // sixteen bits where the offset is added.
        int xOffset = CrocomireRandomDefinitions.AcidSmokeXOffset.Of(random);
        if (!CrocomireRandomDefinitions.AcidSmokeOnRight.IsSetIn(random))
            xOffset = -(xOffset + CrocomireRandomDefinitions.AcidSmokeLeftExtraPixels);
        // Crocomire's room always has its acid surface, which its tide only moves.
        int acidY = samus is null
            ? 224
            : samus.LiquidPhysics.LavaAcidSurfaceY ?? throw new InvalidOperationException("Crocomire's acid smoke rises from his room's acid surface.");
        _spriteObjects.SpawnRoomSpriteObject(
            new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + xOffset), CoordinateAxis.Wrap(acidY + 16 - CrocomireRandomDefinitions.AcidSmokeRise.Of(random))),
            RoomSpriteObjectKind.CrocomireAcidSmoke,
            graphics: default);
    }
}

using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body layer: where it is placed, what his body shows on it, the tongue placed with it and the acid smoke drawn with it.</summary>
[RoomLifetime]
internal sealed class CrocomireBodyLayer
{
    /// <summary>Allocates arena debris and cinematic sprite objects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBodyLayer(RoomSpriteObjectPool spriteObjects, EnemyPresentation presentation, CrocomireEncounterAccess access)
    {
        _spriteObjects = spriteObjects;
        _presentation = presentation;
        _access = access;
    }

    /// <summary>Where Crocomire's body layer is placed: the layer position shown at the screen's top-left.</summary>
    public Position16<BackgroundPixels> Placement { get; internal set; }

    /// <summary>Starts the tongue's <paramref name="program"/> from its first step once <paramref name="timer"/> expires.</summary>
    internal static void StartCrocomireTongueProgram(CrocomireEnemyState state, CrocomireTongueProgram program, WrappingCountdown timer)
    {
        RoomEnemySlot tongue = state.Tongue ??
            throw new InvalidOperationException("Crocomire's tongue program started without a tongue.");
        state.TongueProgram = program;
        state.TonguePosition = 0;
        tongue.FrameHold = timer;
        tongue.Timer = WrappingCountdown.Start(0);
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

    /// <summary>Levels the melt's stretch: every line of his body layer shows <paramref name="verticalScroll"/> (#1468).</summary>
    /// <param name="verticalScroll">The vertical scroll every line shows, 0 through 65535.</param>
    internal void LevelCrocomireMeltDistortion(int verticalScroll) =>
        _access.RequireCrocomireDeath().MeltDistortion = CrocomireMeltDistortion.Level(verticalScroll);

    /// <summary>Places his body layer vertically at the body, then falls through into the horizontal placement.</summary>
    internal void PlaceCrocomireBodyLayer(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        Placement = Placement.WithY(CrocomireBodyLayerAlignment.VerticalPlacement(
            body.Position.Y.Whole, body.Frame));
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

        bool onScreen = CoordinateAxis.Difference(body.Position.X.Whole, state.Camera.X) >= 0
            ? CoordinateAxis.Difference(body.Position.X.Whole - 128, (state.Camera.X + 256)) < 0
            : CoordinateAxis.AsSigned(body.Position.X.Whole + 128 - state.Camera.X) >= 0;
        Placement = Placement.WithX(onScreen
            ? OnScreenCrocomireBodyLayerX(body.Position.X.Whole, state.Camera)
            : 256);
    }

    /// <summary>Converts the body X position into his body layer's horizontal placement, which keeps Crocomire onscreen.</summary>
    /// <param name="bodyX">The body's whole X coordinate, 0 through 65535.</param>
    /// <param name="camera">The camera position.</param>
    private static int OnScreenCrocomireBodyLayerX(int bodyX, Position16<RoomPixels> camera)
    {
        int relative = CoordinateAxis.Wrap(camera.X - bodyX + 51);
        return CoordinateAxis.Distance(camera.X + 51, bodyX) >= 284 ? 256 : relative;
    }

    /// <summary>Emits smoke at the acid surface during Crocomire's death presentation.</summary>
    internal void SpawnCrocomireAcidSmoke(CrocomireEnemyState state, SamusState? samus)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        if (--death.AcidSmokeTimer != 0)
            return;
        death.AcidSmokeTimer = 6;

        int random = _access.ReadCrocomireRandom();
        // The offset is complemented, not negated, when bit one is clear; the smoke's X keeps
        // sixteen bits where the offset is added.
        int xOffset = CrocomireRandomDefinitions.AcidSmokeXOffset.Of(random);
        if (!CrocomireRandomDefinitions.AcidSmokeOnRight.IsSetIn(random))
            xOffset = ~xOffset;
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

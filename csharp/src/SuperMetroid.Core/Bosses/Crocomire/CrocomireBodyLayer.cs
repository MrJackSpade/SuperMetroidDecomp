using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
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
        tongue.InstructionTimer = timer;
        tongue.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>The canvas Crocomire's death assembles his body on.</summary>
    /// <exception cref="InvalidOperationException">Crocomire has no death state, or it has no working canvas.</exception>
    internal BossBodyCanvas WorkingBody => _access.RequireCrocomireDeath().WorkingBody
        ?? throw new InvalidOperationException("Crocomire's death has no working body canvas.");

    /// <summary>Clears the whole working canvas before a melt image is assembled on it.</summary>
    internal void ClearCrocomireWorkingBody() => WorkingBody.Clear(0, WorkingBody.Rows * BossBodyDefinitions.ColumnCount);

    /// <summary>Shows the working canvas's <paramref name="cellCount"/> cells from <paramref name="startCell"/> on his body layer (#1355).</summary>
    /// <exception cref="InvalidDataException">The cells leave the canvas.</exception>
    internal void ShowCrocomireWorkingCells(int startCell, int cellCount) =>
        _presentation.RequireBossBody().Copy(WorkingBody, startCell, cellCount);

    /// <summary>Levels the melt's stretch: every line of his body layer shows <paramref name="verticalScroll"/> (#1468).</summary>
    /// <param name="verticalScroll">The vertical scroll every line shows, 0 through 65535.</param>
    internal void LevelCrocomireMeltDistortion(int verticalScroll) =>
        _access.RequireCrocomireDeath().MeltDistortion = CrocomireMeltDistortion.Level(verticalScroll);

    /// <summary>Ports $A4:8B5B: places his body layer vertically at the body, then falls through into $A4:8BA4.</summary>
    internal void PlaceCrocomireBodyLayer(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        Placement = Placement.WithY(CrocomireBodyLayerAlignment.VerticalPlacement(
            body.Position.Y.Whole, body.Frame));
        PlaceCrocomireTongueAndBodyLayerHorizontally(state);
    }

    /// <summary>Ports $A4:8BA4: places the tongue and his body layer horizontally at the body.</summary>
    internal void PlaceCrocomireTongueAndBodyLayerHorizontally(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;

        // The tongue is a normal second enemy actor placed at its body-relative X offset.
        // $8BA4 updates it even while the body's bulk is shown on his body layer.
        if (state.Tongue is { } tongue)
        {
            int offset = (tongue.SpeciesState<CrocomireTongueState>()
                ?? throw new InvalidOperationException("Crocomire's tongue has no state.")).BodyXOffset;
            tongue.Position = tongue.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + offset)), body.Position.Y.Whole));
        }

        bool onScreen = unchecked((short)(body.Position.X.Whole - state.Camera.X)) >= 0
            ? unchecked((short)(body.Position.X.Whole - 128 - (state.Camera.X + 256))) < 0
            : unchecked((short)(body.Position.X.Whole + 128 - state.Camera.X)) >= 0;
        Placement = Placement.WithX(onScreen
            ? OnScreenCrocomireBodyLayerX(body.Position.X.Whole, state.Camera)
            : (ushort)256);
    }

    /// <summary>Converts the body X position into his body layer's horizontal placement, which keeps Crocomire onscreen.</summary>
    /// <param name="bodyX">The body's whole X coordinate, 0 through 65535.</param>
    /// <param name="camera">The camera position.</param>
    private static ushort OnScreenCrocomireBodyLayerX(int bodyX, Position16<RoomPixels> camera)
    {
        int relative = unchecked((ushort)(camera.X - bodyX + 51));
        return EnemyMath.WrappedMagnitude((ushort)relative) >= 284 ? (ushort)256 : (ushort)relative;
    }

    /// <summary>Emits smoke at the acid surface during Crocomire's death presentation.</summary>
    internal void SpawnCrocomireAcidSmoke(CrocomireEnemyState state, SamusState? samus)
    {
        CrocomireDeathState death = _access.RequireCrocomireDeath();
        death.AcidSmokeTimer = death.AcidSmokeTimer.Decrement().Remaining;
        if (!death.AcidSmokeTimer.IsZero)
            return;
        death.AcidSmokeTimer = WrappingCountdown.Start(6);

        int random = _access.ReadCrocomireRandom();
        // The offset is complemented, not negated, when bit one is clear; the smoke's X keeps
        // sixteen bits where the offset is added.
        int xOffset = random & 63;
        if ((random & 2) == 0)
            xOffset = ~xOffset;
        int acidY = samus?.LiquidPhysics.LavaAcidYPosition ?? 224;
        _spriteObjects.SpawnRoomSpriteObject(
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(acidY + 16 - (SharedRandomState.UpperByte(random) & 31)))),
            RoomSpriteObjectKind.CrocomireAcidSmoke,
            graphics: default);
    }
}

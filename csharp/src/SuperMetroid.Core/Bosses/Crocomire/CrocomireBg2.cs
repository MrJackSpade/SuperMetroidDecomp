using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's BG2 body: its scroll, scroll table, the words it writes into his BG2 image and the acid smoke drawn with it.</summary>
[RoomLifetime]
internal sealed class CrocomireBg2
{
    /// <summary>Allocates arena debris and cinematic sprite objects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireBg2(RoomSpriteObjectPool spriteObjects, EnemyPresentation presentation, CrocomireEncounterAccess access)
    {
        _spriteObjects = spriteObjects;
        _presentation = presentation;
        _access = access;
    }

    /// <summary>Literal BG2 scroll owned by Crocomire while its extended tilemap is active.</summary>
    public Position16<Bg2Pixels> CrocomireBg2Scroll { get; internal set; }

    /// <summary>Switches an actor's instruction pointer and resets its timer for the next native list.</summary>
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

    /// <summary>BG2 tile word used to clear Crocomire's working background rows.</summary>
    internal static ushort CrocomireBlankBg2Tile => CrocomireMeltingTilemap.BlankGlyph.Raw;

    /// <summary>Clears the working BG2 map before death and melting scene transfers.</summary>
    internal void ClearCrocomireBg2WorkingTilemap()
    {
        Span<ushort> tilemap = _access.RequireCrocomireDeath().MutableBg2WorkingTilemap;
        tilemap.Fill(CrocomireBlankBg2Tile);
    }

    /// <summary>Copies a bounded working-map span into the corresponding words of the shown enemy BG2 image (#1355).</summary>
    internal void WriteCrocomireBg2Words(int startWord, int wordCount)
    {
        if (startWord < 0 || wordCount < 0 ||
            startWord + wordCount > CrocomireDeathState.Bg2WorkingWordCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startWord));
        }

        _presentation.RequireBg2Image().Write(startWord, _access.RequireCrocomireDeath().Bg2WorkingTilemap.Slice(startWord, wordCount));
    }

    /// <summary>Writes a uniform vertical offset to the per-scanline BG2 scroll table.</summary>
    /// <param name="verticalScroll">The vertical scroll every line shows, 0 through 65535.</param>
    internal void FillCrocomireBg2ScrollTable(int verticalScroll) =>
        _access.RequireCrocomireDeath().MutableBg2ScrollByScanline.Fill(checked((ushort)verticalScroll));

    /// <summary>Ports $A4:8B5B: places BG2 vertically at the body, then falls through into $A4:8BA4.</summary>
    internal void UpdateCrocomireBg2Scroll(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        CrocomireBg2Scroll = CrocomireBg2Scroll.WithY(CrocomireBg2ScrollDefinitions.VerticalScroll(
            body.Position.Y.Whole, body.Frame));
        UpdateCrocomireTongueAndBg2Horizontal(state);
    }

    /// <summary>Ports $A4:8BA4: places the tongue and BG2 horizontally at the body.</summary>
    internal void UpdateCrocomireTongueAndBg2Horizontal(CrocomireEnemyState state)
    {
        RoomEnemySlot body = state.Body;

        // The tongue is a normal second enemy actor placed at its body-relative X offset.
        // $8BA4 updates it even while the body's bulk is represented by BG2.
        if (state.Tongue is { } tongue)
        {
            int offset = (tongue.SpeciesState<CrocomireTongueState>()
                ?? throw new InvalidOperationException("Crocomire's tongue has no state.")).BodyXOffset;
            tongue.Position = tongue.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + offset)), body.Position.Y.Whole));
        }

        bool onScreen = unchecked((short)(body.Position.X.Whole - state.Camera.X)) >= 0
            ? unchecked((short)(body.Position.X.Whole - 128 - (state.Camera.X + 256))) < 0
            : unchecked((short)(body.Position.X.Whole + 128 - state.Camera.X)) >= 0;
        CrocomireBg2Scroll = CrocomireBg2Scroll.WithX(onScreen
            ? CalculateOnScreenCrocomireBg2Horizontal(body.Position.X.Whole, state.Camera)
            : (ushort)256);
    }

    /// <summary>Converts the body X position into the BG2 horizontal scroll used to keep Crocomire onscreen.</summary>
    /// <param name="bodyX">The body's whole X coordinate, 0 through 65535.</param>
    /// <param name="camera">The camera position.</param>
    private static ushort CalculateOnScreenCrocomireBg2Horizontal(int bodyX, Position16<RoomPixels> camera)
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
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(acidY + 16 - ((SnesLittleEndian.HighByte(random) & 31))))),
            RoomSpriteObjectKind.CrocomireAcidSmoke,
            graphicsIndex: 0);
    }
}

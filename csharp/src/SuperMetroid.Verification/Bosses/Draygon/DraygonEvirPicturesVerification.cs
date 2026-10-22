using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Checks Draygon's opening Evirs draw from pictures of her room's pre-coloured sheet (#1392): the set's own characters
/// before her opening uploads the Evirs' tiles over them, the Evirs' after.
/// </summary>
internal static class DraygonEvirPicturesVerification
{
    /// <summary>The screen point the checked Evir stands at.</summary>
    private static readonly Position16<RoomPixels> EvirPosition = new(0x0080, 0x0080);

    /// <summary>
    /// In Draygon's room an opening Evir shows the picture for whatever is uploaded over its characters, and each picture
    /// paints exactly what the Evir's frame painted over the room's live OBJ characters.
    /// </summary>
    internal static void VerifyDraygonEvirPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The fixture starts in Ceres, whose room leaves its door tiles over standard characters; a session reaching
        // Draygon has started on Zebes, whose game start uploads the standard characters again.
        runtime.HudSetup.QueueStandardObjectTiles();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        // The room load's uploads reach the OBJ characters when its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomSpriteObjectSlot evir = runtime.Enemies.SpriteObjects.SpawnRoomSpriteObject(EvirPosition,
            RoomSpriteObjectKind.DraygonIntroEvir, EnemyPaletteBits.Palette7) ?? throw new InvalidOperationException(
            "Draygon's room has a free sprite-object slot for an opening Evir.");

        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        int tileBase = evir.Attributes.TileNumber;
        string frame = EnemySheetDefinitions.SpriteObjectFrameName(RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame));
        // The runtime draws from the sheets it loaded with its installation.
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        IReadOnlyList<CharacterRegion> regions = sheets.SpawnedRegions(set, tileBase, frame);
        AssertTrue(regions.Count > 0, $"the Evir's frame {frame} is drawn over characters Draygon's opening uploads");
        SpriteSheet sheet = sheets.Spawned(set, tileBase).Sheet;
        string setPicture = sheets.Uploads.PictureName(frame, [.. regions.Select(region => new CharacterRegionContent(region, null))]);
        string evirPicture = sheets.Uploads.PictureName(frame,
            [.. regions.Select(region => new CharacterRegionContent(region, DraygonIntroPresentationDefinitions.EvirUpload))]);

        AssertTrue(ReferenceEquals(sheet.Frame(setPicture), AssertDrawnLikeParts(runtime, evir, "before the opening upload").Image),
            "an Evir drawn before the opening upload shows the picture of the set's own characters");
        runtime.EnemySession.ObjCharacters.Record(DraygonIntroPresentationDefinitions.EvirUpload);
        AssertTrue(ReferenceEquals(sheet.Frame(evirPicture), AssertDrawnLikeParts(runtime, evir, "after the opening upload").Image),
            "an Evir drawn after the opening upload shows the picture of the Evirs' tiles");
        Console.WriteLine($"Draygon Evir pictures: {setPicture} before her opening upload and {evirPicture} after, each painting its frame over the live characters.");
    }

    /// <summary>
    /// Draws <paramref name="evir"/> from the room's sheet and asserts the picture paints exactly what its frame's parts
    /// paint over the room's live OBJ characters, the path it drew through before sheets; returns the draw.
    /// </summary>
    private static ImageSpriteDraw AssertDrawnLikeParts(SuperMetroidRuntime runtime, RoomSpriteObjectSlot evir, string when)
    {
        SpriteArtwork live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.ObjCharacters);
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        runtime.Enemies.SpriteObjects.DrawRoomSpriteObjects(sprites, live, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        ImageSpriteDraw drawn = AssertIs<ImageSpriteDraw>(sprites.Draws.Single(draw => draw.Source == new SpriteSource(SpriteSourceKind.RoomSpriteObject, evir.SlotIndex)),
            $"the Evir draws from a sheet {when}");

        EnemyFrameId shown = RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame);
        AssertTrue((InstalledFixtures.EnemyTiles.Spritemaps ?? throw new InvalidOperationException("Verification requires the installed compositions."))
            .TryGetDisplay(shown, out EnemySpritemapParts parts), $"the Evir's frame {shown} has an installed composition");
        ushort paletteBits = evir.Attributes.PaletteBits;
        var parted = new SpriteFrameBuffer();
        parted.BeginFrame();
        EnemyPresentation.PublishParts(parted, live, drawn.Source, EnemySpriteFrames.Frame(parts, checked((ushort)evir.Attributes.TileNumber), paletteBits),
            drawn.Origin, paletteBits);
        parted.FinalizeFrame();

        var cgram = new SnesCgram();
        for (int colour = 0; colour < 256; colour++)
            cgram.SetColor(colour, runtime.Cgram.Colors[colour]);
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var expected = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        var actual = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        SnesObjRenderer.RenderResolved(parted.Draws, cgram, expected);
        SnesObjRenderer.RenderResolved([drawn], cgram, actual);
        int painted = 0;
        for (int pixel = 0; pixel < count; pixel++)
        {
            if (expected.Pixels[pixel] != actual.Pixels[pixel] || expected.Priorities[pixel] != actual.Priorities[pixel] ||
                expected.Palettes[pixel] != actual.Palettes[pixel])
                throw new InvalidOperationException($"The Evir's picture {when} differs from its frame over the live characters at screen pixel " +
                    $"({pixel % SnesPpuLayout.ScreenSize.X},{pixel / SnesPpuLayout.ScreenSize.X}): colour {actual.Pixels[pixel]} for {expected.Pixels[pixel]}, " +
                    $"row {actual.Palettes[pixel]} for {expected.Palettes[pixel]}, priority {actual.Priorities[pixel]} for {expected.Priorities[pixel]}; uploads: " +
                    string.Join("; ", runtime.EnemySession.ObjCharacters.Uploads.Select(upload => $"{upload.Source}@{upload.FirstByte:X4}+{upload.ByteCount:X4}")));
            if (expected.Palettes[pixel] != 0)
                painted++;
        }
        AssertTrue(painted > 0, $"the Evir paints pixels {when}");
        return drawn;
    }

    /// <summary>Runs the Draygon Evir picture check.</summary>
    [VerificationCommand("--draygon-evir-pictures")]
    internal static int DraygonEvirPicturesCommand()
    {
        Suite(nameof(VerifyDraygonEvirPictures), VerifyDraygonEvirPictures);
        return 0;
    }
}

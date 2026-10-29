using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;

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
        // Draygon has started on Zebes, whose game start shows the standard characters again.
        runtime.HudSetup.ShowStandardObjectCharacters();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        // The room load's uploads reach the OBJ characters when its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomSpriteObjectSlot evir = runtime.Enemies.SpriteObjects.SpawnRoomSpriteObject(EvirPosition,
            RoomSpriteObjectKind.DraygonIntroEvir, SpriteGraphics.InPalette(7)) ?? throw new InvalidOperationException(
            "Draygon's room has a free sprite-object slot for an opening Evir.");

        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        int tileBase = evir.Graphics.TileBase;
        string frame = EnemySheetDefinitions.SpriteObjectFrameName(RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame));
        // The runtime draws from the sheets it loaded with its installation.
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        IReadOnlyList<CharacterRegion[]> layers = sheets.SpawnedLayers(set, tileBase, frame);
        AssertTrue(layers.Count > 0, $"the Evir's frame {frame} is drawn over characters Draygon's opening uploads");
        SpriteSheet sheet = sheets.Spawned(set, tileBase).Sheet;
        IReadOnlyList<string> setPictures = UploadedPictureChecks.ExpectedPictures(sheets, set, frame, layers, []);
        IReadOnlyList<string> evirPictures = UploadedPictureChecks.ExpectedPictures(sheets, set, frame, layers, [PlacedUploads.Of(DraygonIntroPresentationDefinitions.EvirTiles)]);

        UploadedPictureChecks.AssertDrawnPictures(sheet, setPictures, AssertDrawnLikeParts(runtime, evir, "before the opening upload"),
            "an Evir drawn before the opening upload shows the pictures of the set's own characters");
        runtime.EnemySession.SpriteCharacters.Record(PlacedUploads.Of(DraygonIntroPresentationDefinitions.EvirTiles));
        UploadedPictureChecks.AssertDrawnPictures(sheet, evirPictures, AssertDrawnLikeParts(runtime, evir, "after the opening upload"),
            "an Evir drawn after the opening upload shows the pictures of the Evirs' tiles");
        Console.WriteLine($"Draygon Evir pictures: {string.Join(", ", setPictures)} before her opening upload and {string.Join(", ", evirPictures)} after, each painting its frame over the live characters.");
    }

    /// <summary>
    /// Draws <paramref name="evir"/> from the room's sheet and asserts the picture paints exactly what its frame's parts
    /// paint over the room's live OBJ characters, the path it drew through before sheets; returns its draws.
    /// </summary>
    private static ImmutableArray<SpriteDraw> AssertDrawnLikeParts(SuperMetroidRuntime runtime, RoomSpriteObjectSlot evir, string when)
    {
        SpriteArtwork live = RuntimeArtworkContent.Of(runtime).GameplayObjectPage(runtime.EnemySession.SpriteCharacters);
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        runtime.Enemies.SpriteObjects.DrawRoomSpriteObjects(sprites, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        ImmutableArray<SpriteDraw> drawn = [.. sprites.Draws.Where(draw => draw.Source == new SpriteSource(SpriteSourceKind.RoomSpriteObject, evir.SlotIndex))];
        AssertTrue(drawn.Length > 0, $"the Evir draws {when}");

        EnemyFrameId shown = RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame);
        AssertTrue((InstalledFixtures.EnemyTiles.Spritemaps ?? throw new InvalidOperationException("Verification requires the installed compositions."))
            .TryGetDisplay(shown, out EnemySpritemapParts parts), $"the Evir's frame {shown} has an installed composition");
        int paletteRow = evir.Graphics.PaletteRow;
        CharacterSpriteDraw parted = EnemyPartComposition.Parts(live, drawn[0].Source,
            SuperMetroid.AssetExtraction.Enemies.EnemySpriteFrames.Frame(parts, evir.Graphics.TileBase, paletteRow),
            drawn[0].Origin, NativeSpriteGraphicsWords.PaletteBits(paletteRow));

        UploadedPictureChecks.AssertPaintsLikeParts(drawn, [parted], $"the Evir {when}");
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

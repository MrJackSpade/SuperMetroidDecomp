using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Checks Draygon's opening Evirs draw from pictures of her room's pre-coloured sheet (#1392, #1513): no picture before her
/// opening readies the Evirs, for none is drawn then, and its picture of that step after.
/// </summary>
internal static class DraygonEvirPicturesVerification
{
    /// <summary>The screen point the checked Evir stands at.</summary>
    private static readonly Position16<RoomPixels> EvirPosition = new(0x0080, 0x0080);

    /// <summary>
    /// In Draygon's room an opening Evir has no picture before Draygon's opening, so drawing one then fails loudly, and after
    /// it shows its picture of the opening, painting exactly what the Evir's frame paints over the characters the room's
    /// load and the opening leave.
    /// </summary>
    internal static void VerifyDraygonEvirPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        // The room load's uploads reach the OBJ characters when its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomSpriteObjectSlot evir = runtime.Enemies.SpriteObjects.SpawnRoomSpriteObject(EvirPosition,
            RoomSpriteObjectKind.DraygonIntroEvir, SpriteGraphics.SharedIn(SuperMetroid.Core.Assets.PictureShading.Shaded(SuperMetroid.Core.Bosses.Draygon.Artwork.DraygonShades.Intro).With(SuperMetroid.Core.Bosses.Draygon.Artwork.DraygonShades.Sprite))) ?? throw new InvalidOperationException(
            "Draygon's room has a free sprite-object slot for an opening Evir.");

        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        SpawnedObjectOwner owner = evir.Graphics.Owner;
        string frame = EnemySheetDefinitions.SpriteObjectFrameName(RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame));
        // The runtime draws from the sheets it loaded with its installation.
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        IReadOnlyList<FrameArtworkVariation> variations = sheets.SpawnedVariations(set, owner, frame);
        FrameArtworkVariation opening = variations.Single(variation => variation.Sequence == EncounterArtworkSequence.DraygonEvirs);
        AssertTrue(!opening.DrawnBeforeFirstStep && opening.DrawnAt(0), $"the Evir's frame {frame} is drawn only once Draygon's opening readies the Evirs");
        SpawnedObjectSheet sheet = sheets.Spawned(set, owner);
        AssertTrue(!sheet.frames.ContainsKey(frame), $"the spawned objects' sheets hold no picture of the Evir's frame {frame} over the set's own characters");
        // An opening Evir is never drawn before the opening; a draw then fails loudly, for its sheet holds no picture of it.
        AssertThrows<InvalidDataException>(() => DrawEvir(runtime, evir), "an Evir drawn before Draygon's opening");
        string evirPicture = UploadedPictureChecks.ExpectedPicture(frame, variations, [new EncounterArtworkStep(EncounterArtworkSequence.DraygonEvirs, 0)]);
        runtime.EnemySession.ArtworkProgress.Show(EncounterArtworkSequence.DraygonEvirs, 0);
        UploadedPictureChecks.AssertDrawnPicture(sheet.Frame, evirPicture,AssertDrawnLikeParts(runtime, set, evir, "after the opening"),
            "an Evir drawn after the opening shows its picture of the Evirs' tiles");
        Console.WriteLine($"Draygon Evir pictures: no picture before her opening, {evirPicture} after, painting its frame over the characters then.");
    }

    /// <summary>
    /// Draws <paramref name="evir"/> from the room's sheet and asserts the picture paints exactly what its frame's parts
    /// paint over the characters <paramref name="set"/> leaves at the step Draygon's opening has reached; returns its draws.
    /// </summary>
    private static ImmutableArray<SpriteDraw> AssertDrawnLikeParts(SuperMetroidRuntime runtime, RoomEnemyGraphicsSetDefinition set,
        RoomSpriteObjectSlot evir, string when)
    {
        SpriteArtwork live = EnemySheetFixtures.LivePage(set, UploadedPictureChecks.Reached(runtime));
        ImmutableArray<SpriteDraw> drawn = DrawEvir(runtime, evir);
        AssertTrue(drawn.Length > 0, $"the Evir draws {when}");

        EnemyFrameId shown = RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame);
        AssertTrue((InstalledFixtures.EnemyTiles.Spritemaps ?? throw new InvalidOperationException("Verification requires the installed compositions."))
            .TryGetDisplay(shown, out EnemySpritemapParts parts), $"the Evir's frame {shown} has an installed composition");
        int paletteRow = evir.Graphics.PaletteRowIn(runtime.Enemies);
        CharacterSpriteDraw parted = EnemyPartComposition.Parts(live, drawn[0].Source,
            SuperMetroid.AssetExtraction.Enemies.EnemySpriteFrames.Frame(parts, evir.Graphics.NativeTileBaseIn(runtime.Enemies), paletteRow),
            drawn[0].Origin, NativeSpriteGraphicsWords.PaletteBits(paletteRow));

        UploadedPictureChecks.AssertPaintsLikeParts(drawn, [parted], $"the Evir {when}");
        return drawn;
    }

    /// <summary>Draws the room's sprite objects and returns <paramref name="evir"/>'s draws.</summary>
    private static ImmutableArray<SpriteDraw> DrawEvir(SuperMetroidRuntime runtime, RoomSpriteObjectSlot evir)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        runtime.Enemies.SpriteObjects.DrawRoomSpriteObjects(sprites, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        return [.. sprites.Draws.Where(draw => draw.Source == new SpriteSource(SpriteSourceKind.RoomSpriteObject, evir.SlotIndex))];
    }

    /// <summary>Runs the Draygon Evir picture check.</summary>
    [VerificationCommand("--draygon-evir-pictures")]
    internal static int DraygonEvirPicturesCommand()
    {
        Suite(nameof(VerifyDraygonEvirPictures), VerifyDraygonEvirPictures);
        return 0;
    }
}

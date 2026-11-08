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
/// Checks Draygon's opening Evirs draw from their finished picture on her room's pre-coloured sheet (#1392, #1538): no look
/// changes the Evir's frame, and it paints its frame over the characters her room leaves once the Evirs are readied.
/// </summary>
internal static class DraygonEvirPicturesVerification
{
    /// <summary>The screen point the checked Evir stands at.</summary>
    private static readonly Position16<RoomPixels> EvirPosition = new(0x0080, 0x0080);

    /// <summary>
    /// In Draygon's room an opening Evir's frame has one finished picture, which no look gameplay shows changes, and drawn it
    /// shows that picture, painting exactly what the Evir's frame paints over the characters the room's load and the Evirs'
    /// tiles leave.
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
            RoomSpriteObjectKind.DraygonIntroEvir, SpriteGraphics.SharedIn(SuperMetroid.Core.Bosses.Draygon.DraygonColours.Evirs)) ?? throw new InvalidOperationException(
            "Draygon's room has a free sprite-object slot for an opening Evir.");

        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        SpawnedObjectOwner owner = evir.Graphics.Owner;
        string frame = EnemySheetDefinitions.SpriteObjectFrameName(RoomSpriteObjectVisualDefinitions.ShownFrames.Of(evir.Frame));
        // The runtime draws from the sheets it loaded with its installation.
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        AssertEqual(0, sheets.SpawnedChangingEncounters(set, owner, frame).Count, $"no look gameplay shows changes the Evir's frame {frame}");
        SpawnedObjectSheet sheet = sheets.Spawned(set, owner);
        AssertTrue(sheet.frames.ContainsKey(frame), $"the spawned objects' sheets hold the Evir's finished picture under its frame's name {frame}");
        UploadedPictureChecks.AssertDrawnPicture(sheet.Frame, frame, AssertDrawnLikeParts(runtime, set, evir, "as it spawns"),
            "an Evir shows its finished picture of the Evirs' tiles");
        Console.WriteLine($"Draygon Evir pictures: the Evir draws its one picture {frame}, painting its frame over the readied Evirs' characters.");
    }

    /// <summary>
    /// Draws <paramref name="evir"/> from the room's sheet and asserts the picture paints exactly what its frame's parts
    /// paint over the characters <paramref name="set"/> leaves once Draygon's opening has readied the Evirs; returns its draws.
    /// </summary>
    private static ImmutableArray<SpriteDraw> AssertDrawnLikeParts(SuperMetroidRuntime runtime, RoomEnemyGraphicsSetDefinition set,
        RoomSpriteObjectSlot evir, string when)
    {
        SpriteArtwork live = EnemySheetFixtures.LivePage(set, ComposedEncounter.DraygonEvirs,
            EncounterLookUploads.IndexOf(ComposedEncounter.DraygonEvirs, EncounterLookUploads.DraygonEvirsLook));
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

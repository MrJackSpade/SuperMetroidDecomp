using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Confirms Mother Brain draws from pre-coloured pictures (#1392): after each upload her encounter makes over the OBJ
/// characters, and after each step of her corpse's rot, every brain and neck frame draws the pictures of its layers for what
/// their regions show, painting as the frame over the characters those uploads and rotted rows leave.
/// </summary>
internal static class MotherBrainPicturesVerification
{
    /// <summary>Draws every brain and neck frame after each upload and each rot step.</summary>
    internal static void VerifyMotherBrainPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // A session reaching Tourian has started on Zebes, whose game start shows the standard characters again.
        runtime.HudSetup.ShowStandardObjectCharacters();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomEnemySlot head = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Mother Brain's room loads her.")).Head;
        EnemyFrameId[] frames = [.. MotherBrainVisualDefinitions.HeadFrames.Select(entry => entry.Value).Append(MotherBrainVisualDefinitions.NeckJointFrame).Distinct()];
        CorpseRotSimulation corpse = CorpseRotSimulation.Of(runtime.EnemySession.TileArtwork ?? throw new InvalidOperationException("The runtime loads the enemy tiles."),
            runtime.Enemies.BossArtworkBinding.Artwork ?? throw new InvalidOperationException("The runtime loads the boss art."), [EnemyDefinitionId.MotherBrainHead]).Single();
        var shown = new Dictionary<CorpseRotRow, int>();
        int drawn = 0;
        Draw("as her room loads them");
        foreach (MotherBrainSpritePage page in MotherBrainSpritePage.All)
        {
            runtime.Enemies.MotherBrainEncounter.ShowSpritePage(page);
            Draw($"after {page}");
        }
        int steps = corpse.Shown(corpse.Rows).Max(state => state.Max() ?? 0);
        for (int step = 1; step <= steps; step++)
        {
            runtime.Enemies.MotherBrainEncounter.ShowCorpseRotStep(step);
            foreach (CorpseRotRow row in corpse.Rows)
                shown[row] = step;
            Draw($"after rot step {step}");
        }
        Console.WriteLine($"Mother Brain pictures: {frames.Length} brain and neck frames draw their pictures after each of {MotherBrainCharacterSources.All.Count} pages and {steps} rot steps; {drawn} draws paint as the frames over the live characters.");

        void Draw(string context)
        {
            SpriteArtwork live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters);
            byte[] characters = live.Characters[..SpriteArtworkPlacements.TableByteCount].ToArray();
            foreach ((CorpseRotRow row, int step) in shown)
                corpse.RowBytes(row, step).CopyTo(characters.AsSpan(row.FirstByte));
            SpriteArtwork rotted = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters);
            foreach (EnemyFrameId frame in frames)
            {
                var sprites = new SpriteFrameBuffer();
                sprites.BeginFrame();
                var source = new SpriteSource(SpriteSourceKind.MotherBrainBrain);
                var camera = new Position16<RoomPixels>(unchecked((ushort)(head.Position.X.Whole - 128)), unchecked((ushort)(head.Position.Y.Whole - 112)));
                runtime.Enemies.MotherBrainBehavior.Head.DrawMotherBrainWorldSpritemap(sprites, source,
                    new MotherBrainWorldFrame(frame, head.Position.Whole, head.PaletteRow, head.TileIndexBase), camera);
                sprites.FinalizeFrame();
                ImmutableArray<SpriteDraw> pictures = [.. sprites.Draws];
                UploadedPictureChecks.AssertPaintsLikeParts(pictures,
                    [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"Mother Brain {frame} {context}: draws from a sheet"), rotted))],
                    $"Mother Brain {frame} {context}");
                drawn++;
            }
        }
    }

    /// <summary>Runs the Mother Brain picture check.</summary>
    [VerificationCommand("--mother-brain-pictures")]
    internal static int MotherBrainPicturesCommand()
    {
        Suite(nameof(VerifyMotherBrainPictures), VerifyMotherBrainPictures);
        return 0;
    }
}

using SuperMetroid.AssetExtraction.Artwork;
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

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Confirms Mother Brain draws from pre-coloured pictures (#1392, #1513): at each page her encounter shows, and at each step
/// of her corpse's rot, every brain and neck frame draws its picture of the steps reached, painting as the frame over the
/// characters her room's load, every page up to the one reached and the rot step reached leave.
/// </summary>
internal static class MotherBrainPicturesVerification
{
    /// <summary>Draws every brain and neck frame at each page and each rot step.</summary>
    internal static void VerifyMotherBrainPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomEnemySlot head = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Mother Brain's room loads her.")).Head;
        EnemyFrameId[] frames = [.. MotherBrainVisualDefinitions.HeadFrames.Select(entry => entry.Value).Append(MotherBrainVisualDefinitions.NeckJointFrame).Distinct()];
        CorpseRotSimulation corpse = CorpseRotSimulation.Of(RuntimeArtworkContent.Of(runtime).EnemyTiles,
            RuntimeArtworkContent.Of(runtime).BossTiles, [EnemyDefinitionId.MotherBrainHead]).Single();
        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        int drawn = 0;
        Draw("as her room loads them");
        // Her death loads her corpse's pages, rots the corpse away, and only then loads the escape's pages (#1513).
        IReadOnlyList<MotherBrainSpritePage> sequence = MotherBrainSpritePageDefinitions.Sequence;
        int lastCorpsePage = sequence.ToList().FindLastIndex(page => page.Sheet == MotherBrainSpritePageSheet.Corpse);
        AssertTrue(lastCorpsePage >= 0, "Mother Brain's artwork sequence loads her corpse");
        foreach (MotherBrainSpritePage page in sequence.Take(lastCorpsePage + 1))
        {
            runtime.Enemies.MotherBrainEncounter.ShowSpritePage(page);
            Draw($"after {page}");
        }
        // The rot's last step uploads nothing, so its rows stay as the step before left them.
        int steps = corpse.StepUploads.Count - 1;
        for (int step = 1; step <= steps; step++)
        {
            runtime.Enemies.MotherBrainEncounter.ShowCorpseRotStep(step);
            // As the corpse rots away its frames come to paint nothing; each still paints exactly what its parts do.
            Draw($"after rot step {step}", requirePixels: false);
        }
        foreach (MotherBrainSpritePage page in sequence.Skip(lastCorpsePage + 1))
        {
            runtime.Enemies.MotherBrainEncounter.ShowSpritePage(page);
            Draw($"after {page} once the corpse has rotted away");
        }
        Console.WriteLine($"Mother Brain pictures: {frames.Length} brain and neck frames draw their pictures after each of {MotherBrainSpritePageDefinitions.Sequence.Count} pages and {steps} rot steps; {drawn} draws paint as the frames over the characters then.");

        void Draw(string context, bool requirePixels = true)
        {
            SpriteArtwork rotted = EnemySheetFixtures.LivePage(set, UploadedPictureChecks.Reached(runtime));
            foreach (EnemyFrameId frame in frames)
            {
                var sprites = new SpriteFrameBuffer();
                sprites.BeginFrame();
                var source = new SpriteSource(SpriteSourceKind.MotherBrainBrain);
                var camera = new Position16<RoomPixels>(unchecked((ushort)(head.Position.X.Whole - 128)), unchecked((ushort)(head.Position.Y.Whole - 112)));
                runtime.Enemies.MotherBrainBehavior.Head.DrawMotherBrainWorldSpritemap(sprites, source,
                    new MotherBrainWorldFrame(frame, head.Position.Whole, head.Shading), camera);
                sprites.FinalizeFrame();
                ImmutableArray<SpriteDraw> pictures = [.. sprites.Draws];
                UploadedPictureChecks.AssertPaintsLikeParts(pictures,
                    [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"Mother Brain {frame} {context}: draws from a sheet"), rotted))],
                    $"Mother Brain {frame} {context}", requirePixels);
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

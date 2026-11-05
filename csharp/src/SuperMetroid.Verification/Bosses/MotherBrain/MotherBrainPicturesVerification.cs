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
/// Confirms Mother Brain draws from pre-coloured pictures of only the steps each frame is drawn at (#1392, #1513): every
/// frame of her room's sheets her pages change shows, at each page it is drawn over, a picture painting exactly what the
/// frame paints over the characters her room's load and every page up to that one leave; and, played in the cartridge's
/// order, every brain and neck frame drawn while the catalogue draws it paints as the frame over the characters then, and
/// drawn at any other step fails loudly. Her corpse's rot is drawn through its rows (#1531), which a separate check confirms.
/// </summary>
internal static class MotherBrainPicturesVerification
{
    /// <summary>Checks every frame her pages change at each page, then draws every brain and neck frame at each page.</summary>
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
        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        IReadOnlyList<MotherBrainSpritePage> sequence = MotherBrainSpritePageDefinitions.Sequence;
        int followed = UploadedPictureChecks.AssertFramesFollowSequence(set, EncounterArtworkSequence.MotherBrainPages, sequence.Count, "Mother Brain's pages");
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        int drawn = 0, undrawn = 0;
        Draw("as her room loads them");
        foreach (MotherBrainSpritePage page in sequence)
        {
            runtime.Enemies.MotherBrainEncounter.ShowSpritePage(page);
            Draw($"after {page}");
        }
        AssertTrue(undrawn > 0, "Mother Brain's brain is drawn at some page at which it is never drawn, and that draw fails");
        Console.WriteLine($"Mother Brain pictures: {followed} frames follow the pages they are drawn over; {frames.Length} brain and neck frames drawn after each of {sequence.Count} pages make {drawn} draws painting as the frames over the characters then, and {undrawn} draws at steps they are never drawn at fail.");

        void Draw(string context)
        {
            SpriteArtwork rotted = EnemySheetFixtures.LivePage(set, UploadedPictureChecks.Reached(runtime));
            foreach (EnemyFrameId frame in frames)
            {
                // A frame drawn at a step at which the catalogue never draws it has no picture there; its draw fails loudly.
                if (!DrawnNow(sheets.Variations(set, EnemyDefinitionId.MotherBrainHead, EnemySheetDefinitions.FrameName(frame))))
                {
                    AssertThrows<InvalidDataException>(() => DrawFrame(frame), $"Mother Brain {frame} {context}, where it is never drawn");
                    undrawn++;
                    continue;
                }
                ImmutableArray<SpriteDraw> pictures = DrawFrame(frame);
                UploadedPictureChecks.AssertPaintsLikeParts(pictures,
                    [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"Mother Brain {frame} {context}: draws from a sheet"), rotted))],
                    $"Mother Brain {frame} {context}");
                drawn++;
            }
        }

        // Whether a frame following the variations is drawn at the step each of its sequences has reached, or before the first.
        bool DrawnNow(IReadOnlyList<FrameArtworkVariation> variations) => variations.All(variation =>
            runtime.EnemySession.ArtworkProgress.StepOf(variation.Sequence) is int step ? variation.DrawnAt(step) : variation.DrawnBeforeFirstStep);

        ImmutableArray<SpriteDraw> DrawFrame(EnemyFrameId frame)
        {
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            var source = new SpriteSource(SpriteSourceKind.MotherBrainBrain);
            var camera = new Position16<RoomPixels>(unchecked((ushort)(head.Position.X.Whole - 128)), unchecked((ushort)(head.Position.Y.Whole - 112)));
            runtime.Enemies.MotherBrainBehavior.Head.DrawMotherBrainWorldSpritemap(sprites, source,
                new MotherBrainWorldFrame(frame, head.Position.Whole, head.Shading), camera);
            sprites.FinalizeFrame();
            return [.. sprites.Draws];
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

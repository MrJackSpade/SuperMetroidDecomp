using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using System.Collections.Immutable;
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
/// Confirms Mother Brain draws from one finished picture of each frame (#1392, #1538): no look gameplay shows changes her
/// brain and neck frames, and every one drawn paints exactly what the frame paints over the characters her room's load and
/// the looks up to the one the frame is drawn in leave, or draws nothing where its finished picture shows nothing. Her
/// corpse's rot is drawn through its rows (#1531), which a separate check confirms.
/// </summary>
internal static class MotherBrainPicturesVerification
{
    /// <summary>Draws every brain and neck frame and checks it paints its finished picture.</summary>
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
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        SpriteSheet sheet = sheets.For(set, EnemyDefinitionId.MotherBrainHead);
        int drawn = 0, empty = 0;
        foreach (EnemyFrameId frame in frames)
        {
            string name = EnemySheetDefinitions.FrameName(frame);
            AssertEqual(0, sheets.ChangingEncounters(set, EnemyDefinitionId.MotherBrainHead, name).Count, $"Mother Brain {frame} has one finished picture");
            ImmutableArray<SpriteDraw> pictures = DrawFrame(frame);
            if (sheet.ShowsNothing(name))
            {
                AssertEqual(0, pictures.Length, $"Mother Brain {frame}, whose finished picture shows nothing, draws nothing");
                empty++;
                continue;
            }
            // A frame no entry draws in a look of hers shows her room's load (#1538).
            int look = FrameDrawings.Of(frame).For(ComposedEncounter.MotherBrain) is { Looks: [int only] } ? only : 0;
            SpriteArtwork live = EnemySheetFixtures.LivePage(set, ComposedEncounter.MotherBrain, look);
            UploadedPictureChecks.AssertPaintsLikeParts(pictures,
                [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"Mother Brain {frame}: draws from a sheet"), live))],
                $"Mother Brain {frame} in her {EncounterLookUploads.Looks(ComposedEncounter.MotherBrain)[look]} look");
            drawn++;
        }
        AssertTrue(drawn > 0, "Mother Brain draws some brain or neck frame");
        Console.WriteLine($"Mother Brain pictures: {drawn} of {frames.Length} brain and neck frames paint their one finished picture over the characters of their look, and {empty} show nothing.");

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

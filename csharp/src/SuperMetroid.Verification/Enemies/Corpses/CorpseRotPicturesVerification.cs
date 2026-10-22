using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// Confirms the rotting corpses draw their rot from pre-coloured pictures (#1392): through each corpse's whole rot, every
/// frame draws the picture of each layer for the rot step each of its rows last uploaded, and that picture paints as the
/// frame composed over the set's characters with those rows as the extraction's rot leaves them; the dust and other
/// objects the room draws meanwhile draw too.
/// </summary>
internal static class CorpseRotPicturesVerification
{
    /// <summary>The Tourian corpses of the Baby Metroid's room, one of each kind, by definition and first population parameter.</summary>
    private static readonly (EnemyDefinitionId Enemy, int Parameter)[] TourianCorpses =
    [
        (EnemyDefinitionId.CorpseSidehopper, 0), (EnemyDefinitionId.CorpseSidehopper, 2),
        (EnemyDefinitionId.CorpseZoomer, 0), (EnemyDefinitionId.CorpseZoomer, 2), (EnemyDefinitionId.CorpseZoomer, 4),
        (EnemyDefinitionId.CorpseRipper, 0), (EnemyDefinitionId.CorpseRipper, 2),
        (EnemyDefinitionId.CorpseSkree, 0), (EnemyDefinitionId.CorpseSkree, 2), (EnemyDefinitionId.CorpseSkree, 4),
    ];

    /// <summary>Rots each Tourian corpse and the dead Torizo through the runtime, checking every frame's pictures.</summary>
    internal static void VerifyCorpseRotPictures()
    {
        foreach ((EnemyDefinitionId enemy, int parameter) in TourianCorpses)
            VerifyRot(RoomHeaderDefinitions.BigBoy, enemy, parameter);
        VerifyRot(RoomHeaderDefinitions.DustTorizo, EnemyDefinitionId.CorpseTorizo, 0);
    }

    /// <summary>Rots one corpse in <paramref name="room"/> to its end, checking each frame's pictures.</summary>
    private static void VerifyRot(RoomHeaderDefinition room, EnemyDefinitionId enemy, int parameter)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        RoomEnemySlot slot = runtime.Enemies.SlotTable.Slots.First(slot => slot.DefinitionId == enemy && slot.Spawn.Population.Parameter1 == parameter);
        string context = $"dead {enemy} {parameter}";
        Rot rot = StartRot(runtime, slot);
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        CorpseRotSimulation simulation = CorpseRotSimulation.Of(runtime.EnemySession.TileArtwork
                ?? throw new InvalidOperationException("The runtime loads the enemy tiles."),
            runtime.Enemies.BossArtworkBinding.Artwork ?? throw new InvalidOperationException("The runtime loads the boss art."), [enemy])
            .Single(corpse => corpse.Flesh == rot.Flesh.ToString());
        AssertTrue(simulation.Rows.Select(Region).ToHashSet().SetEquals(rot.Rows.Keys),
            $"{context}: the extraction's rows are the ones the runtime uploads");
        var shown = rot.Rows.Keys.ToDictionary(region => region, _ => (int?)null);
        var camera = new Position16<RoomPixels>(unchecked((ushort)(slot.Position.X.Whole - 128)), unchecked((ushort)(slot.Position.Y.Whole - 112)));
        int frames = 0, rotted = 0;
        for (int after = 0; after < 2; frames++)
        {
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = camera, TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
            int step = checked((int)rot.Steps());
            foreach (CharacterRegion region in rot.Uploaded())
                shown[region] = step;
            if (!rot.Rotting())
                after++;
            rotted = Math.Max(rotted, step);
            SpriteArtwork loaded = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.ObjCharacters);
            SpriteArtwork rotting = Rotted(loaded, simulation, shown);
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            // The dust each finished row raises, and every other object the room shows, draws over what the rows show.
            runtime.Enemies.Projectiles.DrawHighPriorityEnemyProjectiles(sprites, camera, timeIsFrozen: false);
            runtime.Enemies.Drawing.AdvanceLayers(camera, 0, 7);
            runtime.Enemies.Drawing.DrawLayers(sprites, camera, 0, 7);
            runtime.Enemies.Projectiles.DrawLowPriorityEnemyProjectiles(sprites, camera, timeIsFrozen: false);
            runtime.Enemies.SpriteObjects.DrawRoomSpriteObjects(sprites, camera);
            sprites.FinalizeFrame();

            EnemyFrameId frame = slot.Frame ?? throw new InvalidOperationException($"{context} shows a frame.");
            string name = EnemySheetDefinitions.FrameName(frame);
            var source = new SpriteSource(SpriteSourceKind.Enemy, slot.SlotIndex);
            AssertLayers(runtime, sheets.For(set, enemy), sheets, name, sheets.Layers(set, enemy, name), shown, sprites, source,
                new EnemySpritemapDraw { Frame = frame, Origin = default, PaletteBits = slot.PaletteIndex, BaseTileIndex = slot.VramTilesIndex }, rotting,
                $"{context} after {step} rot steps");
            if (enemy == EnemyDefinitionId.CorpseTorizo)
            {
                string hook = EnemySheetDefinitions.EncounterFrameName(DeadTorizoArtworkDefinitions.HookFrame);
                AssertLayers(runtime, sheets.Spawned(set, 0).Sheet, sheets, hook, sheets.SpawnedLayers(set, 0, hook), shown, sprites,
                    new SpriteSource(SpriteSourceKind.DeadTorizoHook),
                    new EnemySpritemapDraw { Frame = DeadTorizoArtworkDefinitions.HookFrame, Origin = default, PaletteBits = 0, BaseTileIndex = 0 }, rotting,
                    $"{context}'s hook after {step} rot steps");
            }
        }
        AssertTrue(rotted > 0, $"{context} rots");
        Console.WriteLine($"  {context}: {frames} frames through {rotted} rot steps draw the picture of each layer's uploaded steps, painting its frame over the rotted rows.");
    }

    /// <summary>
    /// Checks the draws of <paramref name="source"/> are the pictures of <paramref name="name"/>'s layers for what their rows
    /// show, painting as <paramref name="frame"/> at their origin over <paramref name="rotting"/>.
    /// </summary>
    private static void AssertLayers(SuperMetroidRuntime runtime, SpriteSheet sheet, IEnemySheetSource sheets, string name,
        IReadOnlyList<CharacterRegion[]> layers, Dictionary<CharacterRegion, int?> shown, SpriteFrameBuffer sprites, SpriteSource source,
        EnemySpritemapDraw frame, SpriteArtwork rotting, string context)
    {
        AssertTrue(layers.Any(regions => regions.Any(shown.ContainsKey)), $"{context}: the frame is drawn over the rotting rows");
        IReadOnlyList<string> expected = [.. layers.Select((regions, layer) => sheets.Uploads.PictureName(name, layer,
            [.. regions.Select(region => new CharacterRegionContent(region, null, shown.GetValueOrDefault(region)))]))];
        ImmutableArray<SpriteDraw> drawn = [.. sprites.Draws.Where(draw => draw.Source == source)];
        UploadedPictureChecks.AssertDrawnPictures(sheet, expected, drawn, context);
        CharacterSpriteDraw parted = EnemyPartComposition.Spritemap(rotting, source, runtime.EnemySession.TileArtwork?.Spritemaps
            ?? throw new InvalidOperationException("The runtime loads the enemy compositions."), frame with { Origin = new(drawn[0].Origin.X, drawn[0].Origin.Y) })
            ?? throw new InvalidOperationException($"{context}: the frame has parts.");
        UploadedPictureChecks.AssertPaintsLikeParts(runtime, drawn, [parted], context);
    }

    /// <summary>The set's characters with each row the rot shows replaced by its bytes after the step it shows.</summary>
    private static SpriteArtwork Rotted(SpriteArtwork loaded, CorpseRotSimulation simulation, Dictionary<CharacterRegion, int?> shown)
    {
        byte[] characters = loaded.Characters[..ObjCharacterUploads.TableByteCount].ToArray();
        foreach (CorpseRotRow row in simulation.Rows)
        {
            if (shown[Region(row)] is int step)
                simulation.RowBytes(row, step).CopyTo(characters.AsSpan(row.FirstByte));
        }
        return ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters);
    }

    /// <summary>The OBJ character region a row uploads over.</summary>
    private static CharacterRegion Region(CorpseRotRow row) => new(row.FirstByte, row.ByteCount);

    /// <summary>Starts <paramref name="slot"/>'s rot as touching it does, showing a Sidehopper victim's corpse first.</summary>
    private static Rot StartRot(SuperMetroidRuntime runtime, RoomEnemySlot slot)
    {
        RoomEnemySystem enemies = runtime.Enemies;
        if (slot.DefinitionId == EnemyDefinitionId.CorpseTorizo)
        {
            DeadTorizoEnemyState torizo = enemies.DeadTorizoBehavior.DeadTorizo ?? throw new InvalidOperationException("The dead Torizo is loaded.");
            enemies.DeadTorizoBehavior.TriggerDeadTorizoRotting(slot);
            var halves = new Dictionary<CharacterRegion, bool>();
            foreach (bool odd in new[] { false, true })
            {
                foreach (DeadTorizoVramTransferDefinition transfer in DeadTorizoVramTransferDefinitions.ForHalf(odd))
                    halves.Add(new CharacterRegion(ObjCharacterUploads.TableByte(transfer.Destination), transfer.SizeInBytes), odd);
            }
            return new Rot(new CorpseTransferTable(CorpseRotationTable.Torizo, 0), halves, () => torizo.ProcessCallCount,
                () => torizo.Function == DeadTorizoFunction.Rotting,
                () => halves.Where(row => row.Value == torizo.UploadsOddTransferHalf).Select(row => row.Key));
        }
        if (slot.DefinitionId == EnemyDefinitionId.CorpseSidehopper)
        {
            DeadSidehopperEnemyState sidehopper = slot.RequireSpeciesState<DeadSidehopperEnemyState>();
            if (sidehopper.Function == DeadSidehopperAiFunction.AliveWaitForCamera)
                enemies.DeadSidehopperBehavior.SetDeadSidehopperProgram(slot, DeadTourianProgram.SidehopperCorpse);
            DeadSidehopperBehavior.TriggerDeadSidehopperRotting(slot, sidehopper);
            return Batch(sidehopper.Flesh, () => sidehopper.ProcessCallCount, () => sidehopper.Function == DeadSidehopperAiFunction.Rotting);
        }
        DeadTourianCorpseEnemyState corpse = slot.RequireSpeciesState<DeadTourianCorpseEnemyState>();
        DeadTourianBehavior.TriggerDeadTourianCorpseRotting(slot);
        return Batch(corpse.Flesh, () => corpse.ProcessCallCount, () => corpse.Function == DeadTourianCorpseFunction.Rotting);
    }

    /// <summary>A corpse that uploads all its rows after each rot step.</summary>
    private static Rot Batch(CorpseTransferTable flesh, Func<uint> steps, Func<bool> rotting)
    {
        Dictionary<CharacterRegion, bool> rows = DeadMonsterRottingDefinitions.ForTransferTable(flesh)
            .ToDictionary(transfer => new CharacterRegion(ObjCharacterUploads.TableByte(transfer.Destination), transfer.SizeInBytes), _ => false);
        uint uploaded = 0;
        return new Rot(flesh, rows, steps, rotting, () =>
        {
            // Each rot step uploads every row; a frame without one uploads none.
            uint taken = steps();
            bool stepped = taken != uploaded;
            uploaded = taken;
            return stepped ? rows.Keys : [];
        });
    }

    /// <summary>Loads <paramref name="room"/> as a game reaching it from Zebes would, its uploads drained.</summary>
    private static SuperMetroidRuntime LoadRoom(RoomHeaderDefinition room)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // A session reaching Tourian has started on Zebes, whose game start uploads the standard characters again.
        runtime.HudSetup.QueueStandardObjectTiles();
        runtime.LoadCartridgeRoomForDebug(room);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        return runtime;
    }

    /// <summary>A rotting corpse's rows and how far its rot has gone.</summary>
    /// <param name="Flesh">The corpse's staged characters.</param>
    /// <param name="Rows">Each row it uploads, with whether it is in the odd half.</param>
    /// <param name="Steps">The rot steps it has taken.</param>
    /// <param name="Rotting">Whether it still rots.</param>
    /// <param name="Uploaded">The rows the frame just run uploaded.</param>
    private sealed record Rot(CorpseTransferTable Flesh, Dictionary<CharacterRegion, bool> Rows, Func<uint> Steps, Func<bool> Rotting,
        Func<IEnumerable<CharacterRegion>> Uploaded);

    /// <summary>Runs the corpse rot picture check.</summary>
    [VerificationCommand("--corpse-rot-pictures")]
    internal static int CorpseRotPicturesCommand()
    {
        Suite(nameof(VerifyCorpseRotPictures), VerifyCorpseRotPictures);
        return 0;
    }
}

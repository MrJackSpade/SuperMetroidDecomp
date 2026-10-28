using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
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
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// Confirms a rotting corpse selects the picture of its rot at the update it rots (#1406): through each corpse's whole
/// rot, every update's draw is its frame's picture of the rot the cartridge's uploads would leave its rows showing by then
/// (the set's characters before its first rot step, then the step a dead monster's rows were last uploaded at, or for the
/// dead Torizo the step each half was last uploaded at, the halves alternating from the odd one each update), and that
/// picture paints as the frame composed over the set's characters with those rows as the extraction's rot leaves them.
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

    /// <summary>Rots each Tourian corpse and the dead Torizo through the runtime, checking the picture each update selects.</summary>
    internal static void VerifyCorpseRotPictures()
    {
        foreach ((EnemyDefinitionId enemy, int parameter) in TourianCorpses)
            VerifyRot(RoomHeaderDefinitions.BigBoy, enemy, parameter);
        VerifyRot(RoomHeaderDefinitions.DustTorizo, EnemyDefinitionId.CorpseTorizo, 0);
    }

    /// <summary>Rots one corpse in <paramref name="room"/> to its end, checking each update's selected picture.</summary>
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
            .Single(corpse => corpse.Flesh == rot.Kind.Name);
        var camera = new Position16<RoomPixels>(unchecked((ushort)(slot.Position.X.Whole - 128)), unchecked((ushort)(slot.Position.Y.Whole - 112)));
        int updates = 0, rotted = 0, painted = 0;
        for (int after = 0; after < 2; updates++)
        {
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = camera, TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
            CorpseRotView expected = rot.Expect();
            if (!rot.Rotting())
                after++;
            rotted = Math.Max(rotted, checked((int)rot.Steps()));
            SpriteArtwork rottingArt = Rotted(runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters), simulation, expected,
                $"{context} after update {updates}");
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            runtime.Enemies.Drawing.AdvanceLayers(camera, 0, 7);
            runtime.Enemies.Drawing.DrawLayers(sprites, camera, 0, 7);
            sprites.FinalizeFrame();

            EnemyFrameId frame = slot.Frame ?? throw new InvalidOperationException($"{context} shows a frame.");
            string name = EnemySheetDefinitions.FrameName(frame);
            AssertEqual(rot.Kind.Name, sheets.RotCorpse(set, enemy, name), $"{context}: frame {name} shows its kind's rot");
            painted += AssertSelected(runtime, sheets.For(set, enemy), expected.PictureName(name), sprites, new SpriteSource(SpriteSourceKind.Enemy, slot.SlotIndex),
                new EnemySpritemapDraw { Frame = frame, Origin = default, PaletteBits = slot.PaletteRow, BaseTileIndex = slot.TileIndexBase }, rottingArt,
                $"{context} after update {updates}");
            if (enemy == EnemyDefinitionId.CorpseTorizo)
            {
                string hook = EnemySheetDefinitions.EncounterFrameName(DeadTorizoArtworkDefinitions.HookFrame);
                AssertEqual(rot.Kind.Name, sheets.SpawnedRotCorpse(set, 0, hook), $"{context}: its hook shows its rot");
                painted += AssertSelected(runtime, sheets.Spawned(set, 0).Sheet, expected.PictureName(hook), sprites, new SpriteSource(SpriteSourceKind.DeadTorizoHook),
                    new EnemySpritemapDraw { Frame = DeadTorizoArtworkDefinitions.HookFrame, Origin = default, PaletteBits = 0, BaseTileIndex = 0 }, rottingArt,
                    $"{context}'s hook after update {updates}");
            }
        }
        AssertTrue(rotted > 0, $"{context} rots");
        // A rot step may leave every row rotted away, so a frame may draw nothing; the corpse paints before it is gone.
        AssertTrue(painted > 0, $"{context} paints while it rots");
        Console.WriteLine($"  {context}: {updates} updates through {rotted} rot steps each select the picture of the rot the update leaves.");
    }

    /// <summary>
    /// Checks the draw of <paramref name="source"/> is <paramref name="sheet"/>'s picture <paramref name="picture"/>, painting
    /// as <paramref name="frame"/> at its origin over <paramref name="rotting"/>.
    /// </summary>
    private static int AssertSelected(SuperMetroidRuntime runtime, SpriteSheet sheet, string picture, SpriteFrameBuffer sprites, SpriteSource source,
        EnemySpritemapDraw frame, SpriteArtwork rotting, string context)
    {
        ImmutableArray<SpriteDraw> drawn = [.. sprites.Draws.Where(draw => draw.Source == source)];
        UploadedPictureChecks.AssertDrawnPictures(sheet, [picture], drawn, context);
        CharacterSpriteDraw parted = EnemyPartComposition.Spritemap(rotting, source, runtime.EnemySession.TileArtwork?.Spritemaps
            ?? throw new InvalidOperationException("The runtime loads the enemy compositions."), frame with { Origin = new(drawn[0].Origin.X, drawn[0].Origin.Y) })
            ?? throw new InvalidOperationException($"{context}: the frame has parts.");
        return UploadedPictureChecks.AssertPaintsLikeParts(drawn, [parted], context, requirePixels: false);
    }

    /// <summary>
    /// The set's characters with each of the corpse's rows replaced by its bytes after the step <paramref name="rot"/> shows
    /// it at, as the extraction's rot leaves them.
    /// </summary>
    private static SpriteArtwork Rotted(SpriteArtwork loaded, CorpseRotSimulation simulation, CorpseRotView rot, string context)
    {
        byte[] characters = loaded.Characters[..SpriteArtworkPlacements.TableByteCount].ToArray();
        if (rot.Steps.Any(step => step is not null))
        {
            CorpseRotState state = simulation.States.SingleOrDefault(state => state.View.PictureName("frame") == rot.PictureName("frame"))
                ?? throw new InvalidOperationException($"{context}: the extraction pictures no rot {rot.PictureName("frame")}.");
            foreach ((CorpseRotRow row, int? step) in state.Steps)
            {
                if (step is int taken)
                    simulation.RowBytes(row, taken).CopyTo(characters.AsSpan(row.FirstByte));
            }
        }
        return ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters);
    }

    /// <summary>Starts <paramref name="slot"/>'s rot as touching it does, showing a Sidehopper victim's corpse first.</summary>
    private static Rot StartRot(SuperMetroidRuntime runtime, RoomEnemySlot slot)
    {
        RoomEnemySystem enemies = runtime.Enemies;
        if (slot.DefinitionId == EnemyDefinitionId.CorpseTorizo)
        {
            DeadTorizoEnemyState torizo = enemies.DeadTorizoBehavior.DeadTorizo ?? throw new InvalidOperationException("The dead Torizo is loaded.");
            enemies.DeadTorizoBehavior.TriggerDeadTorizoRotting(slot);
            // Each main-AI call uploads the other half after the steps the corpse has taken, the first call the odd half.
            bool oddNext = !torizo.ShowsOddHalf;
            int? even = torizo.EvenHalfStep, odd = torizo.OddHalfStep;
            return new Rot(DeadTorizoCorpseDefinitions.Kind, () => torizo.ProcessCallCount, () => torizo.Function == DeadTorizoFunction.Rotting, () =>
            {
                int step = checked((int)torizo.ProcessCallCount);
                if (oddNext)
                    odd = step;
                else
                    even = step;
                oddNext = !oddNext;
                return new CorpseRotView(DeadTorizoCorpseDefinitions.Kind, [even, odd]);
            });
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

    /// <summary>
    /// A dead monster, whose rows the cartridge uploads after each rot step it takes: they show the set's characters before
    /// its first step, then the step last taken.
    /// </summary>
    private static Rot Batch(RottingCorpseKind kind, Func<uint> steps, Func<bool> rotting) =>
        new(kind, steps, rotting, () => new CorpseRotView(kind, [steps() is uint taken and > 0 ? checked((int)taken) : null]));

    /// <summary>Loads <paramref name="room"/> as a game reaching it from Zebes would, its uploads drained.</summary>
    private static SuperMetroidRuntime LoadRoom(RoomHeaderDefinition room)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // A session reaching Tourian has started on Zebes, whose game start shows the standard characters again.
        runtime.HudSetup.ShowStandardObjectCharacters();
        runtime.LoadCartridgeRoomForDebug(room);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        return runtime;
    }

    /// <summary>A rotting corpse, how far its rot has gone, and the rot its draw is expected to select after each update.</summary>
    /// <param name="Kind">The corpse's kind.</param>
    /// <param name="Steps">The rot steps it has taken.</param>
    /// <param name="Rotting">Whether it still rots.</param>
    /// <param name="Expect">The rot the update just run leaves its rows showing; called once after each update.</param>
    private sealed record Rot(RottingCorpseKind Kind, Func<uint> Steps, Func<bool> Rotting, Func<CorpseRotView> Expect);

    /// <summary>Runs the corpse rot picture check.</summary>
    [VerificationCommand("--corpse-rot-pictures")]
    internal static int CorpseRotPicturesCommand()
    {
        Suite(nameof(VerifyCorpseRotPictures), VerifyCorpseRotPictures);
        return 0;
    }
}

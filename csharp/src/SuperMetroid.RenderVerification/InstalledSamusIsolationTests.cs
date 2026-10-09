using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Finite, source-selected production-owner fixtures for #541. These compare stock
/// and edited presentation, not a playthrough or a search for cartridge access.
/// </summary>
internal sealed partial class InstalledSamusIsolationTests
{
    /// <summary>Installed artwork fixture containing stock and independently edited presentation data.</summary>
    private readonly InstalledSamusArtworkFixture artwork;
    /// <summary>Stock and edited suit palettes used by the paired actors.</summary>
    private readonly (SamusSuitColorCatalog Stock, SamusSuitColorCatalog Edited) suits;
    /// <summary>Stock and edited full-body animation palettes.</summary>
    private readonly (SamusFullBodyCycleColorCatalog Stock, SamusFullBodyCycleColorCatalog Edited) cycles;
    /// <summary>Stock and edited Crystal Flash palettes.</summary>
    private readonly (CrystalFlashColorCatalog Stock, CrystalFlashColorCatalog Edited) crystal;
    /// <summary>Beam palette data shared by the finite render scenarios.</summary>
    private readonly BeamPaletteCatalog beams;
    /// <summary>Counters for comparisons, draws, changed OAM/VRAM/CGRAM observations, and constructed scenarios.</summary>
    private int comparisons, draws, changedOam, changedTiles, changedColors, scenarios;

    /// <summary>Loads stock and edited palette catalogs for the supplied installed artwork fixture.</summary>
    /// <param name="artwork">Fixture with the original and edited game installations.</param>
    /// <param name="root">Project root used to open the projectile palette asset.</param>
    private InstalledSamusIsolationTests(InstalledSamusArtworkFixture artwork, string root)
    {
        this.artwork = artwork;
        var source = new GameInstallation(Path.GetFullPath(root));
        suits = SamusIsolationArtworkEdits.Colors(source, artwork.Installation,
            SamusSuitColorFormat.FileName, SamusSuitColorCatalog.Load);
        cycles = SamusIsolationArtworkEdits.Colors(source, artwork.Installation,
            SamusFullBodyCycleColorFormat.FileName, SamusFullBodyCycleColorCatalog.Load);
        crystal = SamusIsolationArtworkEdits.Colors(source, artwork.Installation,
            CrystalFlashColorFormat.FileName, CrystalFlashColorCatalog.Load);
        using var stream = File.OpenRead(Path.Combine(source.ProjectileDirectory, BeamPaletteDefinitions.FileName));
        beams = BeamPaletteCatalog.Load(stream);
    }

    /// <summary>Runs the finite stock-versus-edited Samus isolation scenarios and reports observed differences.</summary>
    /// <param name="artwork">Installed artwork fixture under comparison.</param>
    /// <param name="root">Project root containing the extracted projectile palette asset.</param>
    internal static void Run(InstalledSamusArtworkFixture artwork, string root)
    {
        var tests = new InstalledSamusIsolationTests(artwork, root);
        CheckSnapshotSensitivity();
        tests.CheckMovement();
        tests.CheckPoseCollision();
        tests.CheckSpecialSequences();
        Require(tests.changedOam > 0 && tests.changedTiles > 0 && tests.changedColors > 0,
            "The fixture did not exercise observable OAM, tile and color edits.");
        Console.WriteLine($"Samus visual isolation: {tests.scenarios} finite scenarios, " +
            $"{tests.comparisons} complete mutable-state comparisons, {tests.draws} production draws; " +
            $"{tests.changedOam} changed OAM, {tests.changedTiles} changed VRAM and " +
            $"{tests.changedColors} changed CGRAM observations. No ROM, import or player data is used.");
    }

    /// <summary>Creates a stock/edited actor pair with the same initial pose, equipment, medium, and ceiling.</summary>
    private Pair Create(byte pose = SamusPoseIds.FacingRightNormalPose,
        ushort equipment = 0, ushort medium = SamusLiquidPhysicsState.Air, bool lowCeiling = false)
    {
        scenarios++;
        return new Pair(this, pose, equipment, medium, lowCeiling);
    }

    /// <summary>One constructed Samus and the private memory, room, palette, and artwork it owns.</summary>
    private sealed class Actor
    {
        /// <summary>Isolated address space with no cartridge backing.</summary>
        internal readonly SuperMetroidAddressSpace Memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Samus initialized against this actor's isolated dependencies.</summary>
        internal readonly SamusState Samus;
        /// <summary>Constructed collision room containing a solid floor and optional low ceiling.</summary>
        internal readonly RoomLevelData Level;
        /// <summary>Actor-local color memory populated from its selected suit palette.</summary>
        internal readonly SnesCgram Colors = new();
        /// <summary>Actor-local video memory receiving Samus tile transfers.</summary>
        internal readonly SnesVram Tiles = new();
        /// <summary>Selected stock or edited body artwork.</summary>
        internal readonly SamusBodyArtworkCatalog Body;
        /// <summary>Selected stock or edited suit palette.</summary>
        internal readonly SamusSuitColorCatalog Suits;
        /// <summary>Selected stock or edited full-body cycle palette.</summary>
        internal readonly SamusFullBodyCycleColorCatalog Cycles;
        /// <summary>Selected stock or edited Crystal Flash palette.</summary>
        internal readonly CrystalFlashColorCatalog Crystal;

        /// <summary>Builds a self-contained actor using either the stock or edited presentation inputs.</summary>
        /// <param name="tests">Fixture owner supplying both presentation variants.</param>
        /// <param name="edited">Selects edited assets when true and stock assets otherwise.</param>
        /// <param name="pose">Initial native Samus pose.</param>
        /// <param name="equipment">Equipped item bits applied before pose initialization.</param>
        /// <param name="medium">Air, water, or lava/acid physics configuration.</param>
        /// <param name="lowCeiling">Adds a solid ceiling above the actor's floor row.</param>
        internal Actor(InstalledSamusIsolationTests tests, bool edited, byte pose,
            ushort equipment, ushort medium, bool lowCeiling)
        {
            Body = edited ? tests.artwork.Edited : tests.artwork.Stock;
            Suits = edited ? tests.suits.Edited : tests.suits.Stock;
            Cycles = edited ? tests.cycles.Edited : tests.cycles.Stock;
            Crystal = edited ? tests.crystal.Edited : tests.crystal.Stock;
            Samus = new() { Pose = pose, XPosition = 128, YPosition = 363,
                EquippedItems = equipment, Health = 99, MaxHealth = 299,
                SuitColors = Suits, FullBodyCycleColors = Cycles };
            Samus.TileTransfers.BindArtwork(Body);
            Samus.ArmCannon.Artwork = Body.ArmCannon;
            Samus.RefreshCollisionRadii(Memory);
            Samus.YPosition = (ushort)(FloorY - Samus.Kinematics.YRadius);
            Samus.InitializeAnimation(Memory);
            Samus.CommitPoseHistory(Memory);
            if (medium == SamusLiquidPhysicsState.Water) Samus.LiquidPhysics.ConfigureWater(0);
            else if (medium == SamusLiquidPhysicsState.LavaAcid) Samus.LiquidPhysics.ConfigureLavaAcid(0);
            Samus.LoadSuitPalette(Memory, Colors);
            var blocks = new ushort[RoomWidth * RoomHeight];
            for (int x = 0; x < RoomWidth; x++)
            {
                blocks[FloorRow * RoomWidth + x] = (ushort)((int)RoomCollisionType.SolidBlock << 12);
                if (lowCeiling) blocks[(FloorRow - 3) * RoomWidth + x] = (ushort)((int)RoomCollisionType.SolidBlock << 12);
            }
            Level = new(RoomWidth, RoomHeight, blocks, new byte[blocks.Length],
                new ushort[blocks.Length], new byte[8]);
        }
    }

    /// <summary>Keeps matching stock and edited actors synchronized through the same operations.</summary>
    private sealed class Pair
    {
        private readonly InstalledSamusIsolationTests tests;
        /// <summary>Actor bound to the original installation assets.</summary>
        internal Actor Stock { get; }
        /// <summary>Actor bound to the independently edited installation assets.</summary>
        internal Actor Edited { get; }
        /// <summary>Constructs both variants and checks their initial mechanics and memory.</summary>
        /// <param name="tests">Owning scenario fixture.</param>
        /// <param name="pose">Pose supplied to both actors.</param>
        /// <param name="equipment">Equipment bits supplied to both actors.</param>
        /// <param name="medium">Liquid physics mode supplied to both actors.</param>
        /// <param name="lowCeiling">Whether both constructed rooms contain a low ceiling.</param>
        internal Pair(InstalledSamusIsolationTests tests, byte pose,
            ushort equipment, ushort medium, bool lowCeiling)
        {
            this.tests = tests;
            Stock = new(tests, false, pose, equipment, medium, lowCeiling);
            Edited = new(tests, true, pose, equipment, medium, lowCeiling);
            Check("initialization");
        }

        /// <summary>Applies a mutation to both actors, compares state, and optionally renders them.</summary>
        /// <param name="context">Scenario label used in any failure report.</param>
        /// <param name="operation">Mutation applied once to each actor.</param>
        /// <param name="draw">Whether to follow the mutation with a production draw comparison.</param>
        internal void Apply(string context, Action<Actor> operation, bool draw = true)
        {
            operation(Stock); operation(Edited);
            Check(context);
            if (draw) Draw(context);
        }

        /// <summary>Runs the same operation on each actor, checks equal results and state, then optionally draws.</summary>
        /// <typeparam name="T">Operation result type compared between stock and edited actors.</typeparam>
        /// <param name="context">Scenario label used in any failure report.</param>
        /// <param name="operation">Operation evaluated separately for each actor.</param>
        /// <param name="draw">Whether to follow the operation with a production draw comparison.</param>
        internal T Apply<T>(string context, Func<Actor, T> operation, bool draw = true)
        {
            T expected = operation(Stock), actual = operation(Edited);
            Require(EqualityComparer<T>.Default.Equals(expected, actual), context + ": result changed");
            Check(context);
            if (draw) Draw(context);
            return expected;
        }

        /// <summary>Requires matching Samus mechanics, work/save RAM, and collision-room contents.</summary>
        /// <param name="context">Scenario label included in comparison failures.</param>
        internal void Check(string context)
        {
            new SamusMechanicsSnapshot(Stock.Samus).RequireSame(new(Edited.Samus), context);
            Require(Stock.Memory.WorkRam.SequenceEqual(Edited.Memory.WorkRam) &&
                Stock.Memory.SaveRam.SequenceEqual(Edited.Memory.SaveRam), context + ": mutable memory changed");
            Require(Stock.Level.ForegroundEntries.Span.SequenceEqual(Edited.Level.ForegroundEntries.Span),
                context + ": collision room changed");
            tests.comparisons++;
            if (!Stock.Colors.Colors.SequenceEqual(Edited.Colors.Colors)) tests.changedColors++;
        }

        /// <summary>Draws both actors, compares visual outputs, and verifies drawing leaves mechanics unchanged.</summary>
        /// <param name="context">Scenario label included in rendering failures.</param>
        /// <param name="frame">Animation frame passed to both Samus draw calls.</param>
        internal void Draw(string context, ushort frame = 0)
        {
            var beforeStock = new SamusMechanicsSnapshot(Stock.Samus);
            var beforeEdited = new SamusMechanicsSnapshot(Edited.Samus);
            var a = new OamBuffer(); var b = new OamBuffer();
            a.BeginFrame(); b.BeginFrame();
            ushort cameraX = (ushort)Math.Max(0, Stock.Samus.XPosition - 128);
            ushort cameraY = (ushort)Math.Max(0, Stock.Samus.YPosition - 128);
            bool visible = Stock.Samus.Draw(Stock.Memory, a, cameraX, cameraY, frame);
            Require(visible == Edited.Samus.Draw(Edited.Memory, b, cameraX, cameraY, frame),
                context + ": visibility changed");
            Stock.Samus.TileTransfers.TransferToVram(Stock.Memory, Stock.Tiles);
            Edited.Samus.TileTransfers.TransferToVram(Edited.Memory, Edited.Tiles);
            beforeStock.RequireSame(new(Stock.Samus), context + ": stock draw mutated mechanics");
            beforeEdited.RequireSame(new(Edited.Samus), context + ": edited draw mutated mechanics");
            Check(context + " after draw");
            tests.draws++;
            if (!a.LowTable.SequenceEqual(b.LowTable) || !a.HighTable.SequenceEqual(b.HighTable)) tests.changedOam++;
            if (!Stock.Tiles.Bytes.SequenceEqual(Edited.Tiles.Bytes)) tests.changedTiles++;
        }
    }

    // Geometry belongs only to this constructed room, not to any cartridge definition.
    /// <summary>Dimensions and floor placement of the synthetic collision room, expressed in tiles and pixels.</summary>
    private const int RoomWidth = 32, RoomHeight = 32, FloorRow = 24, FloorY = FloorRow * 16;

    /// <summary>Confirms the mechanics snapshot detects representative changes, including private sequence state.</summary>
    private static void CheckSnapshotSensitivity()
    {
        var samus = RepositoryInstallation.CreateSamus();
        var baseline = new SamusMechanicsSnapshot(samus);
        MustDiffer(() => samus.XPosition++, "whole position");
        samus = new(); baseline = new(samus);
        MustDiffer(() => samus.Kinematics.XSubposition++, "fractional position");
        samus = new(); baseline = new(samus);
        MustDiffer(() => samus.AnimationFrame++, "animation frame");
        samus = new(); baseline = new(samus);
        MustDiffer(() => samus.Kinematics.YRadius++, "collision radius");
        samus = PrepareFlash(new());
        var sequence = new SamusMechanicsSnapshot(samus.CrystalFlash);
        Require(samus.CrystalFlash.TryBegin(SuperMetroidAddressSpace.CreateWithoutCartridge(), samus, FlashChord),
            "Sensitivity setup could not activate the sequence");
        try { sequence.RequireSame(new(samus.CrystalFlash), "private sequence sensitivity"); }
        catch (InvalidOperationException) { return; }
        throw new InvalidOperationException("Snapshot ignored changed private sequence state");

        void MustDiffer(Action change, string field)
        {
            change();
            try { baseline.RequireSame(new(samus), "sensitivity"); }
            catch (InvalidOperationException) { return; }
            throw new InvalidOperationException("Snapshot ignored changed " + field);
        }
    }

    /// <summary>Throws an invalid-operation failure when a fixture invariant is false.</summary>
    /// <param name="condition">Invariant that must hold.</param>
    /// <param name="message">Explanation included in the failure exception.</param>
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

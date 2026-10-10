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
    private readonly InstalledSamusArtworkFixture artwork;
    private readonly (SamusSuitColorCatalog Stock, SamusSuitColorCatalog Edited) suits;
    private readonly (SamusFullBodyCycleColorCatalog Stock, SamusFullBodyCycleColorCatalog Edited) cycles;
    private readonly (CrystalFlashColorCatalog Stock, CrystalFlashColorCatalog Edited) crystal;
    private readonly BeamPaletteCatalog beams;
    private int comparisons, draws, changedOam, changedTiles, changedColors, scenarios;

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

    private Pair Create(SamusPoseId pose = SamusPoseId.FacingRightNormalPose,
        ushort equipment = 0, ushort medium = SamusLiquidPhysicsState.Air, bool lowCeiling = false)
    {
        scenarios++;
        return new Pair(this, pose, equipment, medium, lowCeiling);
    }

    private sealed class Actor
    {
        internal readonly SuperMetroidAddressSpace Memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        internal readonly SamusState Samus;
        internal readonly RoomLevelData Level;
        internal readonly SnesCgram Colors = new();
        internal readonly SnesVram Tiles = new();
        internal readonly SamusBodyArtworkCatalog Body;
        internal readonly SamusSuitColorCatalog Suits;
        internal readonly SamusFullBodyCycleColorCatalog Cycles;
        internal readonly CrystalFlashColorCatalog Crystal;

        internal Actor(InstalledSamusIsolationTests tests, bool edited, SamusPoseId pose,
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

    private sealed class Pair
    {
        private readonly InstalledSamusIsolationTests tests;
        internal Actor Stock { get; }
        internal Actor Edited { get; }
        internal Pair(InstalledSamusIsolationTests tests, SamusPoseId pose,
            ushort equipment, ushort medium, bool lowCeiling)
        {
            this.tests = tests;
            Stock = new(tests, false, pose, equipment, medium, lowCeiling);
            Edited = new(tests, true, pose, equipment, medium, lowCeiling);
            Check("initialization");
        }

        internal void Apply(string context, Action<Actor> operation, bool draw = true)
        {
            operation(Stock); operation(Edited);
            Check(context);
            if (draw) Draw(context);
        }

        internal T Apply<T>(string context, Func<Actor, T> operation, bool draw = true)
        {
            T expected = operation(Stock), actual = operation(Edited);
            Require(EqualityComparer<T>.Default.Equals(expected, actual), context + ": result changed");
            Check(context);
            if (draw) Draw(context);
            return expected;
        }

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
    private const int RoomWidth = 32, RoomHeight = 32, FloorRow = 24, FloorY = FloorRow * 16;

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

    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

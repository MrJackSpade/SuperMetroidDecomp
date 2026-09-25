using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyCeresFlightSpriteArtwork(GameInstallation installation,
        SuperMetroidAddressSpace bus, CeresFlightArtworkCatalog stock,
        ISnesAddressSpace guardedBus)
    {
        foreach (CeresFlightSpriteFrameDefinition frame in CeresFlightSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    native.AddOnScreenSpritemap(bus, source, 120, y, 0x0800);
                else
                    native.AddOffScreenSpritemap(bus, source, 120, y, 0x0800);
                native.FinalizeFrame();
                var installed = new OamBuffer();
                installed.BeginFrame();
                stock.Sprites.Draw(frame.Pointer, installed, 120, y, 0x0800, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                        installed.HighTable.SequenceEqual(native.HighTable) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"Ceres flight {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = CeresFlightSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        CeresFlightSpriteDocument document = JsonSerializer.Deserialize<CeresFlightSpriteDocument>(
            File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock Ceres flight sprites are empty.");
        document.Frames["stars"] = document.Frames["stars"]
            .Select(part => part with { OffsetX = part.OffsetX + 16 }).ToArray();
        using (var output = File.Create(overridePath))
            CeresFlightSpritePresentation.Write(output, document);

        CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
        var stockFlight = new IntroCeresFlightState(guardedBus, stock);
        var editedFlight = new IntroCeresFlightState(guardedBus, edited);
        for (int frame = 0; frame < 20; frame++)
        {
            stockFlight.Step();
            editedFlight.Step();
        }
        LayeredRenderSnapshot original = stockFlight.CaptureRenderSnapshot();
        LayeredRenderSnapshot replacement = editedFlight.CaptureRenderSnapshot();
        AssertEqual(IntroCeresFlightPhase.FlyingIntoCamera, editedFlight.Phase,
            "Ceres star edit reaches the visible front approach");
        AssertTrue(original.Memory.Vram.SequenceEqual(replacement.Memory.Vram) &&
                original.Memory.Cgram.SequenceEqual(replacement.Memory.Cgram),
            "Ceres sprite composition edit does not change characters or palette");
        AssertTrue(!original.Memory.Oam.SequenceEqual(replacement.Memory.Oam) &&
                !SoftwareLayeredSnapshotRenderer.Render(original).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(replacement)),
            "Ceres star composition edit changes production OAM and visible pixels");

        stockFlight.BindArtwork(edited);
        AssertTrue(stockFlight.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                replacement.Memory.Oam),
            "restored Ceres flight rebinds the selected actor composition without restarting");

        document.Frames.Remove("stars");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresFlightSpritePresentation.Write(invalid, document);
        }, "Ceres flight sprites reject a missing named frame");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a sprite override cannot hide corrupted stock Ceres sprites");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ceres sprites vanished during stock repair.");
        var repairedFlight = new IntroCeresFlightState(guardedBus,
            repaired.LoadIntroCinematicArt().CeresFlight);
        for (int frame = 0; frame < 20; frame++)
            repairedFlight.Step();
        AssertTrue(repairedFlight.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                replacement.Memory.Oam),
            "Ceres sprite override survives stock extraction repair");
        File.Delete(overridePath);
        Console.WriteLine("  Ceres flight sprites: six native OAM frames, visible edit, rebind, strict validation, and repair preservation pass.");
    }
}

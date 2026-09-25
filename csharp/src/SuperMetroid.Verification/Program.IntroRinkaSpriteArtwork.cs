using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyIntroRinkaSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        const ushort x = 120;
        ushort palette = IntroCinematicRomData.Objects.DiscoveryPalette.Raw;
        foreach (IntroRinkaSpriteFrameDefinition definition in IntroRinkaSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer);
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    native.AddOnScreenSpritemap(bus, source, x, y, palette);
                else
                    native.AddOffScreenSpritemap(bus, source, x, y, palette);
                native.FinalizeFrame();
                var installed = new OamBuffer();
                installed.BeginFrame();
                stock.RinkaSprites.Draw(definition.Pointer, installed, x, y, palette,
                    onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                        installed.HighTable.SequenceEqual(native.HighTable) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro Rinka {definition.Name} at Y=${y:X4} preserves native OAM clipping");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroRinkaSpriteFormat.FileName;
        IntroRinkaSpriteDocument document = JsonSerializer.Deserialize<IntroRinkaSpriteDocument>(
            File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Frames["rinka-0"] = document.Frames["rinka-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroRinkaSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = new IntroArtworkSourceReadGuard(bus, blockIntroRinkas: true);
        var rinkas = new IntroRinkaSystem();
        var samus = new SamusState { XPosition = 0x0200, YPosition = 0x0100 };
        for (int frame = 0; frame <= 75; frame++)
            rinkas.Step(guarded, samus, motherBrainExploding: false);
        AssertEqual(2, rinkas.SpawnedCount,
            "intro Rinka first two-wave actors exist for installed artwork test");
        var stockOam = new OamBuffer();
        stockOam.BeginFrame();
        rinkas.Draw(guarded, stockOam, stock.RinkaSprites);
        stockOam.FinalizeFrame();
        var nativeLiveOam = new OamBuffer();
        nativeLiveOam.BeginFrame();
        rinkas.Draw(bus, nativeLiveOam);
        nativeLiveOam.FinalizeFrame();
        AssertTrue(stockOam.LowTable.SequenceEqual(nativeLiveOam.LowTable) &&
                stockOam.HighTable.SequenceEqual(nativeLiveOam.HighTable),
            "installed intro Rinkas preserve native live-actor OAM placement and attributes");
        var editedOam = new OamBuffer();
        editedOam.BeginFrame();
        rinkas.Draw(guarded, editedOam, edited.RinkaSprites);
        editedOam.FinalizeFrame();
        AssertTrue(!stockOam.LowTable.SequenceEqual(editedOam.LowTable) &&
                stockOam.HighTable.SequenceEqual(editedOam.HighTable),
            "intro Rinka palette edit changes live OAM attributes without moving actors");
        for (int part = 0; part < stockOam.LastFinalizedSpriteCount; part++)
            AssertTrue(stockOam.LowTable.Slice(part * 4, 2)
                    .SequenceEqual(editedOam.LowTable.Slice(part * 4, 2)),
                $"intro Rinka palette edit preserves native part {part} X/Y");

        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockState = new IntroCinematicState(guarded, characterArtwork: stock);
        var editedState = new IntroCinematicState(guarded, characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetField("flashbackRinkas", flags)!
                .SetValue(state, rinkas);
        }
        var render = typeof(IntroCinematicState)
            .GetMethod("RenderMotherBrainFlashback", flags)!;
        Rgba32[] stockPixels = (Rgba32[])render.Invoke(stockState, null)!;
        Rgba32[] editedPixels = (Rgba32[])render.Invoke(editedState, null)!;
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "intro Rinka override changes visible production flashback pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue(((Rgba32[])render.Invoke(stockState, null)!).SequenceEqual(editedPixels),
            "restored intro Rinka scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production intro Rinkas never reread the three native compositions");

        document.Frames.Remove("rinka-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroRinkaSpritePresentation.Write(malformed, document);
        }, "intro Rinka artwork rejects a missing named frame");
        File.Delete(overridePath);
    }
}

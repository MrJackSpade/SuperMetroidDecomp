using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyIntroEggEffectSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        const ushort x = 120;
        ushort palette = IntroCinematicRomData.Objects.DiscoveryPalette.Raw;
        foreach (IntroEggEffectSpriteFrameDefinition definition in
            IntroEggEffectSpriteDefinitions.Frames)
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
                stock.EggEffectSprites.Draw(definition.Pointer, installed, x, y,
                    palette, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                        installed.HighTable.SequenceEqual(native.HighTable) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro egg effect {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroEggEffectSpriteFormat.FileName;
        IntroEggEffectSpriteDocument document =
            JsonSerializer.Deserialize<IntroEggEffectSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Frames["fragment-0"] = document.Frames["fragment-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroEggEffectSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = new IntroArtworkSourceReadGuard(bus, blockIntroEggEffects: true);
        var fragment = new IntroEggParticle(index: 0);
        fragment.Step(guarded);
        var stockOam = new OamBuffer();
        stockOam.BeginFrame();
        fragment.Draw(guarded, stockOam, stock.EggEffectSprites);
        stockOam.FinalizeFrame();
        var nativeLiveOam = new OamBuffer();
        nativeLiveOam.BeginFrame();
        fragment.Draw(bus, nativeLiveOam);
        nativeLiveOam.FinalizeFrame();
        AssertTrue(stockOam.LowTable.SequenceEqual(nativeLiveOam.LowTable) &&
                stockOam.HighTable.SequenceEqual(nativeLiveOam.HighTable),
            "installed egg fragment preserves native live-actor OAM");
        var editedOam = new OamBuffer();
        editedOam.BeginFrame();
        fragment.Draw(guarded, editedOam, edited.EggEffectSprites);
        editedOam.FinalizeFrame();
        AssertTrue(!stockOam.LowTable.SequenceEqual(editedOam.LowTable) &&
                stockOam.HighTable.SequenceEqual(editedOam.HighTable) &&
                stockOam.LowTable.Slice(0, 2).SequenceEqual(editedOam.LowTable.Slice(0, 2)),
            "egg-fragment art edit changes OAM palette without moving its particle");

        var slime = new IntroEggSlimeDrop(0x0070, 0x00b8, index: 0);
        for (int frame = 0; frame < 31; frame++)
        {
            slime.Step(guarded);
            var slimeOam = new OamBuffer();
            slimeOam.BeginFrame();
            slime.Draw(guarded, slimeOam, stock.EggEffectSprites);
            slimeOam.FinalizeFrame();
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockState = new IntroCinematicState(guarded, characterArtwork: stock);
        var editedState = new IntroCinematicState(guarded, characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupBabyDiscoveryCrossfade", flags)!
                .Invoke(state, null);
            var discovery = (IntroBabyDiscoveryState)typeof(IntroCinematicState)
                .GetField("babyDiscovery", flags)!.GetValue(state)!;
            var particles = (List<IntroEggParticle>)typeof(IntroBabyDiscoveryState)
                .GetField("eggParticles", flags)!.GetValue(discovery)!;
            particles.Add(fragment);
        }
        var render = typeof(IntroCinematicState).GetMethod("RenderBabyDiscovery", flags)!;
        Rgba32[] stockPixels = (Rgba32[])render.Invoke(stockState, null)!;
        Rgba32[] editedPixels = (Rgba32[])render.Invoke(editedState, null)!;
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "egg-fragment override changes visible production discovery pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue(((Rgba32[])render.Invoke(stockState, null)!).SequenceEqual(editedPixels),
            "restored egg-discovery scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production egg effects never reread eleven native spritemaps");

        document.Frames.Remove("fragment-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroEggEffectSpritePresentation.Write(malformed, document);
        }, "egg effect artwork rejects a missing named frame");
        File.Delete(overridePath);
    }
}

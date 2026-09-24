using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCrystalFlashColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.CrystalFlashColorExtractor.Extract(rom);
        CrystalFlashColorCatalog native = CrystalFlashColorCatalog.Load(
            new MemoryStream(extracted, writable: false));
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            ushort pointer = ReadWord(rom, SamusPaletteRomData.CrystalFlash.BodyRecords +
                frame * SamusPaletteRomData.CrystalFlash.BodyRecordByteCount);
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                        SamusPaletteRomData.Banks.Palette | (pointer + color * 2)),
                    native.ResolveBody(frame, color),
                    $"Crystal Flash body frame {frame}, color {color} matches cartridge");
        }
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        {
            ushort pointer = ReadWord(rom, SamusPaletteRomData.CrystalFlash.BubblePointers +
                frame * sizeof(ushort));
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                        SamusPaletteRomData.Banks.Palette | (pointer + color * 2)),
                    native.ResolveBubble(frame, color),
                    $"Crystal Flash bubble frame {frame}, color {color} matches cartridge");
        }

        CrystalFlashColorDocument document = JsonSerializer.Deserialize<CrystalFlashColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, CrystalFlashColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Crystal Flash color JSON is null.");
        Paint(document.Body[0]);
        Paint(document.Bubble[0]);
        string replacement = Path.Combine(overrideDirectory, CrystalFlashColorFormat.FileName);
        File.WriteAllBytes(replacement, CrystalFlashColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Crystal Flash color edit changes installed-content identity");
        AssertTrue(edited.CrystalFlashColors.ResolveBody(0, 1) !=
            original.CrystalFlashColors.ResolveBody(0, 1),
            "body color edit differs from stock");
        AssertTrue(edited.CrystalFlashColors.ResolveBubble(0, 1) !=
            original.CrystalFlashColors.ResolveBubble(0, 1),
            "bubble color edit differs from stock");

        SamusState samus = StartCrystalFlash();
        SamusState nativeSamus = StartCrystalFlash();

        var guard = new CrystalFlashColorReadGuard(rom);
        var cgram = new SnesCgram();
        var nativeCgram = new SnesCgram();
        int duration = CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        int calls = CrystalFlashColorFormat.BodyFrameCount * duration;
        for (int call = 0; call < calls; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(guard, cgram, samus,
                    palettes: null, colors: edited.CrystalFlashColors),
                $"Crystal Flash call {call} retains palette ownership");
            AssertTrue(nativeSamus.CrystalFlash.UpdatePalette(rom, nativeCgram, nativeSamus),
                $"native Crystal Flash call {call} retains palette ownership");
            AssertEqual(nativeSamus.CrystalFlash.CrystalPaletteTimer,
                samus.CrystalFlash.CrystalPaletteTimer,
                $"color edit preserves body timer on call {call}");
            AssertEqual(nativeSamus.CrystalFlash.SpecialPaletteTimer,
                samus.CrystalFlash.SpecialPaletteTimer,
                $"color edit preserves bubble timer on call {call}");
            AssertEqual(nativeSamus.CrystalFlash.CommonPaletteTimer,
                samus.CrystalFlash.CommonPaletteTimer,
                $"color edit preserves body cursor on call {call}");
            AssertEqual(nativeSamus.CrystalFlash.SpecialPaletteFrame,
                samus.CrystalFlash.SpecialPaletteFrame,
                $"color edit preserves bubble cursor on call {call}");
            int bodyFrame = call / duration;
            int bubbleFrame = call / 5 % CrystalFlashColorFormat.BubbleFrameCount;
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                AssertEqual(edited.CrystalFlashColors.ResolveBody(bodyFrame, color),
                    cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + color],
                    $"Crystal Flash body call {call}, color {color} reaches CGRAM");
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                AssertEqual(edited.CrystalFlashColors.ResolveBubble(bubbleFrame, color),
                    cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color],
                    $"Crystal Flash bubble call {call}, color {color} reaches CGRAM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Crystal Flash cycle avoids pointer and source-color ROM reads");

        PaletteRgb5 previous = document.Body[0][1];
        document.Body[0][1] = previous with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CrystalFlashColorCatalog.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
                document, MapPresentationFormat.JsonOptions), writable: false)),
            "Crystal Flash colors reject out-of-range RGB5 channels");

        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "crystal-color-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog afterRepair = AreaMapPresentationCatalog.Load(
            replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, afterRepair.ContentIdentity,
            "stock re-extraction preserves Crystal Flash override");
        AssertTrue(overrideBeforeRepair.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)),
            "stock re-extraction never rewrites user Crystal Flash colors");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing Crystal Flash override restores stock identity");
        Console.WriteLine("Crystal Flash colors: 136 stock words, 100 independent body/bubble calls, edited CGRAM, ROM guard, strict values and stock repair pass.");

        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 originalColor = colors[1];
            colors[1] = originalColor with
            {
                Blue = originalColor.Blue == 31 ? 30 : originalColor.Blue + 1,
            };
        }

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            RomDataReader.ReadWordFixedBank(bus, address);

        SamusState StartCrystalFlash()
        {
            var started = new SamusState
            {
                Pose = SamusPoseIds.FacingRightNormalPose,
                XPosition = 128,
                YPosition = 128,
                Health = 1,
                MaxHealth = 99,
                Missiles = 10,
                SuperMissiles = 10,
                PowerBombs = 10,
            };
            started.RefreshCollisionRadii(rom);
            started.HorizontalSpeed.SpecialPaletteTimer = 0;
            AssertTrue(started.CrystalFlash.TryBegin(rom, started, controllerInput: 0,
                skipInputCheck: true), "start real Crystal Flash palette owner");
            return started;
        }
    }

    private sealed class CrystalFlashColorReadGuard : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> forbidden = new();
        public int ForbiddenReadAttempts { get; private set; }

        public CrystalFlashColorReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
            {
                int address = SamusPaletteRomData.CrystalFlash.BodyRecords +
                    frame * SamusPaletteRomData.CrystalFlash.BodyRecordByteCount;
                ushort pointer = RomDataReader.ReadWordFixedBank(source, address);
                forbidden.Add(address);
                forbidden.Add(address + 1);
                BlockColors(pointer, CrystalFlashColorFormat.BodyColorCount);
            }
            for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
            {
                int address = SamusPaletteRomData.CrystalFlash.BubblePointers +
                    frame * sizeof(ushort);
                ushort pointer = RomDataReader.ReadWordFixedBank(source, address);
                forbidden.Add(address);
                forbidden.Add(address + 1);
                BlockColors(pointer, CrystalFlashColorFormat.BubbleColorCount);
            }
        }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Crystal Flash reread palette data ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private void BlockColors(ushort pointer, int count)
        {
            int address = SamusPaletteRomData.Banks.Palette | pointer;
            for (int offset = 0; offset < count * sizeof(ushort); offset++)
                forbidden.Add(address + offset);
        }
    }
}

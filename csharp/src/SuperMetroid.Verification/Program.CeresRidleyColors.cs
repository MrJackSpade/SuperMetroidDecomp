using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresRidleyColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.CeresRidleyColorExtractor.Extract(rom);
        CeresRidleyColorCatalog native = CeresRidleyColorCatalog.Load(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        Check(CeresRidleyPaletteRomData.StartColors,
            CeresRidleyPaletteRomData.StartColorCount, native.ResolveStart);
        for (int row = 0; row < CeresRidleyPaletteRomData.EyeFadeRowCount; row++)
            Check(CeresRidleyPaletteRomData.EyeFadeColors + row *
                CeresRidleyPaletteRomData.EyeFadeColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.EyeFadeColorCount,
                color => native.ResolveEyeFade(row, color));
        for (int row = 0; row < CeresRidleyPaletteRomData.BodyFadeRowCount; row++)
            Check(CeresRidleyPaletteRomData.BodyFadeColors + row *
                CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.BodyFadeColorCount,
                color => native.ResolveBodyFade(row, color));
        for (int row = 0; row < CeresRidleyPaletteRomData.HealthRowCount; row++)
            Check(CeresRidleyPaletteRomData.HealthColors + row *
                CeresRidleyPaletteRomData.HealthColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.HealthColorCount,
                color => native.ResolveHealth(row, color));
        Check(CeresRidleyPaletteRomData.RetreatBgColors,
            CeresRidleyPaletteRomData.RetreatBgColorCount, native.ResolveRetreatBg);
        Check(CeresRidleyPaletteRomData.RetreatSharedColors,
            CeresRidleyPaletteRomData.RetreatSharedColorCount, native.ResolveRetreatShared);

        CeresRidleyColorDocument document = JsonSerializer.Deserialize<CeresRidleyColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, CeresRidleyColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Ceres Ridley colors are null.");
        Paint(document.Start);
        Paint(document.EyeFade[15]);
        Paint(document.BodyFade[0]);
        Paint(document.Health[0]);
        Paint(document.Health[2]);
        Paint(document.RetreatBg);
        Paint(document.RetreatShared);
        string replacement = Path.Combine(overrideDirectory, CeresRidleyColorFormat.FileName);
        File.WriteAllBytes(replacement, CeresRidleyColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Ceres Ridley palette edit changes installed identity");
        var runtime = new SuperMetroidRuntime(rom) { MapPresentation = edited };
        AssertTrue(ReferenceEquals(edited.CeresRidleyColors, runtime.Enemies.CeresRidleyColors),
            "installed runtime binds Ceres Ridley colors to enemy owner");
        runtime.MapPresentation = original;
        AssertTrue(ReferenceEquals(original.CeresRidleyColors, runtime.Enemies.CeresRidleyColors),
            "runtime rebind drops previous Ceres Ridley colors");
        var guarded = new CeresRidleyColorReadGuard(rom, forbidden);
        var enemies = new RoomEnemySystem { CeresRidleyColors = edited.CeresRidleyColors };
        var cgram = new SnesCgram();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        RoomEnemySlot slot = enemies.Slots[0];
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeCeresRidley", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        initialize(slot);
        CheckCgram(document.Start.Length, CeresRidleyPaletteRomData.StartCgramIndex,
            edited.CeresRidleyColors.ResolveStart, "Ceres Ridley initialization");

        var eyeTick = typeof(RoomEnemySystem).GetMethod("TickCeresRidleyEyeFade", flags)!
            .CreateDelegate<Action<RidleyEnemyState>>(enemies);
        var bodyTick = typeof(RoomEnemySystem).GetMethod("TickCeresRidleyBodyFade", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState>>(enemies);
        var healthTick = typeof(RoomEnemySystem).GetMethod("UpdateCeresRidleyHealthPalette", flags)!
            .CreateDelegate<Action<RidleyEnemyState>>(enemies);
        var state = new RidleyEnemyState { Function = RidleyAiFunction.FadeInEyes };
        for (ushort step = 0; step < CeresRidleyEyeFadeDefinitions.PaletteStepCount; step++)
        {
            state.FadePaletteOffset = step;
            state.FunctionTimer = 0;
            eyeTick(state);
            int row = CeresRidleyEyeFadeDefinitions.Get(step).PaletteRow;
            AssertEqual(unchecked((ushort)(step + 1)), state.FadePaletteOffset,
                $"edited eye fade step {step} advances");
            CheckCgram(CeresRidleyPaletteRomData.EyeFadeColorCount,
                CeresRidleyPaletteRomData.EyeFadeCgramIndex,
                color => edited.CeresRidleyColors.ResolveEyeFade(row, color),
                $"Ceres Ridley eye fade step {step}");
        }
        state.Function = RidleyAiFunction.FadeInBody;
        for (int row = 0; row < CeresRidleyPaletteRomData.BodyFadeRowCount; row++)
        {
            state.FadePaletteOffset = (ushort)(row *
                CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort));
            state.FunctionTimer = 1;
            bodyTick(slot, state);
            AssertEqual(row == CeresRidleyPaletteRomData.BodyFadeRowCount - 1
                    ? (ushort)0
                    : (ushort)((row + 1) *
                        CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort)),
                state.FadePaletteOffset, $"edited body fade row {row} advances");
            CheckCgram(CeresRidleyPaletteRomData.BodyFadeColorCount,
                CeresRidleyPaletteRomData.BodyFadeBgCgramIndex,
                color => edited.CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body BG fade row {row}");
            CheckCgram(CeresRidleyPaletteRomData.BodyFadeColorCount,
                CeresRidleyPaletteRomData.BodyFadeObjCgramIndex,
                color => edited.CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body OBJ fade row {row}");
        }
        state.FightMode = 1;
        foreach (ushort count in new ushort[] { 50, 69, 70, 99 })
        {
            state.HitCounter = count;
            healthTick(state);
            int row = count < 70 ? CeresRidleyPaletteRomData.HealthMidRow :
                CeresRidleyPaletteRomData.HealthLateRow;
            CheckCgram(CeresRidleyPaletteRomData.HealthColorCount,
                CeresRidleyPaletteRomData.HealthCgramIndex,
                color => edited.CeresRidleyColors.ResolveHealth(row, color),
                $"Ceres Ridley hit count {count}");
        }
        slot.EnemyDefinitionPointer = 0xe13f;
        RidleyEnemyState activeRidley = enemies.Ridley ??
            throw new InvalidOperationException("Ceres Ridley initialization did not publish state.");
        activeRidley.Function = RidleyAiFunction.CeresRetreatDelay;
        activeRidley.FunctionTimer = 0;
        activeRidley.MovementAnimationEnabled = 0;
        typeof(RoomEnemySystem).GetMethod("RunCeresRidleyMain", flags)!
            .Invoke(enemies, [slot, null, null]);
        AssertEqual(RidleyAiFunction.CeresPublishEscapeHandoff, activeRidley.Function,
            "Ceres Ridley retreat publishes the Mode-7 handoff");
        CheckCgram(CeresRidleyPaletteRomData.RetreatBgColorCount,
            CeresRidleyPaletteRomData.RetreatBgCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatBg, "Ceres Ridley retreat BG");
        CheckCgram(CeresRidleyPaletteRomData.RetreatSharedColorCount,
            CeresRidleyPaletteRomData.RetreatSharedBgCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatShared, "Ceres Ridley retreat shared BG");
        CheckCgram(CeresRidleyPaletteRomData.RetreatSharedColorCount,
            CeresRidleyPaletteRomData.RetreatSharedObjCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatShared, "Ceres Ridley retreat shared OBJ");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Ceres Ridley installed colors avoid their original source ranges");

        document.EyeFade[0][0] = document.EyeFade[0][0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyColorCatalog.Write(document),
            "Ceres Ridley colors reject invalid RGB5 channel");
        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "ceres-ridley-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog repaired = AreaMapPresentationCatalog.Load(replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, repaired.ContentIdentity,
            "Ceres Ridley stock repair preserves override identity");
        AssertTrue(overrideBeforeRepair.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)),
            "Ceres Ridley stock repair does not rewrite override");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity,
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory).ContentIdentity,
            "removing Ceres Ridley override restores stock identity");
        Console.WriteLine("Ceres Ridley colors: 321 native words, initialization/64 eye steps/16 body rows/health thresholds/retreat destinations, ROM guard and stock repair pass.");

        void Check(int source, int count, Func<int, ushort> resolve)
        {
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                forbidden.Add(address);
                forbidden.Add(address + 1);
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, address), resolve(color),
                    $"Ceres Ridley source ${source:X6} color {color}");
            }
        }
        void CheckCgram(int count, int destination, Func<int, ushort> resolve, string context)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram.Colors[destination + color],
                    $"{context} color {color}");
        }
        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 previous = colors[1];
            colors[1] = previous with { Blue = previous.Blue == 31 ? 30 : previous.Blue + 1 };
        }
    }

    private sealed class CeresRidleyColorReadGuard(ISnesAddressSpace inner,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Ceres Ridley color reread ${address:X6}.");
            }
            return inner.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}

using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoLandingLifetimeVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Torizo palette instruction steps without extending the authored landing earthquake.</summary>
internal static class TorizoPaletteShakeVerification
{
    /// <summary>Checks shake duration, masked fades, save-state continuation, and palette catalog bytes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static int VerifyTorizoPaletteShake()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var cgram = new SnesCgram();
        for (int i = 0; i < 256; i++) cgram.SetColor(i, Bgr555.FromWord(0x7fff));
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        enemies.Presentation._cgram = cgram;
        var actor = enemies.SlotTable.Slots[0];
        actor.EnemyDefinitionPointer = EnemyDefinitionId.BombTorizo;
        actor.Definition = RoomEnemyDefinitionCatalog.Get(actor.EnemyDefinitionPointer);
        var state = new TorizoEnemyState(actor, false);
        enemies.BombTorizoBehavior._torizoState = state;
        void Execute(ushort opcode)
        {
            ushort cursor = 0xb939;
            AssertTrue(enemies.BombTorizoBehavior.TryProcessBombTorizoInstruction(
                    actor, new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 },
                    opcode, ref cursor, out _),
                "Torizo instruction handled");
            AssertEqual((ushort)0xb93b, cursor, "Torizo operand-free cursor");
        }
        enemies.Earthquake.EarthquakeType = 4;
        enemies.Earthquake.EarthquakeTimer = NativeCountdown.Start(32);
        // The native awakening loop invokes B271 every four frames. It must not
        // extend a pending landing quake, regardless of the palette target.
        for (int frame = 0; frame < 96; frame++)
        {
            if (frame < 64 && frame % 4 == 0)
                Execute((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange);
            ushort timerBeforeShake = enemies.Earthquake.EarthquakeTimer.RawValue;
            var shake = enemies.HandleRoomShaking(false);
            // An accepted shake frame is exactly one that consumes an earthquake-timer tick.
            AssertEqual(frame < 32, enemies.Earthquake.EarthquakeTimer.RawValue != timerBeforeShake, $"Palette instruction cannot extend landing shake at frame {frame}");
            if (frame < 32)
            {
                AssertEqual((short)0, shake.Bg1.X, "Landing BG1 X");
                AssertEqual((short)(((32 - frame) & 2) == 0 ? 2 : -2), shake.Bg1.Y, "Landing BG1 Y");
            }
        }
        Execute((ushort)TorizoInstruction.Instruction_Torizo_SetupPaletteTransitionToBlack);
        AssertEqual((ushort)0x7fff, cgram.Colors[144], "Black target setup does not black out current palette");
        for (int call = 0; call < 14; call++)
            Execute((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange);
        for (int i = 0; i < 256; i++)
            AssertEqual((ushort)(i is >= 144 and < 176 ? 0 : 0x7fff), cgram.Colors[i], $"Masked fade color {i}");
        AssertEqual((ushort)0, enemies.Earthquake.EarthquakeTimer.RawValue, "Completed fade does not create an earthquake");
        state.PaletteTransition = null;
        for (int i = 0; i < 256; i++) cgram.SetColor(i, Bgr555.FromWord(0x7fff));
        Execute((ushort)TorizoInstruction.Instruction_Torizo_SetupPaletteTransitionToNormalTorizo);
        AssertEqual((ushort)0x7fff, cgram.Colors[145], "Normal target setup preserves current colors");
        Execute((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange);
        AssertEqual((ushort)0x7fff, cgram.Colors[145], "Native transition number zero is a no-op");
        Execute((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange);
        AssertEqual((ushort)0x7bde, cgram.Colors[145], "First denominator-12 step toward native normal color 56BA");
        // Save mid-fade: target and numerator must survive, not restart from white.
        using var saved = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(saved, state);
        saved.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<TorizoEnemyState>(saved);
        var restoredColors = new SnesCgram();
        for (int i = 0; i < 256; i++) restoredColors.SetColor(i, cgram.Colors[i]);
        for (int call = 2; call < 14; call++)
        {
            Execute((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange);
            (restored.PaletteTransition ?? throw new InvalidOperationException("Verification requires restored.PaletteTransition.")).Step(restoredColors, TorizoPaletteDefinitions.BodyPaletteMask);
            AssertTrue(new CgramPalettes(cgram).ImageEquals(new CgramPalettes(restoredColors)), "Mid-fade save preserves each color step");
        }
        VerifyTorizoPaletteCatalogsAgainstRetailRom(cgram);
        Suite(nameof(VerifyTorizoLandingLifetime), () => VerifyTorizoLandingLifetime());
        Console.WriteLine("Torizo palette opcode: repeated calls preserve 32-frame landing shake, defer target writes, fade only sprite palettes 1/2, and create no quake; all five cataloged Torizo palette pairs match the cartridge.");
        return 0;
    }

    /// <summary>Compares the faded body colors and every cataloged Torizo palette row with bank $AA.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Torizo palette tables in bank $AA")]
    private static void VerifyTorizoPaletteCatalogsAgainstRetailRom(SnesCgram cgram)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int i = 0; i < 32; i++)
        {
            int address = 0xaa8707 + i * 2;
            ushort native = rom.ReadCartridgeWord(address);
            AssertEqual(native, cgram.Colors[144 + i], "Normal fade reaches pinned cartridge target");
            address = 0xaa8787 + i * 2;
            native = rom.ReadCartridgeWord(address);
            AssertEqual(native, TorizoPaletteDefinitions.Golden[i], "Shared Golden fade target matches cartridge");
            foreach ((int source, string name, Bgr555 catalog) in new[]
            {
                (0xaa8687, "shared rows eleven/fifteen", TorizoPaletteDefinitions.SharedRows[i]),
                (0xaa86c7, "Bomb Torizo initial body", TorizoPaletteDefinitions.BombInitial[i]),
                (0xaa8747, "Golden Torizo initial body", TorizoPaletteDefinitions.GoldenInitial[i]),
            })
            {
                address = source + i * 2;
                native = rom.ReadCartridgeWord(address);
                AssertEqual(native, catalog, $"Torizo {name} color {i} matches cartridge");
            }
        }
    }

    /// <summary>Runs the focused Torizo palette and landing-shake checks.</summary>
    [VerificationCommand("--torizo-palette-shake")]
    internal static int TorizoPaletteShakeCommand()
    {
        return VerifyTorizoPaletteShake();
    }
}

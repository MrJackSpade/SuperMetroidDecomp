using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static int VerifyTorizoPaletteShake()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var cgram = new SnesCgram();
        for (int i = 0; i < 256; i++) cgram.SetColor(i, 0x7fff);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        var actor = enemies.Slots[0];
        actor.EnemyDefinitionPointer = RoomEnemySystem.BombTorizoDefinition;
        actor.Definition = RoomEnemyDefinitionCatalog.Get(actor.EnemyDefinitionPointer);
        var state = new TorizoEnemyState(actor, false);
        typeof(RoomEnemySystem).GetField("_torizoState", flags)!.SetValue(enemies, state);
        var instruction = typeof(RoomEnemySystem).GetMethod("TryProcessBombTorizoInstruction", flags)!;
        void Execute(ushort opcode)
        {
            object?[] args = [actor, null, null, opcode, (ushort)0xb939, (ushort)0, (byte)0, false];
            AssertTrue((bool)instruction.Invoke(enemies, args)!, "Torizo instruction handled");
            AssertEqual((ushort)0xb93b, (ushort)args[4]!, "Torizo operand-free cursor");
        }
        typeof(RoomEnemySystem).GetProperty(nameof(RoomEnemySystem.EarthquakeType))!.SetValue(enemies, (ushort)4);
        typeof(RoomEnemySystem).GetProperty(nameof(RoomEnemySystem.EarthquakeTimer))!.SetValue(enemies, (ushort)32);
        // The native awakening loop invokes B271 every four frames. It must not
        // extend a pending landing quake, regardless of the palette target.
        for (int frame = 0; frame < 96; frame++)
        {
            if (frame < 64 && frame % 4 == 0)
                Execute(TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange);
            ushort timerBeforeShake = enemies.EarthquakeTimer;
            var shake = enemies.HandleRoomShaking(false);
            // An accepted shake frame is exactly one that consumes an earthquake-timer tick.
            AssertEqual(frame < 32, enemies.EarthquakeTimer != timerBeforeShake, $"Palette instruction cannot extend landing shake at frame {frame}");
            if (frame < 32)
            {
                AssertEqual((short)0, shake.Bg1X, "Landing BG1 X");
                AssertEqual((short)(((32 - frame) & 2) == 0 ? 2 : -2), shake.Bg1Y, "Landing BG1 Y");
            }
        }
        Execute(TorizoInstructionCodes.Instruction_Torizo_SetupPaletteTransitionToBlack);
        AssertEqual((ushort)0x7fff, cgram.Colors[144], "Black target setup does not black out current palette");
        for (int call = 0; call < 14; call++)
            Execute(TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange);
        for (int i = 0; i < 256; i++)
            AssertEqual((ushort)(i >= 144 && i < 176 ? 0 : 0x7fff), cgram.Colors[i], $"Masked fade color {i}");
        AssertEqual((ushort)0, enemies.EarthquakeTimer, "Completed fade does not create an earthquake");
        state.PaletteTransition = null;
        for (int i = 0; i < 256; i++) cgram.SetColor(i, 0x7fff);
        Execute(TorizoInstructionCodes.Instruction_Torizo_SetupPaletteTransitionToNormalTorizo);
        AssertEqual((ushort)0x7fff, cgram.Colors[145], "Normal target setup preserves current colors");
        Execute(TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange);
        AssertEqual((ushort)0x7fff, cgram.Colors[145], "Native transition number zero is a no-op");
        Execute(TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange);
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
            Execute(TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange);
            restored.PaletteTransition!.Step(restoredColors, TorizoPaletteDefinitions.BodyPaletteMask);
            AssertTrue(cgram.Colors.SequenceEqual(restoredColors.Colors), "Mid-fade save preserves each color step");
        }
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int i = 0; i < 32; i++)
        {
            int address = 0xaa8707 + i * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, cgram.Colors[144 + i], "Normal fade reaches pinned cartridge target");
            address = 0xaa8787 + i * 2;
            native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, TorizoPaletteDefinitions.Golden[i], "Shared Golden fade target matches cartridge");
            foreach ((int source, string name, ushort catalog) in new[]
            {
                (0xaa8687, "shared rows eleven/fifteen", TorizoPaletteDefinitions.SharedRows[i]),
                (0xaa86c7, "Bomb Torizo initial body", TorizoPaletteDefinitions.BombInitial[i]),
                (0xaa8747, "Golden Torizo initial body", TorizoPaletteDefinitions.GoldenInitial[i]),
            })
            {
                address = source + i * 2;
                native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(native, catalog, $"Torizo {name} color {i} matches cartridge");
            }
        }
        Suite(nameof(VerifyTorizoLandingLifetime), () => VerifyTorizoLandingLifetime());
        Console.WriteLine("Torizo palette opcode: repeated calls preserve 32-frame landing shake, defer target writes, fade only sprite palettes 1/2, and create no quake; all five cataloged Torizo palette pairs match the cartridge.");
        return 0;
    }
}

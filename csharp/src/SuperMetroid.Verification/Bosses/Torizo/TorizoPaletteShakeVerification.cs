using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoLandingLifetimeVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Torizo palette instruction steps without extending the authored landing earthquake.</summary>
internal static class TorizoPaletteShakeVerification
{
    /// <summary>Checks shake duration, masked fades, save-state continuation, and palette catalog bytes.</summary>
    internal static int VerifyTorizoPaletteShake()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var cgram = new SamusColourRig();
        for (int i = 0; i < 256; i++) cgram.SetColor(i, RgbColor.FromWord(0x7fff));
        enemies.Presentation.BindColours(cgram.Gameplay);
        var actor = enemies.SlotTable.Slots[0];
        actor.Bind(EnemyDefinitionId.BombTorizo, actor.Definition);
        actor.Bind(actor.DefinitionId, RoomEnemyDefinitionCatalog.Get(actor.DefinitionId));
        // The initializer's function words; the build-pinned debugger state below rejects the undefined zero defaults.
        var state = new TorizoEnemyState(actor, false) { Function = TorizoFunction.Falling, PreInstruction = TorizoPreInstruction.Idle };
        enemies.BombTorizoBehavior.Access.TorizoState = state;
        void Execute(TorizoCueKind kind) =>
            AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(
                    actor, state, new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 },
                    new TorizoCue.Command(kind), AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.BombTorizoInitial1)) is null,
                "Torizo palette cue runs on");
        enemies.Earthquake.EarthquakeType = 4;
        enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(32);
        // The native awakening loop invokes B271 every four frames. It must not
        // extend a pending landing quake, regardless of the palette target.
        for (int frame = 0; frame < 96; frame++)
        {
            if (frame < 64 && frame % 4 == 0)
                Execute(TorizoCueKind.AdvancePaletteFade);
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
        Execute(TorizoCueKind.FadeToBlack);
        AssertEqual(0x7fff, cgram.Colors[144], "Black target setup does not black out current palette");
        for (int call = 0; call < 14; call++)
            Execute(TorizoCueKind.AdvancePaletteFade);
        for (int i = 0; i < 256; i++)
            AssertEqual((ushort)(i is >= 144 and < 176 ? 0 : 0x7fff), cgram.Colors[i], $"Masked fade color {i}");
        AssertEqual((ushort)0, enemies.Earthquake.EarthquakeTimer.RawValue, "Completed fade does not create an earthquake");
        state.PaletteTransition = null;
        for (int i = 0; i < 256; i++) cgram.SetColor(i, RgbColor.FromWord(0x7fff));
        Execute(TorizoCueKind.FadeToNormalColors);
        AssertEqual(0x7fff, cgram.Colors[145], "Normal target setup preserves current colors");
        Execute(TorizoCueKind.AdvancePaletteFade);
        AssertEqual(0x7fff, cgram.Colors[145], "Native transition number zero is a no-op");
        Execute(TorizoCueKind.AdvancePaletteFade);
        AssertEqual(0x7bde, cgram.Colors[145], "First denominator-12 step toward native normal color 56BA");
        // Save mid-fade: target and numerator must survive, not restart from white.
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, state);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<TorizoEnemyState>(saved);
        // The restored fade carries its own copy of the statue's colours, which it steps as the live one steps them.
        GradualPaletteTransition restoredFade = restored.PaletteTransition ?? throw new InvalidOperationException("Verification requires restored.PaletteTransition.");
        for (int call = 2; call < 14; call++)
        {
            Execute(TorizoCueKind.AdvancePaletteFade);
            restoredFade.Step();
            AssertTrue(restoredFade.Target.Run(0).Colors.SequenceEqual(cgram.Colors.Slice(144, 32)), "Mid-fade save preserves each color step");
        }
        VerifyTorizoPaletteCatalogsAgainstRetailRom(cgram);
        Suite(nameof(VerifyTorizoLandingLifetime), () => VerifyTorizoLandingLifetime());
        Console.WriteLine("Torizo palette opcode: repeated calls preserve 32-frame landing shake, defer target writes, fade only sprite palettes 1/2, and create no quake; all five cataloged Torizo palette pairs match the cartridge.");
        return 0;
    }

    /// <summary>Compares the faded body colors and every cataloged Torizo palette row with bank $AA.</summary>
    private static void VerifyTorizoPaletteCatalogsAgainstRetailRom(SamusColourRig cgram)
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
            foreach ((int source, string name, RgbColor catalog) in new[]
            {
                new VerifyTorizoPaletteCatalogsAgainstRetailRomCase(0xaa8687, "shared rows eleven/fifteen", TorizoPaletteDefinitions.SharedRows[i]),
                new VerifyTorizoPaletteCatalogsAgainstRetailRomCase(0xaa86c7, "Bomb Torizo initial body", TorizoPaletteDefinitions.BombInitial[i]),
                new VerifyTorizoPaletteCatalogsAgainstRetailRomCase(0xaa8747, "Golden Torizo initial body", TorizoPaletteDefinitions.GoldenInitial[i]),
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

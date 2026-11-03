using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoLandingLifetimeVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Torizo palette instruction steps without extending the authored landing earthquake.</summary>
internal static class TorizoPaletteShakeVerification
{
    /// <summary>Checks shake duration, the fades' shades, save-state continuation, and the import's palette colours.</summary>
    internal static int VerifyTorizoPaletteShake()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        var actor = enemies.SlotTable.Slots[0];
        actor.Bind(EnemyDefinitionId.BombTorizo, actor.Definition);
        actor.Bind(actor.DefinitionId, RoomEnemyHeaders.Get(actor.DefinitionId));
        // The initializer's function words; the build-pinned debugger state below rejects the undefined zero defaults.
        var state = new TorizoEnemyState(actor, false) { Function = TorizoFunction.Falling, FrameBehavior = TorizoFrameBehavior.Idle };
        enemies.BombTorizoBehavior.Access.TorizoState = state;
        void Execute(TorizoCueKind kind) =>
            AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(
                    actor, state, new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 },
                    new TorizoCue.Command(kind), AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.BombTorizoInitial1)) is null,
                "Torizo palette cue runs on");
        enemies.Earthquake.EarthquakeType = 4;
        enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(32);
        // The native awakening loop invokes B271 every four frames. It must not extend a pending landing quake.
        Execute(TorizoCueKind.FadeToNormalColors);
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
        // Sixteen steps: fourteen move the colours, the fifteenth settles them, the sixteenth starts again from the settled colours.
        AssertEqual(PictureShading.Shaded(HitFlash()).With(TorizoShades.Shared).With(
            TorizoShades.Fading(new TorizoLook(TorizoLookKind.Normal), new TorizoLook(TorizoLookKind.Normal), 0)), state.Colours.Shading,
            "A fade toward the settled colours starts again from them");

        PictureShading beforeBlack = state.Colours.Shading;
        Execute(TorizoCueKind.FadeToBlack);
        AssertEqual(beforeBlack, state.Colours.Shading, "Black target setup does not black out the current colours");
        // The running fade's counter stands at one: thirteen more steps move the colours and the fourteenth settles them.
        for (int call = 0; call < 14; call++)
            Execute(TorizoCueKind.AdvancePaletteFade);
        AssertEqual(PictureShading.Shaded(HitFlash()).With(TorizoShades.Shared).With(TorizoShades.Body(new TorizoLook(TorizoLookKind.Black))),
            state.Colours.Shading, "The completed fade settles the body and belly in black");
        AssertEqual((ushort)0, enemies.Earthquake.EarthquakeTimer.RawValue, "Completed fade does not create an earthquake");

        Execute(TorizoCueKind.FadeToNormalColors);
        Execute(TorizoCueKind.AdvancePaletteFade);
        Execute(TorizoCueKind.AdvancePaletteFade);
        // Save mid-fade: the fade's start, target and step must survive, not restart.
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, state);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<TorizoEnemyState>(saved);
        AssertEqual(state.Colours.Shading, restored.Colours.Shading, "Mid-fade save preserves the fade's step");

        // The import pre-colours each step: the black fade's last moving step and the normal fade's are their targets'.
        IReadOnlyList<SheetShade> shades = TorizoSheetShades.Of(InstalledFixtures.EnemyTiles.AuxiliaryColors
            ?? throw new InvalidOperationException("The installed enemy artwork holds the Torizos' colours."));
        SheetShade toBlack = shades.Single(shade => shade.Shade == TorizoShades.Fading(new TorizoLook(TorizoLookKind.Normal), new TorizoLook(TorizoLookKind.Black), 13));
        AssertTrue(toBlack.Inks.SequenceEqual(Enumerable.Range(16, 32)), "The fades recolour only sprite rows one and two");
        AssertTrue(toBlack.Colours.All(colour => colour == RgbColor.Black), "The black fade's last step is black");
        SheetShade toNormal = shades.Single(shade => shade.Shade == TorizoShades.Fading(new TorizoLook(TorizoLookKind.Black), new TorizoLook(TorizoLookKind.Normal), 13));
        VerifyTorizoPaletteCatalogsAgainstRetailRom(toNormal, shades);
        Suite(nameof(VerifyTorizoLandingLifetime), () => VerifyTorizoLandingLifetime());
        Console.WriteLine("Torizo palette opcode: repeated calls preserve 32-frame landing shake, name their fade's shades, fade only sprite rows 1/2, save mid-fade and create no quake; the import's Torizo colours match the cartridge.");
        return 0;

        static PictureShade HitFlash() => SuperMetroid.Core.Enemies.Drawing.EnemyShades.HitFlash;
    }

    /// <summary>Compares the normal fade's last step and every imported Torizo colouring with bank $AA.</summary>
    private static void VerifyTorizoPaletteCatalogsAgainstRetailRom(SheetShade toNormal, IReadOnlyList<SheetShade> shades)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SheetShade golden = shades.Single(shade => shade.Shade == TorizoShades.Body(new TorizoLook(TorizoLookKind.Golden)));
        SheetShade shared = shades.Single(shade => shade.Shade == TorizoShades.Shared);
        SheetShade bombInitial = shades.Single(shade => shade.Shade == TorizoShades.Body(new TorizoLook(TorizoLookKind.BombStatue)));
        SheetShade goldenInitial = shades.Single(shade => shade.Shade == TorizoShades.Body(new TorizoLook(TorizoLookKind.GoldenInitial)));
        for (int i = 0; i < 32; i++)
        {
            AssertEqual(rom.ReadCartridgeWord(0xaa8707 + i * 2), toNormal.Colours[i].ToWord(), "Normal fade reaches pinned cartridge target");
            AssertEqual(rom.ReadCartridgeWord(0xaa8787 + i * 2), golden.Colours[i].ToWord(), "Shared Golden fade target matches cartridge");
            AssertEqual(rom.ReadCartridgeWord(0xaa8687 + i * 2), shared.Colours[i].ToWord(), $"Torizo shared orb/flame and statue-shading colour {i} matches cartridge");
            AssertEqual(rom.ReadCartridgeWord(0xaa86c7 + i * 2), bombInitial.Colours[i].ToWord(), $"Bomb Torizo initial body colour {i} matches cartridge");
            AssertEqual(rom.ReadCartridgeWord(0xaa8747 + i * 2), goldenInitial.Colours[i].ToWord(), $"Golden Torizo initial body colour {i} matches cartridge");
        }
    }

    /// <summary>Runs the focused Torizo palette and landing-shake checks.</summary>
    [VerificationCommand("--torizo-palette-shake")]
    internal static int TorizoPaletteShakeCommand()
    {
        return VerifyTorizoPaletteShake();
    }
}

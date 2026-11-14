using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Fx.PaletteFx.LookupPaletteFxAreasVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.LookupPaletteFxDispatchVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

/// <summary>Checks compiled palette-FX definitions, area selection, and production spawn metadata.</summary>
internal static class RoomPaletteFxDefinitionsVerification
{
    /// <summary>
    /// The bank-$8D definitions of the Nintendo boot logo's fade (#1686), the cinematic fade, the dark lightning, the Wrecked
    /// Ship lights' copy and the Tourian glow's copy, which no scene spawns and no room's record sets, and the empty object
    /// the unused area bits select, which shows nothing (#1827), so they are no effect of the game.
    /// </summary>
    private static readonly HashSet<ushort> UnreachableDefinitions = [0xe198, 0xe1ec, 0xf745, 0xf769, 0xf771, 0xf7a5];

    /// <summary>Audits the 57 effect entries and eight area lists, then checks spawns and room loads without metadata reads.</summary>
    internal static void VerifyRoomPaletteFxDefinitions(CartridgeImportAddressSpace rom)
    {

        ushort[] pointers =
        [
            .. DefinitionRange(
                RoomPaletteFxDefinitions.CinematicDefinitionsBegin,
                RoomPaletteFxDefinitions.CinematicDefinitionsEnd),
            .. DefinitionRange(
                RoomPaletteFxDefinitions.RoomDefinitionsBegin,
                RoomPaletteFxDefinitions.RoomDefinitionsEnd),
            .. DefinitionRange(
                RoomPaletteFxDefinitions.TourianDefinitionsBegin,
                RoomPaletteFxDefinitions.TourianDefinitionsEnd),
        ];
        pointers = [.. pointers.Where(pointer => !UnreachableDefinitions.Contains(pointer))];
        Suite(nameof(VerifyPaletteFxDispatchDomain), () => VerifyPaletteFxDispatchDomain());
        Suite(nameof(VerifyPaletteFxSetupSelection), () => VerifyPaletteFxSetupSelection(rom));
        Suite(nameof(VerifyPaletteFxInitialListSelection), () => VerifyPaletteFxInitialListSelection(rom));
        AssertEqual(57, pointers.Length, "compiled palette-FX definition count");

        var forbidden = new HashSet<int>();
        int genericCompiledPrograms = 0;
        int specializedCompiledPrograms = 0;
        foreach (ushort pointer in pointers)
        {
            int address = RoomFxBankSourceDefinitions.PaletteFx | pointer;
            for (int index = 0; index < RoomPaletteFxDefinitions.DefinitionByteCount; index++)
                forbidden.Add(address + index);

            ColourEffectDefinition definition = ColourEffectSelection.Get(pointer);
            if (definition.InitialAnimation == ColourEffectAnimation.HyperBeam)
            {
                specializedCompiledPrograms++;
            }
            else
            {
                AssertTrue(Enum.IsDefined(definition.InitialAnimation),
                    $"palette-FX ${pointer:X4} begins a typed program");
                genericCompiledPrograms++;
            }
        }
        AssertEqual(56, genericCompiledPrograms,
            "generic palette-FX definitions with typed initial programs");
        AssertEqual(1, specializedCompiledPrograms,
            "specialized Hyper Beam palette-FX definition count");

        Suite(nameof(VerifyPaletteFxAreaListPointers), () => VerifyPaletteFxAreaListPointers(rom));
        Suite(nameof(VerifyPaletteFxAreaSelections), () => VerifyPaletteFxAreaSelections(rom));
        for (int area = 0; area < RoomPaletteFxDefinitions.AreaCount; area++)
        {
            int pointerAddress = RoomFxNativeSources.AreaPaletteFxObjectListPointers + area * 2;
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            ushort nativeList = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointerAddress);
            for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
            {
                int entryAddress = RoomFxRomDataBanksTooling.RoomDefinitions |
                    unchecked((ushort)(nativeList + bit * 2));
                forbidden.Add(entryAddress);
                forbidden.Add(entryAddress + 1);
            }
        }

        var guardedRom = NewRoomPaletteFxDefinitionGuard(rom, forbidden);
        Suite(nameof(VerifyPaletteFxDispatchSpawns), () => VerifyPaletteFxDispatchSpawns(pointers));
        Suite(nameof(VerifyColourEffectTurnOrder), () => VerifyColourEffectTurnOrder());

        for (int area = 0; area < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; area++)
        for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
        {
            // Tourian's third bit selects the glow copy no room sets, which the decode refuses, and an unused bit starts no
            // effect.
            if ((area, bit) == (SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(AreaId.Tourian), 2) ||
                AreaPaletteFxObjects.GetAreaDefinition(area, bit) is not { } effect)
                continue;
            var definition = new RoomEffectDefinition(LiquidStartLevel: 0, LiquidTargetLevel: 0, LiquidLevelSpeed: default, LiquidMoveDelay: 0,
                Type: RoomFxType.None, VisorAnimation: RoomVisorAnimation.Steady, EffectBlend: null, Liquid: null,
                PaletteEffects: [effect], BlockAnimations: [], PaletteBlend: RoomFxPaletteBlend.None);
            var systemSession = new ColourEffectSessionState();
            var system = ColourEffectSystem.ForDefinition(systemSession, definition,
                equippedItems: 0, areaMiniBossDefeated: false);
            AssertEqual(
                PaletteFxObjectAccess.NativeAddress(effect),
                ActiveColourEffect(system).Id,
                $"area {area} bit {bit} production room load");
        }

        var hyperBeam = new HyperBeamColourEffectState();
        hyperBeam.Spawn();
        hyperBeam.Step(new SamusColours());
        AssertEqual(0, hyperBeam.CurrentFrameIndex,
            "specialized Hyper Beam owner executes with its definition bytes forbidden");

        AssertThrows<InvalidDataException>(
            () => PaletteFxObjectAccess.FromNative(0),
            "zero palette-FX definition is outside the authored domain");
        AssertThrows<InvalidDataException>(
            () => ColourEffectSelection.Get(
                unchecked((ushort)(RoomPaletteFxDefinitions.RoomDefinitionsBegin + 1))),
            "unaligned palette-FX definition is outside the authored domain");
        AssertThrows<ArgumentOutOfRangeException>(
            () => AreaPaletteFxObjects.GetAreaDefinition(-1, 0),
            "negative palette-FX area index");
        AssertThrows<ArgumentOutOfRangeException>(
            () => AreaPaletteFxObjects.GetAreaDefinition(0, 8),
            "out-of-range palette-FX bit index");

        Console.WriteLine(
            "Palette-FX definitions: 57 setup/list records, eight native area lists, " +
            "64 area selections, every production spawn, all retail room-load selections, " +
            "56 compiled generic program entries, and the specialized Hyper Beam owner pass " +
            "with fixed metadata reads forbidden.");
    }

    /// <summary>Checks each definition's started effect: its identity, countdown, condition and initial program.</summary>
    internal static void VerifyPaletteFxDispatchSpawns(IEnumerable<ushort> pointers)
    {
        foreach (ushort pointer in pointers)
        {
            ColourEffectDefinition definition = ColourEffectSelection.Get(pointer);
            var systemSession = new ColourEffectSessionState();
            var system = new ColourEffectSystem(systemSession);
            system.Start(pointer, equippedItems: 0);
            RoomColourEffect effect = ActiveColourEffect(system);
            AssertEqual(pointer, effect.Id, $"${pointer:X4} production ID");
            AssertEqual(1, effect.UpdatesUntilNextStep,
                $"${pointer:X4} production countdown");
            // Every native start installs a pre-instruction that does nothing, the intro's included: no condition.
            AssertEqual(null, effect.Condition, $"${pointer:X4} production condition");
            Animation<ColourEffectAnimation> expected = ColourEffectAnimations.Of(definition.Start == ColourEffectStart.ChoosesHeatAnimationForSuit
                ? ColourEffectHeatAnimations.Start(ColourEffectHeatSuit.Power)
                : definition.InitialAnimation);
            AssertTrue(ReferenceEquals(expected, effect.Animation), $"${pointer:X4} production initial animation");
            AssertEqual(AnimationPlace.StartOfAnimation, effect.Place, $"${pointer:X4} production starts its animation");
        }

    }

    /// <summary>
    /// Checks the colour effects' turn order (#1554, #1598): effects take turns in the order they started, an effect that
    /// ends leaves the turns, an effect started later joins after every running effect, and any number of effects run at
    /// once.
    /// </summary>
    internal static void VerifyColourEffectTurnOrder()
    {
        var conditions = new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false };
        var system = new ColourEffectSystem(new ColourEffectSessionState());
        system.Start(ColourEffectId.TitleScreenFlickeringDisplays, 0);
        system.Start(ColourEffectId.FadeInSuperMetroidTitleLogo, 0);
        system.Start(ColourEffectId.TitleScreenBabyMetroidTubeLight, 0);
        AssertTrue(system.effects.Select(effect => effect.Effect).SequenceEqual(
            [ColourEffectId.TitleScreenFlickeringDisplays, ColourEffectId.FadeInSuperMetroidTitleLogo, ColourEffectId.TitleScreenBabyMetroidTubeLight]),
            "effects take turns in start order");

        // The title logo's fade ends by itself; the loops keep running.
        int updates = 0;
        while (system.IsRunning(ColourEffectId.FadeInSuperMetroidTitleLogo))
        {
            AssertTrue(++updates <= 64, "the title logo's fade ends");
            system.StepScreen(conditions);
        }
        AssertTrue(system.effects.Select(effect => effect.Effect).SequenceEqual(
            [ColourEffectId.TitleScreenFlickeringDisplays, ColourEffectId.TitleScreenBabyMetroidTubeLight]),
            "an ended effect leaves the turns");

        system.Start(ColourEffectId.FadeInNintendoCopyright, 0);
        AssertTrue(system.effects.Select(effect => effect.Effect).SequenceEqual(
            [ColourEffectId.TitleScreenFlickeringDisplays, ColourEffectId.TitleScreenBabyMetroidTubeLight, ColourEffectId.FadeInNintendoCopyright]),
            "a later start joins after every running effect");
        system.StepScreen(conditions);
        ColourEffectColours[] shown = [.. system.Shown.Select(record => record.Colours)];
        // The title logo's fade ran to its end, so its final look lies under every running effect (#1597).
        AssertTrue(shown.SequenceEqual([ColourEffectColours.TitleLogo, ColourEffectColours.TitleScreenDisplays,
                ColourEffectColours.BabyMetroidTubeLight, ColourEffectColours.NintendoCopyright]),
            "the settled logo lies first, then the running effects' records in the order they started");
        AssertEqual<ColourEffectFrame?>(new ColourEffectFrame(ColourEffectAnimation.FadeInNintendoCopyright, 0),
            system.FrameOf(ColourEffectColours.NintendoCopyright), "the copyright shows its first frame on its first turn");

        // No cap holds the effects back (#1598): a ninth effect starts like the first.
        var many = new ColourEffectSystem(new ColourEffectSessionState());
        for (int started = 0; started < 9; started++)
            many.Start(ColourEffectId.TitleScreenFlickeringDisplays, 0);
        AssertEqual(9, many.effects.Count, "any number of effects run at once");
    }

    /// <summary>Enumerates native fixed-size definition records within one bank-$8D range.</summary>
    private static IEnumerable<ushort> DefinitionRange(ushort first, ushort last)
    {
        for (int pointer = first; pointer <= last; pointer +=
            RoomPaletteFxDefinitions.DefinitionByteCount)
        {
            yield return unchecked((ushort)pointer);
        }
    }

    /// <summary>The one effect running after a production start or room load.</summary>
    private static RoomColourEffect ActiveColourEffect(ColourEffectSystem system) => system.effects is [RoomColourEffect effect]
        ? effect
        : throw new InvalidOperationException($"Expected one running colour effect, found {system.effects.Count}.");

    /// <summary>Rejects runtime reads of compiled definition and area-list metadata bytes.</summary>
    internal static CartridgeReadGuard NewRoomPaletteFxDefinitionGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Palette-FX runtime reread compiled metadata byte ${address:X6}.");

    /// <summary>Loads retail data and runs the palette-FX metadata audit from the command dispatcher.</summary>
    [VerificationCommand("--room-palette-fx-definitions")]
    internal static int RoomPaletteFxDefinitionsCommand()
    {
        Suite(nameof(VerifyRoomPaletteFxDefinitions), () => VerifyRoomPaletteFxDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}

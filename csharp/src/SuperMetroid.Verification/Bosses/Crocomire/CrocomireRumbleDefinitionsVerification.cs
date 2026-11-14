using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Checks Crocomire's wall-rumble cursor records and death-sequence timing against retail data.</summary>
internal static class CrocomireRumbleDefinitionsVerification
{
    /// <summary>The marker the rumble table holds twice after its last step, as the last oscillation's timing.</summary>
    private const ushort RumbleEndMarker = 0x8080;

    /// <summary>Audits all source words, then compares every rumble update through wall-spike palette publication.</summary>
    internal static void VerifyCrocomireRumbleDefinitions(CartridgeImportAddressSpace rom)
    {
        const int sourceAddress = 0xa498ca;

        // The table's words from the first oscillation on (#1879): each oscillation's return offset and extreme, then
        // the next oscillation's repeats and speed, and the end marker where the last oscillation's successor would be
        // and twice after it. The first oscillation's repeats and speed are the code's, and the two copied prefix words
        // before the table are never a target the rumble reaches, so the oscillations omit them.
        const int firstWord = 2;
        var expectedWords = new ushort[32];
        var assigned = new bool[32];
        int count = CrocomireRumbleDefinitions.Oscillations.Length;
        for (int oscillation = 0; oscillation < count; oscillation++)
        {
            CrocomireRumbleOscillation definition = CrocomireRumbleDefinitions.Oscillations[oscillation];
            int returnIndex = CrocomireNativeWords.RumbleCursor(oscillation, towardExtreme: false) / 2;
            int extremeIndex = CrocomireNativeWords.RumbleCursor(oscillation, towardExtreme: true) / 2;
            AssertEqual((oscillation, false), CrocomireNativeWords.RumbleAt(returnIndex * 2), $"Crocomire rumble oscillation {oscillation} return cursor round trip");
            AssertEqual((oscillation, true), CrocomireNativeWords.RumbleAt(extremeIndex * 2), $"Crocomire rumble oscillation {oscillation} extreme cursor round trip");
            AssertTrue(definition.ReturnOffset >= 0 && definition.ExtremeOffset < 0, $"Crocomire rumble oscillation {oscillation} swings up from its return");
            expectedWords[returnIndex] = unchecked((ushort)definition.ReturnOffset);
            expectedWords[extremeIndex] = unchecked((ushort)definition.ExtremeOffset);
            bool last = oscillation == count - 1;
            expectedWords[extremeIndex + 1] = last ? RumbleEndMarker : (ushort)CrocomireRumbleDefinitions.Oscillations[oscillation + 1].Repeats;
            expectedWords[extremeIndex + 2] = last ? RumbleEndMarker : (ushort)CrocomireRumbleDefinitions.Oscillations[oscillation + 1].Speed;
            assigned[returnIndex] = assigned[extremeIndex] = assigned[extremeIndex + 1] = assigned[extremeIndex + 2] = true;
        }
        int endIndex = CrocomireNativeWords.RumbleCursor(count, towardExtreme: false) / 2;
        foreach (int index in new[] { endIndex, endIndex + 1 })
        {
            expectedWords[index] = RumbleEndMarker;
            assigned[index] = true;
        }

        for (int index = firstWord; index < expectedWords.Length; index++)
        {
            AssertTrue(assigned[index], $"Crocomire rumble source word {index} ownership");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((sourceAddress + index * 2)), expectedWords[index],
                $"Crocomire rumble source word {index}");
        }

        AssertThrows<InvalidDataException>(
            () => CrocomireNativeWords.RumbleAt(8),
            "Crocomire timing word cannot become a target");

        var guarded = NewCrocomireRumbleReadGuard(rom);
        CrocomireColorCatalog colors = CrocomireColorFile.Read(
            new MemoryStream(CrocomireColorExtractor.Extract(rom), writable: false));
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            BossArtwork = new BossSourceArtwork { CrocomireColors = colors },
        };
        // The cartridge's code starts the rumble with ten repeats at one pixel an update: the first oscillation's.
        AssertEqual(10, CrocomireRumbleDefinitions.Oscillations[0].Repeats, "Crocomire rumble starts with the code's repeats");
        AssertEqual(1, CrocomireRumbleDefinitions.Oscillations[0].Speed, "Crocomire rumble starts at the code's speed");
        var death = new CrocomireDeathState
        {
            RumbleYOffset = 0,
            RumbleRepeatsLeft = CrocomireRumbleDefinitions.Oscillations[0].Repeats,
        };
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        var state = new CrocomireEnemyState(body)
        {
            DeathPhase = CrocomireDeathPhase.RumbleHiddenWall,
            RumbleOscillation = 0,
            RumbleTowardExtreme = false,
        };
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;

        // The reference model keeps the cartridge's words; production keeps signed pixel quantities, which the
        // comparisons read back as words.
        ushort referenceIndex = checked((ushort)CrocomireNativeWords.RumbleCursor(state.RumbleOscillation, state.RumbleTowardExtreme));
        ushort referenceY = unchecked((ushort)death.RumbleYOffset);
        ushort referenceCooldown = 10;
        ushort referenceDelta = 1;
        ushort referenceDeathIndex = CrocomireNativeWords.DeathPhases.Word(state.DeathPhase);
        int frames = 0;
        while (referenceIndex != 0x0080 && frames < 2048)
        {
            StepCrocomireRumbleReference(
                rom,
                ref referenceIndex,
                ref referenceY,
                ref referenceCooldown,
                ref referenceDelta,
                ref referenceDeathIndex);
            enemies.CrocomireBehavior.Skeleton.RunCrocomireWallRumble(state);
            frames++;

            // The terminator writes its marker into the word the rumble shares with the melt shake counter; every
            // melt is over by then and nothing reads either again, so production keeps neither the marker nor the place.
            if (state.DeathPhase == CrocomireDeathPhase.RumbleHiddenWall)
            {
                AssertEqual(referenceIndex, CrocomireNativeWords.RumbleCursor(state.RumbleOscillation, state.RumbleTowardExtreme),
                    $"Crocomire rumble frame {frames} target");
                AssertEqual(referenceY, unchecked((ushort)death.RumbleYOffset),
                    $"Crocomire rumble frame {frames} Y offset");
            }
            // The terminator also loads its marker as the next phase's cooldown and delta, which nothing reads because
            // the rumble ends at the next step; production loads no next phase there, so the comparison stops once the marker is loaded.
            if (referenceDelta != 0x8080)
            {
                AssertEqual(referenceCooldown, death.RumbleRepeatsLeft,
                    $"Crocomire rumble frame {frames} repeats left");
                AssertEqual(referenceDelta, CrocomireRumbleDefinitions.Oscillations[state.RumbleOscillation].Speed,
                    $"Crocomire rumble frame {frames} speed");
            }
            AssertEqual(referenceDeathIndex, CrocomireNativeWords.DeathPhases.Word(state.DeathPhase),
                $"Crocomire rumble frame {frames} death index");
        }

        AssertTrue(frames < 2048, "Crocomire rumble production sequence terminates");
        AssertEqual(0x0080, state.DeathPhaseDelay,
            "Crocomire rumble production terminator handoff");

        // The termination loads the wall spikes' colours, which the import pre-colours his sprites in (#1512).
        AssertTrue(state.Colours.ShowsWallSpikes, "Crocomire rumble termination shows the wall spikes' colours");
        SheetShade spikes = BossSheetShades.OwnRow(new BossSourceArtwork { CrocomireColors = colors }, EnemyDefinitionId.Crocomire, 0)
            .Single(shade => shade.Shade == CrocomireShades.WallSpikes);
        for (int color = 0; color < CrocomireColorDefinitions.WallSpikesCount; color++)
            AssertEqual(colors.ResolveWallSpikes(color), spikes.Colours[color],
                $"Crocomire rumble termination applies installed wall-spike color {color}");

        Console.WriteLine(
            $"Crocomire rumble definitions: all 30 reachable native words and {frames} exact production frames pass with the source stream forbidden.");
    }

    /// <summary>Advances a separate cartridge-word reference cursor for one rumble frame.</summary>
    private static void StepCrocomireRumbleReference(
        ISnesAddressSpace rom,
        ref ushort index,
        ref ushort yOffset,
        ref ushort cooldown,
        ref ushort delta,
        ref ushort deathIndex)
    {
        const int sourceAddress = 0xa498ca;

        ushort target = CartridgeImportSource.Require(rom).ReadCartridgeWord((sourceAddress + index));
        if (target == 0x8080)
        {
            yOffset = 0x8080;
            index = 0x0080;
            deathIndex += 2;
            return;
        }

        if (yOffset == target)
        {
            if (unchecked((short)target) < 0)
            {
                if (cooldown != 0)
                {
                    cooldown--;
                    index -= 2;
                    return;
                }

                index += 2;
                cooldown = CartridgeImportSource.Require(rom).ReadCartridgeWord((sourceAddress + index));
                index += 2;
                delta = CartridgeImportSource.Require(rom).ReadCartridgeWord((sourceAddress + index));
            }

            index += 2;
            return;
        }

        yOffset = unchecked((short)(yOffset - target)) >= 0
            ? unchecked((ushort)(yOffset - delta))
            : unchecked((ushort)(yOffset + delta));
    }

    /// <summary>Blocks live reads of rumble records and Crocomire palette source ranges.</summary>
    private static CartridgeReadGuard NewCrocomireRumbleReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa498ca and < 0xa4990a or
                >= CrocomireSourceDefinitions.FightBodyColors and
                < (CrocomireSourceDefinitions.WallSpikesColors +
                    CrocomireColorDefinitions.WallSpikesCount * sizeof(ushort)),
                address => $"Crocomire rumble attempted migrated data read ${address:X6}.");

    /// <summary>Runs the Crocomire rumble comparison from the command dispatcher.</summary>
    [VerificationCommand("--lookup-stream5-crocomire-rumble")]
    internal static int LookupStream5CrocomireRumbleCommand()
    {
        Suite(nameof(VerifyCrocomireRumbleDefinitions), () => VerifyCrocomireRumbleDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}

using SuperMetroid.Core.Samus.Palettes;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Checks Crocomire's wall-rumble cursor records and death-sequence timing against retail data.</summary>
internal static class CrocomireRumbleDefinitionsVerification
{
    /// <summary>The marker the rumble table holds twice after its last step, as the last oscillation's timing.</summary>
    private const ushort RumbleEndMarker = 0x8080;

    /// <summary>Audits all source words, then compares every rumble update through wall-spike palette publication.</summary>
    internal static void VerifyCrocomireRumbleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa498ca;

        // The table's words from the first step's target on: each step's target at its cursor, a negative target's
        // cooldown and delta in the two words after it, and the end marker twice after the last step. The two copied
        // prefix words before the first step are never a target the rumble reaches, so the schedule omits them.
        const int firstWord = 2;
        var expectedWords = new ushort[32];
        var assigned = new bool[32];
        int stepCount = CrocomireRumbleDefinitions.All.Length;
        for (int step = 0; step < stepCount; step++)
        {
            CrocomireRumbleDefinition definition = CrocomireRumble.At(step);
            AssertTrue(!CrocomireRumble.IsComplete(step), $"Crocomire rumble step {step} is a target");
            int targetIndex = CrocomireNativeWords.RumbleCursor(step) / 2;
            AssertEqual(step, CrocomireNativeWords.RumbleStep(targetIndex * 2), $"Crocomire rumble step {step} cursor round trip");
            expectedWords[targetIndex] = unchecked((ushort)definition.TargetYOffset);
            assigned[targetIndex] = true;
            if (definition.TargetYOffset >= 0)
                continue;

            // The last target ends the rumble, so the schedule carries no next phase where the cartridge holds its marker.
            bool last = step == stepCount - 1;
            expectedWords[targetIndex + 1] = last ? RumbleEndMarker : (ushort)definition.Cooldown;
            expectedWords[targetIndex + 2] = last ? RumbleEndMarker : (ushort)definition.Delta;
            assigned[targetIndex + 1] = true;
            assigned[targetIndex + 2] = true;
        }
        AssertTrue(CrocomireRumble.IsComplete(stepCount), "Crocomire rumble completes past its last target");
        int endIndex = CrocomireNativeWords.RumbleCursor(stepCount) / 2;
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
            () => CrocomireRumble.At(stepCount),
            "Crocomire rumble step past the schedule has no target");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumble.IsComplete(stepCount + 1),
            "Crocomire rumble step outside the schedule");
        AssertThrows<InvalidDataException>(
            () => CrocomireNativeWords.RumbleStep(8),
            "Crocomire timing word cannot become a target");

        var guarded = NewCrocomireRumbleReadGuard(rom);
        CrocomireColorCatalog colors = CrocomireColorFile.Read(
            new MemoryStream(CrocomireColorExtractor.Extract(rom), writable: false));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), BossArtwork = new BossSourceArtwork { CrocomireColors = colors },
        };
        var cgram = new SamusColourRig();
        var death = new CrocomireDeathState
        {
            RumbleYOffset = 0,
            RumbleCooldown = WrappingCountdown.Start(10),
            RumbleDelta = 1,
        };
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        var state = new CrocomireEnemyState(body)
        {
            DeathSequenceIndex = CrocomireDeathPhase.RumbleHiddenWall,
            RumbleStep = CrocomireRumble.FirstStep,
        };
        enemies.Presentation.BindColours(cgram.Gameplay);
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;

        // The reference model keeps the cartridge's words; production keeps signed pixel quantities, which the
        // comparisons read back as words.
        ushort referenceIndex = checked((ushort)CrocomireNativeWords.RumbleCursor(state.RumbleStep));
        ushort referenceY = unchecked((ushort)death.RumbleYOffset);
        ushort referenceCooldown = death.RumbleCooldown.RawValue;
        ushort referenceDelta = checked((ushort)death.RumbleDelta);
        // The native dispatcher indexes its table by even byte offset; the state stores the phase in table order.
        ushort referenceDeathIndex = (ushort)((ushort)state.DeathSequenceIndex * 2);
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
            // melt is over by then and nothing reads either again, so production keeps neither the marker nor the step.
            if (state.DeathSequenceIndex == CrocomireDeathPhase.RumbleHiddenWall)
            {
                AssertEqual(referenceIndex, CrocomireNativeWords.RumbleCursor(state.RumbleStep),
                    $"Crocomire rumble frame {frames} target step");
                AssertEqual(referenceY, unchecked((ushort)death.RumbleYOffset),
                    $"Crocomire rumble frame {frames} Y offset");
            }
            // The terminator also loads its marker as the next phase's cooldown and delta, which nothing reads because
            // the rumble ends at the next step; production loads no next phase there, so the comparison stops once the marker is loaded.
            if (referenceDelta != 0x8080)
            {
                AssertEqual(referenceCooldown, death.RumbleCooldown.RawValue,
                    $"Crocomire rumble frame {frames} cooldown");
                AssertEqual(referenceDelta, death.RumbleDelta,
                    $"Crocomire rumble frame {frames} delta");
            }
            AssertEqual(referenceDeathIndex, (ushort)((ushort)state.DeathSequenceIndex * 2),
                $"Crocomire rumble frame {frames} death index");
        }

        AssertTrue(frames < 2048, "Crocomire rumble production sequence terminates");
        AssertEqual((ushort)0x0080, state.DeathPhaseDelay.RawValue,
            "Crocomire rumble production terminator handoff");

        for (int color = 0; color < CrocomireColorDefinitions.WallSpikesCount; color++)
            AssertEqual(colors.ResolveWallSpikes(color),
                cgram.Colors[CrocomireColorDestinations.WallSpikesDestination + color],
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

using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Bosses.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Checks Crocomire's wall-rumble cursor records and death-sequence timing against retail data.</summary>
internal static class CrocomireRumbleDefinitionsVerification
{
    /// <summary>Audits all source words, then compares every rumble update through wall-spike palette publication.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyCrocomireRumbleDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa498ca;

        var expectedWords = new ushort[32];
        var assigned = new bool[32];
        // Walk the native cursor chain: each target's next-target cursor names the following
        // word, skipping the two timing words that only negative targets carry.
        ushort tableOffset = 0;
        foreach (CrocomireRumbleDefinition definition in CrocomireRumbleDefinitions.All)
        {
            AssertEqual(definition, CrocomireRumbleDefinitions.AtOffset(tableOffset),
                $"Crocomire rumble cursor ${tableOffset:X4} resolves its enumerated target");
            int targetIndex = tableOffset >> 1;
            expectedWords[targetIndex] = unchecked((ushort)definition.TargetYOffset);
            assigned[targetIndex] = true;
            bool hasTiming = !definition.IsTerminator && definition.NextTargetOffset == tableOffset + 6;
            tableOffset = (ushort)(definition.IsTerminator
                ? tableOffset + 2
                : definition.NextTargetOffset);
            if (!hasTiming)
                continue;

            expectedWords[targetIndex + 1] = (ushort)definition.Cooldown;
            expectedWords[targetIndex + 2] = (ushort)definition.Delta;
            assigned[targetIndex + 1] = true;
            assigned[targetIndex + 2] = true;
        }

        for (int index = 0; index < expectedWords.Length; index++)
        {
            AssertTrue(assigned[index], $"Crocomire rumble source word {index} ownership");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((sourceAddress + index * 2)), expectedWords[index],
                $"Crocomire rumble source word {index}");
        }

        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(1),
            "Crocomire odd rumble offset");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(8),
            "Crocomire timing word cannot become a target");
        AssertThrows<InvalidDataException>(
            () => CrocomireRumbleDefinitions.AtOffset(0x40),
            "Crocomire rumble offset outside table");

        var guarded = NewCrocomireRumbleReadGuard(rom);
        CrocomireColorCatalog colors = CrocomireColorFile.Read(
            new MemoryStream(CrocomireColorExtractor.Extract(rom), writable: false));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), BossArtwork = new BossArtworkCatalog { CrocomireColors = colors },
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
            RumbleTableOffset = 4,
        };
        enemies.Presentation.BindColours(cgram.Gameplay);
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;

        // The reference model keeps the cartridge's words; production keeps signed pixel quantities, which the
        // comparisons read back as words.
        ushort referenceIndex = checked((ushort)state.RumbleTableOffset);
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

            AssertEqual(referenceIndex, state.RumbleTableOffset,
                $"Crocomire rumble frame {frames} target index");
            // The terminator writes its marker into the word the rumble shares with the melt shake counter;
            // production keeps that marker in the counter and leaves the last offset, which nothing reads again.
            if (state.DeathSequenceIndex == CrocomireDeathPhase.RumbleHiddenWall)
                AssertEqual(referenceY, unchecked((ushort)death.RumbleYOffset),
                    $"Crocomire rumble frame {frames} Y offset");
            AssertEqual(referenceCooldown, death.RumbleCooldown.RawValue,
                $"Crocomire rumble frame {frames} cooldown");
            AssertEqual(referenceDelta, death.RumbleDelta,
                $"Crocomire rumble frame {frames} delta");
            AssertEqual(referenceDeathIndex, (ushort)((ushort)state.DeathSequenceIndex * 2),
                $"Crocomire rumble frame {frames} death index");
        }

        AssertTrue(frames < 2048, "Crocomire rumble production sequence terminates");
        AssertEqual((ushort)0x0080, state.DeathPhaseDelay.RawValue,
            "Crocomire rumble production terminator handoff");
        AssertEqual((ushort)0x8080, death.MeltShakeCounter.RawValue,
            "Crocomire rumble production terminator marker reaches the melt shake counter");
        for (int color = 0; color < CrocomireColorDefinitions.WallSpikesCount; color++)
            AssertEqual(colors.ResolveWallSpikes(color),
                cgram.Colors[CrocomireColorDestinations.WallSpikesDestination + color],
                $"Crocomire rumble termination applies installed wall-spike color {color}");

        Console.WriteLine(
            $"Crocomire rumble definitions: all 32 native words and {frames} exact production frames pass with the source stream forbidden.");
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

using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Verification.Enemies;

/// <summary>Confirms running-cadence stage five reads the Mode 7 queue residue the Ceres getaway leaves (#1338).</summary>
internal static class SpeedBoosterStageFiveResidueVerification
{
    /// <summary>Delay bytes a stage-five cadence can read: ten frames and the loop command.</summary>
    private const int CadenceBytes = 11;

    /// <summary>
    /// Speed Booster stage five's delay-list pointer is <c>$91:0303</c>, inside the Mode 7 transfer queue. Ceres
    /// Ridley's getaway queues the capsule's two descriptors and then the wing's six in the same frame, so the
    /// wing descriptors reach that offset, and the NMI handler clears only the queue's first word. The cadence
    /// must read those residue bytes, rebuilt here from the retail ROM's descriptor lists, instead of a
    /// never-written zero, and keep them after later frames that queue only the capsule.
    /// </summary>
    internal static void VerifySpeedBoosterStageFiveResidue()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ushort stageFive = SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer(
            SamusRunningCadenceDefinitions.MaximumSoundQueueSelection);
        int stageFiveQueueOffset = stageFive - SnesAddressMath.OffsetInBank(Mode7TransferQueueLayout.QueueStart);
        var residue = new SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue();
        for (int index = 0; index < CadenceBytes; index++)
        {
            AssertEqual((byte)0, SamusRunningCadenceDefinitions.ReadAnimationByte(residue, stageFive, (ushort)index),
                "stage five reads boot-cleared queue bytes before any Ceres getaway");
        }

        // The getaway writes its transfer lists into the room's Mode 7 map (#1349).
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The getaway draws no random numbers here."))
            {
                Mode7Map = new Mode7Map(new byte[Mode7Map.Cells], default),
            })
        {
            Presentation = { CeresRidleyMode7Colors = CeresRidleyMode7ColorCatalog.Load(new MemoryStream(CeresRidleyMode7ColorExtractor.Extract(bus))) },
        };
        enemies.Presentation.BindVram(BgArtworkFixtures.For(new SnesVram()));
        enemies.Presentation.BindCgram(new SnesCgram());
        enemies.Room.Session.Mode7QueueResidue = residue;
        var state = new RidleyEnemyState { Mode7TableIndex = 1 };

        // The first NMI advances both: capsule phase one, then wing frame one.
        enemies.CeresRidleyBehavior.TickCeresRidleyMode7Getaway(state, null, 0);
        byte[] expected = NativeQueueImage(bus, CeresMode7TransferList.BabyFrame1, CeresMode7TransferList.WingFrame1);
        AssertTrue(SamusAnimationDelayDefinitions.IsCommand(expected[stageFiveQueueOffset]),
            "the wing's fourth descriptor leaves a command byte, not a delay, at $0303");
        AssertStageFive(residue, stageFive, stageFiveQueueOffset, expected, "after the getaway queues wing frame one");

        // Four NMIs later only the capsule queues; the wing residue at $0303 is untouched.
        enemies.CeresRidleyBehavior.TickCeresRidleyMode7Getaway(state, null, 4);
        AssertStageFive(residue, stageFive, stageFiveQueueOffset, expected, "after a capsule-only frame");

        // Eight NMIs in, capsule phase three (frame one again) and wing frame zero queue together.
        enemies.CeresRidleyBehavior.TickCeresRidleyMode7Getaway(state, null, 8);
        AssertStageFive(residue, stageFive, stageFiveQueueOffset,
            NativeQueueImage(bus, CeresMode7TransferList.BabyFrame1, CeresMode7TransferList.WingFrame0),
            "after the getaway queues wing frame zero");
        Console.WriteLine("  Speed Booster stage five: reads the Ceres getaway's Mode 7 queue residue at $0303 and keeps it across capsule-only frames.");
    }

    /// <summary>
    /// The queue bytes <c>QueueMode7Transfers</c> leaves after one capsule list and one wing list: each descriptor
    /// copied at the running offset, a zero after the last, and the NMI handler's clear of the first word.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Ceres Mode 7 descriptor lists")]
    private static byte[] NativeQueueImage(ISnesAddressSpace bus, CeresMode7TransferList capsule, CeresMode7TransferList wing)
    {
        IImportCartridgeSource rom = CartridgeImportSource.Require(bus);
        const int descriptorBytes = Mode7TransferQueueLayout.VramDescriptorByteCount;
        var image = new byte[Mode7TransferQueueLayout.QueueByteCount];
        int offset = 0;
        foreach (CeresMode7TransferList list in new[] { capsule, wing })
        {
            var source = new SnesAddress(Mode7TransferQueueLayout.CeresSourceBank, (ushort)list);
            for (int descriptor = 0; descriptor < CeresMode7TransferDefinitions.Get(list).Count; descriptor++, offset += descriptorBytes)
            {
                for (int index = 0; index < descriptorBytes; index++)
                    image[offset + index] = rom.ReadCartridgeByte((int)source.AddWithinBank(descriptor * descriptorBytes + index));
                image[offset + descriptorBytes] = 0;
            }
        }
        image[0] = 0;
        image[1] = 0;
        return image;
    }

    /// <summary>Requires the stage-five cadence bytes to equal the native queue residue.</summary>
    private static void AssertStageFive(SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue residue, ushort stageFive, int queueOffset, byte[] expected, string context)
    {
        for (int index = 0; index < CadenceBytes; index++)
        {
            AssertEqual(expected[queueOffset + index],
                SamusRunningCadenceDefinitions.ReadAnimationByte(residue, stageFive, (ushort)index),
                $"stage five cadence byte {index} {context}");
        }
    }

    /// <summary>Runs the Speed Booster stage-five residue suite.</summary>
    [VerificationCommand("--speed-booster-stage-five-residue")]
    internal static int SpeedBoosterStageFiveResidueCommand()
    {
        Suite(nameof(VerifySpeedBoosterStageFiveResidue), () => VerifySpeedBoosterStageFiveResidue());
        return 0;
    }
}

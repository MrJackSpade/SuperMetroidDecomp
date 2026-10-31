using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;

/// <summary>Confirms the unauthored Speed Booster cadence takes the delays the Ceres getaway's wings leave behind (#1338, #1479).</summary>
internal static class SpeedBoosterStageFiveResidueVerification
{
    /// <summary>The floor update queue's bytes, which the native image below rebuilds.</summary>
    private const int QueueBytes = 96;

    /// <summary>
    /// Speed Booster stage five's delay-list pointer is <c>$91:0303</c>, inside the Mode 7 transfer queue. Ceres
    /// Ridley's getaway queues the capsule's two descriptors and then the wing's six in the same frame, so the
    /// wing descriptors reach that offset, and the NMI handler clears only the queue's first word. The cadence's
    /// installed delays must equal those residue bytes, rebuilt here from the retail ROM's descriptor lists, instead of
    /// a never-written zero, and keep them after later frames that queue only the capsule.
    /// </summary>
    internal static void VerifySpeedBoosterStageFiveResidue()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int stageFiveQueueOffset = SamusRunningCadenceCodec.UnauthoredFirstResidueByte(CartridgeImportSource.Require(bus));
        var cadence = new UnauthoredBoostCadence();
        for (int frame = 0; frame < UnauthoredBoostCadence.FrameCount; frame++)
        {
            AssertEqual(0, cadence.Delay(frame),
                "stage five reads boot-cleared queue bytes before any Ceres getaway");
        }

        // The getaway writes its transfer lists into the room's Mode 7 map (#1349).
        var enemies = new RoomEnemySystem(new EnemySession(cadence), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The getaway draws no random numbers here."))
            {
                Floor = RoomBackgroundsFixtures.CeresFloor(),
            })
        {
            BossArtworkBinding =
            {
                CeresRidleyFloorColors = CeresRidleyMode7ColorFile.Read(new MemoryStream(CeresRidleyMode7ColorExtractor.Extract(bus))),
                CeresRidleyGetawayBoostDelays = CeresRidleyGetawayBoostDelayFiles.Read(new MemoryStream(CeresRidleyGetawayBoostDelayFiles.Extract(bus))),
            },
        };
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
        var state = new RidleyEnemyState { GetawayFrameIndex = 1 };

        // The first NMI advances both: capsule phase one, then wing frame one.
        enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyGetawayFloor(state, null, 0);
        byte[] expected = NativeQueueImage(bus, CeresFloorAnimationFrame.BabyFrame1, CeresFloorAnimationFrame.WingFrame1);
        AssertStageFive(cadence, stageFiveQueueOffset, expected, "after the getaway queues wing frame one");

        // Four NMIs later only the capsule queues; the wing residue at $0303 is untouched.
        enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyGetawayFloor(state, null, 4);
        AssertStageFive(cadence, stageFiveQueueOffset, expected, "after a capsule-only frame");

        // Eight NMIs in, capsule phase three (frame one again) and wing frame zero queue together.
        enemies.CeresRidleyBehavior.Getaway.TickCeresRidleyGetawayFloor(state, null, 8);
        AssertStageFive(cadence, stageFiveQueueOffset,
            NativeQueueImage(bus, CeresFloorAnimationFrame.BabyFrame1, CeresFloorAnimationFrame.WingFrame0),
            "after the getaway queues wing frame zero");
        Console.WriteLine("  Speed Booster stage five: takes the delays the Ceres getaway's wings leave at $0303 and keeps them across capsule-only frames.");
    }

    /// <summary>
    /// The queue bytes <c>QueueMode7Transfers</c> leaves after one capsule list and one wing list: each descriptor
    /// copied at the running offset, a zero after the last, and the NMI handler's clear of the first word.
    /// </summary>
    private static byte[] NativeQueueImage(ISnesAddressSpace bus, CeresFloorAnimationFrame capsule, CeresFloorAnimationFrame wing)
    {
        IImportCartridgeSource rom = CartridgeImportSource.Require(bus);
        const int descriptorBytes = 9;
        var image = new byte[QueueBytes];
        int offset = 0;
        foreach (CeresFloorAnimationFrame list in new[] { capsule, wing })
        {
            var source = new SnesAddress(0xa6, CeresNativeWords.FloorAnimationFrames.Word(list));
            for (int descriptor = 0; descriptor < CeresFloorDrawingDefinitions.Frames[list].Count; descriptor++, offset += descriptorBytes)
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

    /// <summary>Requires the unauthored cadence's delays to equal the native queue residue its stream word names.</summary>
    private static void AssertStageFive(UnauthoredBoostCadence cadence, int queueOffset, byte[] expected, string context)
    {
        for (int frame = 0; frame < UnauthoredBoostCadence.FrameCount; frame++)
        {
            AssertEqual(expected[queueOffset + frame], cadence.Delay(frame),
                $"unauthored cadence frame {frame} {context}");
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

using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidBodyContourVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid contour and growth-resume selectors against native comparison logic.</summary>
internal static class KraidContourCaseMappingsVerification
{
    /// <summary>Walks the seven native contour records using wrapped CMP/BPL subtraction.</summary>
    internal static short NativeKraidBodyLeftEdge(CartridgeImportAddressSpace rom, short relativeY)
    {
        for (int record = 0; record < 7; record++)
        {
            int address = 0xa7b161 + 4 * record;
            // Native CMP/BPL uses the sign of wrapped subtraction, not ordered signed comparison.
            if (unchecked((short)(relativeY - rom.ReadCartridgeWord(address))) >= 0 ||
                unchecked((short)(relativeY - rom.ReadCartridgeWord((address + 4)))) >= 0)
                return unchecked((short)rom.ReadCartridgeWord((address + 2)));
        }
        throw new InvalidOperationException("Native Kraid body contour did not terminate within its records.");
    }

    /// <summary>
    /// Checks every relative Y a room can hold against the native selection, and strict projectile overlap at the
    /// staircase's edges. The native wrapped comparison differs only more than 31,745 pixels above Kraid, which no room
    /// reaches (#1876).
    /// </summary>
    internal static void VerifyKraidBodyContourCases(CartridgeImportAddressSpace rom)
    {
        const int roomHeightLimit = SuperMetroid.Core.Rooms.Scrolling.RoomScrollGrid.KeptCellCount * 256;
        for (int relativeY = -roomHeightLimit; relativeY <= roomHeightLimit; relativeY++)
        {
            AssertEqual(NativeKraidBodyLeftEdge(rom, (short)relativeY), KraidBodyContour.LeftEdge(relativeY),
                $"Kraid contour native CMP/BPL selection at Y={relativeY}");
        }
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.Places.InUpdateOrder[0];
        var shot = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake()).Shots[0];
        // The tested shot holds its place as an uncharged Power beam, the type the cleared slot word reads as.
        shot.OccupyAsFired(SuperMetroid.Core.Progression.SamusProjectileType.Beam(SuperMetroid.Core.Progression.SamusBeamCombination.Power, charged: false));
        body.Position = body.Position.WithWhole(new(256, 1024));
        shot.HeldShot().Radius = shot.Radius.WithX(0);
        foreach (short y in new short[] { -113, -112, -1, 0 })
        {
            shot.HeldShot().Position = shot.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + y)));
            int edge = body.Position.X.Whole + NativeKraidBodyLeftEdge(rom, y);
            for (int delta = -1; delta <= 1; delta++)
            {
                shot.HeldShot().Position = shot.Position.WithWholeX((ushort)(edge + delta));
                AssertEqual(delta > 0, overlaps(body, shot), "Contour production edge remains strict");
            }
        }
    }

    /// <summary>Decodes native tilemap comparisons and offsets to find Kraid's head-resume pointer and timer.</summary>
    internal static NativeKraidGrowthResumeResult NativeKraidGrowthResume(CartridgeImportAddressSpace rom, ushort tilemap)
    {
        // Decode the native LDY/CMP immediates rather than using compiled selector constants.
        ushort offset = rom.ReadCartridgeWord(0xa7c042);
        for (int selector = 0; selector < 3; selector++)
        {
            int instruction = 0xa7c029 + 8 * selector;
            if (tilemap != rom.ReadCartridgeWord((instruction + 4))) continue;
            offset = rom.ReadCartridgeWord((instruction + 1));
            break;
        }
        return new NativeKraidGrowthResumeResult((ushort)(rom.ReadCartridgeWord(0xa7c047) + offset), rom.ReadCartridgeWord((0xa70000 | (rom.ReadCartridgeWord(0xa7c04d) + offset))));
    }

    /// <summary>
    /// Checks the typed growth resumes against the native selector for every sixteen-bit word growth could read: the
    /// closed, opening and wider poses' head images select their resume, and every other word selects the default.
    /// </summary>
    internal static void VerifyKraidGrowthResumeCases(CartridgeImportAddressSpace rom)
    {
        var native = new KraidHeadNativeCursors(rom);
        var selected = new Dictionary<ushort, KraidHeadGrowthResume>
        {
            [SuperMetroid.Core.Bosses.Kraid.Artwork.KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Closed)] = new(KraidHeadSegment.RoarClosing, KraidHeadFrame.Closed),
            [SuperMetroid.Core.Bosses.Kraid.Artwork.KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Opening)] = new(KraidHeadSegment.RoarClosing, KraidHeadFrame.Opening),
            [SuperMetroid.Core.Bosses.Kraid.Artwork.KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Wider)] = new(KraidHeadSegment.RoarClosing, KraidHeadFrame.Wider),
        };
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            KraidHeadGrowthResume resume = selected.GetValueOrDefault((ushort)raw, new KraidHeadGrowthResume(KraidHeadSegment.RoarOpenHold, KraidHeadFrame.Open));
            (ushort pointer, ushort timer) = NativeKraidGrowthResume(rom, (ushort)raw);
            KraidHeadEntry resumed = KraidHeadAnimations.GrowthResume(resume);
            AssertEqual(pointer, native.PointerAt(resumed.Playback), "Kraid native word-dependent resume cursor");
            AssertEqual(timer, resumed.Delay, "Kraid native word-dependent resume delay");
        }
    }
}

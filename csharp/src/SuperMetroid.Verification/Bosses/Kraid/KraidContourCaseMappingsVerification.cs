using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidBodyContourVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid contour and growth-resume selectors against native comparison logic.</summary>
internal static class KraidContourCaseMappingsVerification
{
    /// <summary>Walks the seven native contour records using wrapped CMP/BPL subtraction.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid body contour left-edge records")]
    internal static short NativeKraidBodyLeftEdge(SuperMetroidAddressSpace rom, short relativeY)
    {
        for (int record = 0; record < 7; record++)
        {
            int address = 0xa7b161 + 4 * record;
            // Native CMP/BPL uses the sign of wrapped subtraction, not ordered signed comparison.
            if (unchecked((short)(relativeY - CartridgeImportSource.Require(rom).ReadCartridgeWord(address))) >= 0 ||
                unchecked((short)(relativeY - CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)))) >= 0)
                return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)));
        }
        throw new InvalidOperationException("Native Kraid body contour did not terminate within its records.");
    }

    /// <summary>Checks every relative Y mapping and strict projectile overlap at wrapped edge cases.</summary>
    internal static void VerifyKraidBodyContourCases(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            short relativeY = unchecked((short)raw);
            AssertEqual(NativeKraidBodyLeftEdge(rom, relativeY), KraidBodyContour.LeftEdge(relativeY),
                $"Kraid contour native CMP/BPL selection at Y={relativeY}");
        }
        var enemies = new RoomEnemySystem(new EnemySession());
        var overlaps = CreateKraidBodyContourProbe();
        var body = enemies.SlotTable.Slots[0];
        var shot = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None).Slots[0];
        body.Position = body.Position.WithWhole(new(256, 32768));
        shot.Radius = shot.Radius.WithX(0);
        foreach (short y in new short[] { -32768, -31746, -31745 })
        {
            shot.Position = shot.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + y)));
            int edge = body.Position.X.Whole + NativeKraidBodyLeftEdge(rom, y);
            for (int delta = -1; delta <= 1; delta++)
            {
                shot.Position = shot.Position.WithWholeX((ushort)(edge + delta));
                AssertEqual(delta > 0, overlaps(body, shot), "Wrapped contour production edge remains strict");
            }
        }
    }

    /// <summary>Decodes native tilemap comparisons and offsets to find Kraid's head-resume pointer and timer.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid growth head-resume immediates")]
    internal static NativeKraidGrowthResumeResult NativeKraidGrowthResume(SuperMetroidAddressSpace rom, ushort tilemap)
    {
        // Decode the native LDY/CMP immediates rather than using compiled selector constants.
        ushort offset = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa7c042);
        for (int selector = 0; selector < 3; selector++)
        {
            int instruction = 0xa7c029 + 8 * selector;
            if (tilemap != CartridgeImportSource.Require(rom).ReadCartridgeWord((instruction + 4))) continue;
            offset = CartridgeImportSource.Require(rom).ReadCartridgeWord((instruction + 1));
            break;
        }
        return new NativeKraidGrowthResumeResult((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa7c047) + offset), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa70000 | (CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa7c04d) + offset))));
    }

    /// <summary>
    /// Checks the typed growth resumes against the native selector for every sixteen-bit word growth could read: the
    /// closed, opening and wider poses' head images select their resume, and every other word selects the default.
    /// </summary>
    internal static void VerifyKraidGrowthResumeCases(SuperMetroidAddressSpace rom)
    {
        var native = new KraidHeadNativeCursors(CartridgeImportSource.Require(rom));
        var selected = new Dictionary<ushort, KraidHeadGrowthResume>
        {
            [SuperMetroid.Core.Bosses.Kraid.Artwork.KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Closed)] = KraidHeadGrowthResume.NextFrameClosed,
            [SuperMetroid.Core.Bosses.Kraid.Artwork.KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Opening)] = KraidHeadGrowthResume.NextFrameOpening,
            [SuperMetroid.Core.Bosses.Kraid.Artwork.KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Wider)] = KraidHeadGrowthResume.NextFrameWider,
        };
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            KraidHeadGrowthResume resume = selected.GetValueOrDefault((ushort)raw, KraidHeadGrowthResume.NextStepOther);
            (ushort pointer, ushort timer) = NativeKraidGrowthResume(rom, (ushort)raw);
            AssertEqual(native.CursorAt(pointer), KraidHeadAnimations.GrowthResumeCursor(resume),
                "Kraid native word-dependent resume cursor");
            AssertEqual(timer, KraidHeadAnimations.GrowthResumeDelay(resume), "Kraid native word-dependent resume delay");
        }
    }
}

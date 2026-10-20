using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain body, brain, and neck contact lists against native data and guarded runtime collision.</summary>
internal static class MotherBrainContactHitboxesVerification
{
    /// <summary>
    /// Proves that Mother Brain's compiled physical contact rectangles match the pinned
    /// cartridge and that the real collision routine no longer reads their source lists.
    /// </summary>
    internal static void VerifyMotherBrainContactHitboxes()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        MotherBrainContactPart[] parts =
        [
            MotherBrainContactPart.Body,
            MotherBrainContactPart.Brain,
            MotherBrainContactPart.Neck,
        ];

        int comparedWords = 0;
        foreach (MotherBrainContactPart part in parts)
        {
            int address = MotherBrainContactHitboxDefinitions.GetSourceAddress(part);
            MotherBrainContactHitboxes hitboxes =
                MotherBrainContactHitboxDefinitions.Get(part);
            AssertEqual(hitboxes.Count, CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                $"Mother Brain {part} hitbox count");

            for (int index = 0; index < hitboxes.Count; index++)
            {
                int record = address + 2 + index * 8;
                EnemyHitboxRectangle hitbox = hitboxes[index];
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(record)), hitbox.Left,
                    $"Mother Brain {part} hitbox {index} left");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2))), hitbox.Top,
                    $"Mother Brain {part} hitbox {index} top");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4))), hitbox.Right,
                    $"Mother Brain {part} hitbox {index} right");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6))), hitbox.Bottom,
                    $"Mother Brain {part} hitbox {index} bottom");
                comparedWords += 4;
            }
        }

        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0200))};
        var state = new MotherBrainEnemyState(body);
        Func<MotherBrainEnemyState, SamusState, MotherBrainContactPart,
                Position16<RoomPixels>, bool> resolve = MotherBrainBody.ResolveMotherBrainSamusCollisionPart;

        const ushort originX = 0x0200;
        const ushort originY = 0x0200;
        int collisionCases = 0;
        foreach (MotherBrainContactPart part in parts)
        {
            for (int yDelta = -64; yDelta <= 80; yDelta++)
            {
                for (int xDelta = -56; xDelta <= 56; xDelta++)
                {
                    var samus = new SamusState
                    {
                        Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(originX + xDelta)), unchecked((ushort)(originY + yDelta)))),
                        Health = 999,
                    };
                    samus.Kinematics.Radius = new(5, 19);

                    (bool expected, ushort expectedXDisplacement) =
                        ExpectedMotherBrainContact(part, new((short)xDelta, (short)yDelta), new(5, 19));
                    AssertEqual(expected,
                        resolve(state, samus, part, new(originX, originY)),
                        $"Mother Brain {part} contact at ({xDelta},{yDelta})");
                    AssertEqual(expectedXDisplacement, samus.Kinematics.ExtraDisplacement.X.WholePixels,
                        $"Mother Brain {part} X displacement at ({xDelta},{yDelta})");
                    AssertEqual(expected ? (ushort)4 : (ushort)0,
                        samus.Kinematics.ExtraDisplacement.Y.WholePixels,
                        $"Mother Brain {part} Y displacement at ({xDelta},{yDelta})");
                    collisionCases++;
                }
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainContactHitboxDefinitions.Get((MotherBrainContactPart)3),
            "Mother Brain hitbox catalog rejects an unknown component");
        Console.WriteLine(
            $"  Mother Brain contact hitboxes: {comparedWords} native extent words and " +
            $"{collisionCases} production collision probes pass without a ROM bus.");
    }

    /// <summary>Computes expected contact and minimum horizontal shove from the compiled part rectangles.</summary>
    private static (bool Collides, ushort XDisplacement) ExpectedMotherBrainContact(
        MotherBrainContactPart part,
        Offset16<RoomPixels> delta,
        Extent16<RoomPixels> radius)
    {
        foreach (EnemyHitboxRectangle hitbox in MotherBrainContactHitboxDefinitions.Get(part))
        {
            int yExtent = delta.Y >= 0 ? hitbox.Bottom : hitbox.Top;
            if (radius.Y + Math.Abs(yExtent) - Math.Abs((int)delta.Y) < 0)
                continue;

            int xExtent = delta.X >= 0 ? hitbox.Right : hitbox.Left;
            int overlap = radius.X + Math.Abs(xExtent) - Math.Abs((int)delta.X);
            if (overlap >= 0)
                return (true, unchecked((ushort)Math.Max(overlap, 4)));
        }

        return (false, 0);
    }

    /// <summary>Runs the catalog comparison and production collision probes for Mother Brain's body, brain, and neck.</summary>
    [VerificationCommand("--mother-brain-contact-hitboxes")]
    internal static int MotherBrainContactHitboxesCommand()
    {
        Suite(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes());
        return 0;
    }
}

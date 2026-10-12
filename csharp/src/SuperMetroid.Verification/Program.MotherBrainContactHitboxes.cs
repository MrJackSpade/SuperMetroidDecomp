using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class MotherBrainContactHitboxesVerification
{
    /// <summary>
    /// Proves that Mother Brain's compiled physical contact rectangles match the pinned
    /// cartridge and that the real collision routine no longer reads their source lists.
    /// </summary>
    internal static void VerifyMotherBrainContactHitboxes()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
                MotherBrainContactHitbox hitbox = hitboxes[index];
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

        var body = new RoomEnemySlot(0) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0200))};
        var state = new MotherBrainEnemyState(body);
        Func<MotherBrainEnemyState, SamusState, MotherBrainContactPart,
                Position16<RoomPixels>, bool> resolve = MotherBrainBehavior.ResolveMotherBrainSamusCollisionPart;

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

    private static (bool Collides, ushort XDisplacement) ExpectedMotherBrainContact(
        MotherBrainContactPart part,
        Offset16<RoomPixels> delta,
        Extent16<RoomPixels> radius)
    {
        foreach (MotherBrainContactHitbox hitbox in MotherBrainContactHitboxDefinitions.Get(part))
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

    [VerificationCommand("--mother-brain-contact-hitboxes")]
    internal static int MotherBrainContactHitboxesCommand()
    {
        Suite(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes());
        return 0;
    }
}

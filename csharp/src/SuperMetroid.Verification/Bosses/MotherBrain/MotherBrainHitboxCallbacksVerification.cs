using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>
/// ROM oracle for Mother Brain's compiled hitbox rectangles: every frame root's components and every rectangle's
/// edges and typed touch and shot reactions, encoded back through the callback codec, against the cartridge's lists.
/// </summary>
internal static class MotherBrainHitboxCallbacksVerification
{
    /// <summary>Bytes in one native hitbox record: four signed edges and the touch and shot callbacks.</summary>
    private const int HitboxRecordBytes = 12;

    /// <summary>Bytes in one native extended-frame component record.</summary>
    private const int ComponentRecordBytes = 8;

    /// <summary>Compares all seventeen frame roots' components and rectangles with the cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain's compiled hitbox rectangles and callbacks")]
    internal static void VerifyMotherBrainHitboxCallbacks(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int rectangles = 0;
        for (int index = 0; index < MotherBrainBodyVisualDefinitions.FrameCount; index++)
        {
            EnemySpritemapDefinition frame = MotherBrainBodyVisualDefinitions.Frame(index);
            int root = (int)new SnesAddress(frame.Bank, frame.Pointer);
            ReadOnlySpan<MotherBrainCollisionComponent> components = MotherBrainCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual(rom.ReadCartridgeByte(root), components.Length, $"frame {frame.Name} component count");
            for (int component = 0; component < components.Length; component++)
            {
                int record = root + 2 + component * ComponentRecordBytes;
                MotherBrainCollisionComponent compiled = components[component];
                AssertEqual(cartridge.ReadCartridgeWord(record), unchecked((ushort)compiled.Offset.X),
                    $"frame {frame.Name} component {component} X");
                AssertEqual(cartridge.ReadCartridgeWord(record + 2), unchecked((ushort)compiled.Offset.Y),
                    $"frame {frame.Name} component {component} Y");
                int list = (int)new SnesAddress(MotherBrainCollisionDefinitions.Bank, cartridge.ReadCartridgeWord(record + 6));
                AssertEqual(cartridge.ReadCartridgeWord(list), compiled.Hitboxes.Count,
                    $"frame {frame.Name} component {component} rectangle count");
                for (int box = 0; box < compiled.Hitboxes.Count; box++)
                {
                    int entry = list + 2 + box * HitboxRecordBytes;
                    MotherBrainCollisionHitbox hitbox = compiled.Hitboxes[box];
                    string at = $"frame {frame.Name} component {component} rectangle {box}";
                    AssertEqual(cartridge.ReadCartridgeWord(entry), unchecked((ushort)hitbox.Left), $"{at} left");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 2), unchecked((ushort)hitbox.Top), $"{at} top");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 4), unchecked((ushort)hitbox.Right), $"{at} right");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 6), unchecked((ushort)hitbox.Bottom), $"{at} bottom");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 8), MotherBrainHitboxCallbackCodec.ToCallbackWord(hitbox.Touch),
                        $"{at} touch reaction");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 10), MotherBrainHitboxCallbackCodec.ToCallbackWord(hitbox.Shot),
                        $"{at} shot reaction");
                    rectangles++;
                }
            }
        }

        AssertThrows<InvalidOperationException>(() => MotherBrainHitboxCallbackCodec.ToCallbackWord((MotherBrainHitboxTouch)0),
            "an undefined touch reaction has no callback");
        AssertThrows<InvalidOperationException>(() => MotherBrainHitboxCallbackCodec.ToCallbackWord((MotherBrainHitboxShot)0),
            "an undefined shot reaction has no callback");
        Console.WriteLine(
            $"Mother Brain hitbox callbacks: {MotherBrainBodyVisualDefinitions.FrameCount} frame roots and {rectangles} rectangles match the cartridge's edges and callbacks.");
    }

    /// <summary>Runs the Mother Brain hitbox-callback ROM oracle.</summary>
    [VerificationCommand("--mother-brain-hitbox-callbacks")]
    internal static int MotherBrainHitboxCallbacksCommand()
    {
        VerifyMotherBrainHitboxCallbacks(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        return 0;
    }
}

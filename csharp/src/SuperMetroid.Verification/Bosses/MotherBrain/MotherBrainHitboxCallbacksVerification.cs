using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

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

    /// <summary>
    /// Compares all sixteen body frame roots' components and rectangles with the cartridge, and her brain's rectangle with
    /// the root of the blank brain frame her parts' initial list shows (#2220).
    /// </summary>
    internal static void VerifyMotherBrainHitboxCallbacks(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        int rectangles = 0;
        IEnumerable<(string Name, ushort Pointer, EnemyCollisionComponent[] Components)> roots = Enumerable
            .Range(0, MotherBrainBodyFrameDefinitions.FrameCount)
            .Select(MotherBrainBodyVisualDefinitionsConstants.Frame)
            .Select(frame => (frame.Name, frame.Pointer, MotherBrainCollision.ComponentsAt(frame.Pointer).ToArray()))
            .Append(("her brain's blank frame", (ushort)0xa320, MotherBrainCollisionDefinitions.BrainComponents));
        foreach ((string name, ushort pointer, EnemyCollisionComponent[] components) in roots)
        {
            int root = (int)new SnesAddress(MotherBrainCollision.Bank, pointer);
            // #2211: the port lists only the native components that hold rectangles, in their order; the collision tests
            // walk the rectangles, so a native component without one is never tested.
            int listed = 0;
            for (int component = 0; component < rom.ReadCartridgeByte(root); component++)
            {
                int record = root + 2 + component * ComponentRecordBytes;
                int list = (int)new SnesAddress(MotherBrainCollision.Bank, cartridge.ReadCartridgeWord(record + 6));
                if (cartridge.ReadCartridgeWord(list) == 0)
                    continue;
                AssertTrue(listed < components.Length, $"frame {name} lists native component {component}");
                EnemyCollisionComponent compiled = components[listed++];
                AssertEqual(cartridge.ReadCartridgeWord(record), unchecked((ushort)compiled.Offset.X),
                    $"frame {name} component {component} X");
                AssertEqual(cartridge.ReadCartridgeWord(record + 2), unchecked((ushort)compiled.Offset.Y),
                    $"frame {name} component {component} Y");
                AssertEqual(cartridge.ReadCartridgeWord(list), compiled.Hitboxes.Count,
                    $"frame {name} component {component} rectangle count");
                for (int box = 0; box < compiled.Hitboxes.Count; box++)
                {
                    int entry = list + 2 + box * HitboxRecordBytes;
                    EnemyCollisionHitbox hitbox = compiled.Hitboxes[box];
                    string at = $"frame {name} component {component} rectangle {box}";
                    AssertEqual(cartridge.ReadCartridgeWord(entry), unchecked((ushort)hitbox.Left), $"{at} left");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 2), unchecked((ushort)hitbox.Top), $"{at} top");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 4), unchecked((ushort)hitbox.Right), $"{at} right");
                    AssertEqual(cartridge.ReadCartridgeWord(entry + 6), unchecked((ushort)hitbox.Bottom), $"{at} bottom");
                    AssertEqual(EnemyReactionAccess.Of(0xa9, cartridge.ReadCartridgeWord(entry + 8), shot: false), (EnemyReaction?)hitbox.Touch,
                        $"{at} touch reaction");
                    AssertEqual(EnemyReactionAccess.Of(0xa9, cartridge.ReadCartridgeWord(entry + 10), shot: true), (EnemyReaction?)hitbox.Shot,
                        $"{at} shot reaction");
                    rectangles++;
                }
            }
            AssertEqual(components.Length, listed, $"frame {name} lists only the native components that hold rectangles");
        }

        Console.WriteLine(
            $"Mother Brain hitbox callbacks: {MotherBrainBodyFrameDefinitions.FrameCount} body frame roots, her brain's and {rectangles} rectangles match the cartridge's edges and callbacks.");
    }

    /// <summary>Runs the Mother Brain hitbox-callback ROM oracle.</summary>
    [VerificationCommand("--mother-brain-hitbox-callbacks")]
    internal static int MotherBrainHitboxCallbacksCommand()
    {
        VerifyMotherBrainHitboxCallbacks(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        return 0;
    }
}

using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Independent bounded decode of the already inventoried native visual and collision records.</summary>
    private static (EnemyExtendedDrawComponent[] Oam, EnemyBg2TilemapWrite[] Bg2) ReadBossDisplayOracle(
        CartridgeImportAddressSpace source, EnemyExtendedFrameDefinition frame, HashSet<int> checkedLists)
    {
        ushort Word(int address) => (ushort)(source.ReadCartridgeByte(address) | source.ReadCartridgeByte(address + 1) << 8);
        int Address(ushort pointer) => frame.Bank << 16 | pointer;
        int root = Address(frame.Pointer), count = source.ReadCartridgeByte(root);
        var oam = new List<EnemyExtendedDrawComponent>();
        var bg2 = new List<EnemyBg2TilemapWrite>();
        var physical = BossPhysicalComponents(frame);
        bool emptyPhysics = frame.Bank == DraygonBg2FrameDefinitions.Bank && DraygonCollisionDefinitions.IsEmptyOamFrame(frame.Pointer);
        // Compiled Draygon OAM collision deliberately omits components whose
        // native lists are empty. Confirm each omitted native list below.
        if (!emptyPhysics) AssertEqual(physical.Length, count, frame.Name + " native physical component count");
        for (int component = 0; component < count; component++)
        {
            int record = root + 2 + component * 8;
            short x = unchecked((short)Word(record)), y = unchecked((short)Word(record + 2));
            ushort visual = Word(record + 4), hitbox = Word(record + 6);
            if (!emptyPhysics) AssertEqual(physical[component], (x, y, hitbox), frame.Name + " immutable physical component");
            else AssertEqual(0, (int)Word(Address(hitbox)), "omitted Draygon component has no native collision records");
            if (checkedLists.Add(Address(hitbox)))
            {
                ushort[][] boxes = BossPhysicalHitboxes(frame.Bank, hitbox);
                AssertEqual(boxes.Length, (int)Word(Address(hitbox)), frame.Name + " native hitbox list count");
                for (int index = 0; index < boxes.Length; index++)
                for (int field = 0; field < boxes[index].Length; field++)
                    AssertEqual(Word(Address(hitbox) + 2 + index * 12 + field * 2), boxes[index][field],
                        frame.Name + " native rectangle/touch/shot record");
            }
            if (Word(Address(visual)) == EnemyBg2FrameLayout.StreamMarker)
            {
                int cursor = Address(unchecked((ushort)(visual + 2)));
                bool ended = false;
                for (int command = 0; command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                {
                    ushort destination = Word(cursor);
                    if (destination == ushort.MaxValue) { ended = true; break; }
                    int length = Word(cursor + 2);
                    AssertTrue(length is > 0 and <= EnemyBg2FrameLayout.TilemapWidth, "native oracle BG2 run is bounded");
                    var words = new ushort[length];
                    for (int index = 0; index < length; index++) words[index] = Word(cursor + 4 + index * 2);
                    bg2.Add(new EnemyBg2TilemapWrite(checked((ushort)((destination - EnemyBg2FrameLayout.WorkingRamBase) / 2)), words));
                    cursor += 4 + length * 2;
                }
                AssertTrue(ended, "native oracle BG2 stream terminates within its declared bound");
                continue;
            }
            int sprites = Word(Address(visual));
            AssertTrue(sprites <= EnemySpritemapDefinitions.MaximumParts, "native oracle OAM count is bounded");
            var parts = new EnemySpritemapPart[sprites];
            for (int index = 0; index < sprites; index++)
            {
                int address = Address(visual) + 2 + index * 5;
                var nativeX = new SnesSpritemapXWord(Word(address));
                // Native unused X bits are not presentation; compare the canonical
                // signed offset and size that actually reach packed OAM.
                parts[index] = new EnemySpritemapPart(SnesSpritemapXWord.Create(nativeX.SignedOffset, nativeX.IsLarge),
                    source.ReadCartridgeByte(address + 2), new SnesObjAttributeWord(Word(address + 3)));
            }
            oam.Add(new EnemyExtendedDrawComponent(x, y, EnemySpritemapParts.FromOwnedArray(parts)));
        }
        return (oam.ToArray(), bg2.ToArray());
    }

    /// <summary>Returns independently compiled physical component positions and native hitbox pointers for a boss frame.</summary>
    /// <param name="frame">The decoded visual frame whose bank and pointer select the collision definition.</param>
    /// <returns>Each component's signed offset and native hitbox pointer in frame order.</returns>
    private static (short X, short Y, ushort Pointer)[] BossPhysicalComponents(EnemyExtendedFrameDefinition frame) => frame.Bank switch
    {
        CrocomireBg2FrameDefinitions.Bank => CrocomireBodyCollisionDefinitions.ComponentsAt(frame.Pointer).ToArray()
            .Select(part => (part.X, part.Y, part.HitboxPointer)).ToArray(),
        PhantoonBg2FrameDefinitions.Bank => PhantoonCollisionDefinitions.ComponentsAt(frame.Pointer).ToArray()
            .Select(part => (part.X, part.Y, part.HitboxPointer)).ToArray(),
        DraygonBg2FrameDefinitions.Bank => DraygonCollisionDefinitions.ComponentsAt(frame.Pointer).ToArray()
            .Select(part => (part.X, part.Y, part.HitboxPointer)).ToArray(),
        _ => throw new InvalidDataException("Unknown native oracle family."),
    };

    /// <summary>Reads a boss component's collision rectangles as their six native 16-bit record fields.</summary>
    /// <param name="bank">The enemy-data bank that identifies the boss collision catalog.</param>
    /// <param name="pointer">The native pointer to the component's hitbox list.</param>
    /// <returns>One six-word record per rectangle, preserving native list order.</returns>
    private static ushort[][] BossPhysicalHitboxes(byte bank, ushort pointer) => bank switch
    {
        CrocomireBg2FrameDefinitions.Bank => CrocomireBodyCollisionDefinitions.HitboxesAt(pointer).ToArray()
            .Select(box => Words(box.Left, box.Top, box.Right, box.Bottom, box.TouchAi, box.ShotAi)).ToArray(),
        PhantoonBg2FrameDefinitions.Bank => PhantoonCollisionDefinitions.HitboxesAt(pointer).ToArray()
            .Select(box => Words(box.Left, box.Top, box.Right, box.Bottom, box.TouchAi, box.ShotAi)).ToArray(),
        DraygonBg2FrameDefinitions.Bank => DraygonCollisionDefinitions.HitboxesAt(pointer).ToArray()
            .Select(box => Words(box.Left, box.Top, box.Right, box.Bottom, box.TouchAi, box.ShotAi)).ToArray(),
        _ => throw new InvalidDataException("Unknown native oracle bank."),
    };

    /// <summary>Encodes a collision rectangle and its touch/shot AI selectors in native word order.</summary>
    /// <param name="left">Signed left edge of the rectangle.</param>
    /// <param name="top">Signed top edge of the rectangle.</param>
    /// <param name="right">Signed right edge of the rectangle.</param>
    /// <param name="bottom">Signed bottom edge of the rectangle.</param>
    /// <param name="touch">Touch-collision AI selector.</param>
    /// <param name="shot">Projectile-collision AI selector.</param>
    /// <returns>The six 16-bit fields as stored in the native collision record.</returns>
    private static ushort[] Words(short left, short top, short right, short bottom, ushort touch, ushort shot) =>
        [unchecked((ushort)left), unchecked((ushort)top), unchecked((ushort)right), unchecked((ushort)bottom), touch, shot];
}

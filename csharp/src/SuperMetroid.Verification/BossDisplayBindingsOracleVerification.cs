using SuperMetroid.Core;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Decodes native boss display records while cross-checking compiled collision geometry.</summary>
internal static class BossDisplayBindingsOracleVerification
{
    /// <summary>Independent bounded decode of the already inventoried native visual and collision records.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for native boss extended OAM and BG2 display records")]
    internal static (EnemyExtendedDrawComponent[] Oam, EnemyBg2TilemapWrite[] Bg2) ReadBossDisplayOracle(
        CartridgeImportAddressSpace source, EnemyExtendedFrameDefinition frame, HashSet<int> checkedLists)
    {
        int Address(ushort pointer) => ((int)new SnesAddress(frame.Bank, pointer));
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
            short x = unchecked((short)source.ReadCartridgeWord(record)), y = unchecked((short)source.ReadCartridgeWord((record + 2)));
            ushort visual = source.ReadCartridgeWord((record + 4)), hitbox = source.ReadCartridgeWord((record + 6));
            if (!emptyPhysics) AssertEqual(physical[component], (x, y, hitbox), frame.Name + " immutable physical component");
            else AssertEqual(0, (int)source.ReadCartridgeWord(Address(hitbox)), "omitted Draygon component has no native collision records");
            if (checkedLists.Add(Address(hitbox)))
            {
                ushort[][] boxes = BossPhysicalHitboxes(frame.Bank, hitbox);
                AssertEqual(boxes.Length, (int)source.ReadCartridgeWord(Address(hitbox)), frame.Name + " native hitbox list count");
                for (int index = 0; index < boxes.Length; index++)
                for (int field = 0; field < boxes[index].Length; field++)
                    AssertEqual(source.ReadCartridgeWord((Address(hitbox) + 2 + index * 12 + field * 2)), boxes[index][field],
                        frame.Name + " native rectangle/touch/shot record");
            }
            if (source.ReadCartridgeWord(Address(visual)) == EnemyBg2FrameLayout.StreamMarker)
            {
                int cursor = Address(unchecked((ushort)(visual + 2)));
                bool ended = false;
                for (int command = 0; command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                {
                    ushort destination = source.ReadCartridgeWord(cursor);
                    if (destination == ushort.MaxValue) { ended = true; break; }
                    int length = source.ReadCartridgeWord((cursor + 2));
                    AssertTrue(length is > 0 and <= EnemyBg2FrameLayout.TilemapWidth, "native oracle BG2 run is bounded");
                    var words = new ushort[length];
                    for (int index = 0; index < length; index++) words[index] = source.ReadCartridgeWord((cursor + 4 + index * 2));
                    bg2.Add(new EnemyBg2TilemapWrite(checked((ushort)((destination - EnemyBg2FrameLayout.WorkingRamBase) / 2)), words));
                    cursor += 4 + length * 2;
                }
                AssertTrue(ended, "native oracle BG2 stream terminates within its declared bound");
                continue;
            }
            int sprites = source.ReadCartridgeWord(Address(visual));
            AssertTrue(sprites <= EnemySpritemapDefinitions.MaximumParts, "native oracle OAM count is bounded");
            var parts = new EnemySpritemapPart[sprites];
            for (int index = 0; index < sprites; index++)
            {
                int address = Address(visual) + 2 + index * 5;
                var nativeX = new SnesSpritemapXWord(source.ReadCartridgeWord(address));
                // Native unused X bits are not presentation; compare the canonical
                // signed offset and size that actually reach packed OAM.
                parts[index] = new EnemySpritemapPart(SnesSpritemapXWord.Create(nativeX.SignedOffset, nativeX.IsLarge),
                    source.ReadCartridgeByte(address + 2), new SnesObjAttributeWord(source.ReadCartridgeWord((address + 3))));
            }
            oam.Add(new EnemyExtendedDrawComponent(new(x, y), EnemySpritemapParts.FromOwnedArray(parts)));
        }
        return (oam.ToArray(), bg2.ToArray());
    }

    /// <summary>Selects compiled component offsets and hitbox pointers for the frame's boss family.</summary>
    private static (short X, short Y, ushort Pointer)[] BossPhysicalComponents(EnemyExtendedFrameDefinition frame) => frame.Bank switch
    {
        CrocomireBg2FrameDefinitions.Bank => CrocomireBodyCollisionDefinitions.ComponentsAt(frame.Pointer)
            .SelectToArray(part => (part.Offset.X, part.Offset.Y, part.HitboxPointer)),
        PhantoonBg2FrameDefinitions.Bank => PhantoonCollisionDefinitions.ComponentsAt(frame.Pointer)
            .Select(part => (part.Offset.X, part.Offset.Y, part.HitboxPointer)).ToArray(),
        DraygonBg2FrameDefinitions.Bank => DraygonCollisionDefinitions.ComponentsAt(frame.Pointer)
            .Select(part => (part.Offset.X, part.Offset.Y, (ushort)part.HitboxPointer)).ToArray(),
        _ => throw new InvalidDataException("Unknown native oracle family."),
    };

    /// <summary>Selects compiled hitbox rectangles and callbacks for a native boss list.</summary>
    private static ushort[][] BossPhysicalHitboxes(byte bank, ushort pointer) => bank switch
    {
        CrocomireBg2FrameDefinitions.Bank => CrocomireBodyCollisionDefinitions.HitboxesAt(pointer)
            .SelectToArray(box => Words(box.Left, box.Top, box.Right, box.Bottom, box.TouchAi, box.ShotAi)),
        PhantoonBg2FrameDefinitions.Bank => PhantoonCollisionDefinitions.HitboxesAt(pointer)
            .Select(box => Words(box.Left, box.Top, box.Right, box.Bottom, box.TouchAi, box.ShotAi)).ToArray(),
        DraygonBg2FrameDefinitions.Bank => DraygonCollisionDefinitions.HitboxesAt(
                ClosedNativeWords.Decode<DraygonHitboxList>(pointer, "compiled Draygon hitbox list"))
            .SelectToArray(box => Words(box.Left, box.Top, box.Right, box.Bottom, box.TouchAi, box.ShotAi)),
        _ => throw new InvalidDataException("Unknown native oracle bank."),
    };

    /// <summary>Arranges one hitbox's bounds and callbacks in cartridge record order.</summary>
    private static ushort[] Words(short left, short top, short right, short bottom, ushort touch, ushort shot) =>
        [unchecked((ushort)left), unchecked((ushort)top), unchecked((ushort)right), unchecked((ushort)bottom), touch, shot];
}

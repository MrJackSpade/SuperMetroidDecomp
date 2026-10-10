using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledDraygonOamCollision(
        SuperMetroidAddressSpace rom)
    {
        // Version 6 grouped all 60 selected bank-$A5 OAM roots under a
        // Draygon key. The final twelve are selected by Spore Spawn's program;
        // never broaden Draygon's empty-list shortcut to a different enemy.
        var sporePointers = new HashSet<ushort>();
        for (int index = 0;
             index < SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = SporeSpawnInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            sporePointers.Add(ReadWord(
                (DraygonBg2FrameDefinitions.Bank << 16) | address));
        }

        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new FrontendCartridgeReadGuard(rom));
        var walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.DraygonBody;
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = DraygonBg2FrameDefinitions.Bank,
        };
        slot.XPosition = 0x100;
        slot.YPosition = 0x100;

        int draygonFrames = 0;
        int sporeFrames = 0;
        int componentsChecked = 0;
        foreach (EnemyExtendedFrameDefinition frame in
                 EnemyExtendedFrameDefinitions.Frames.ToArray().Take(
                     EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount))
        {
            if (frame.Bank != DraygonBg2FrameDefinitions.Bank)
                continue;
            if (DraygonBg2FrameDefinitions.IsFrame(frame.Pointer))
                continue;
            if (sporePointers.Contains(frame.Pointer))
            {
                AssertTrue(frame.Name.StartsWith("spore_spawn_oam_",
                        StringComparison.Ordinal),
                    $"Spore Spawn frame {frame.Pointer:X4} has its own author key");
                AssertTrue(!DraygonCollisionDefinitions.IsEmptyOamFrame(frame.Pointer),
                    $"Spore Spawn frame {frame.Name} is not a Draygon collision frame");
                sporeFrames++;
                continue;
            }

            AssertTrue(DraygonCollisionDefinitions.IsEmptyOamFrame(frame.Pointer),
                $"Draygon OAM frame {frame.Name} has a compiled no-hitbox identity");
            AssertTrue(frame.Name.StartsWith("draygon_oam_",
                    StringComparison.Ordinal),
                $"Draygon frame {frame.Pointer:X4} retains its author family");
            AssertEqual(0,
                DraygonCollisionDefinitions.ComponentsAt(frame.Pointer).Length,
                $"Draygon OAM frame {frame.Name} has no collision components");
            int root = (frame.Bank << 16) | frame.Pointer;
            int componentCount = rom.ReadByte(root);
            AssertTrue(componentCount is > 0 and <= 8,
                $"Draygon OAM frame {frame.Name} has bounded components");
            for (int component = 0; component < componentCount; component++)
            {
                ushort listPointer = ReadWord(root + 2 + component * 8 + 6);
                AssertTrue((DraygonHitboxList)listPointer is
                    DraygonHitboxList.Empty or
                    DraygonHitboxList.OtherEmpty,
                    $"Draygon OAM frame {frame.Name} component {component} has a native empty list");
                AssertEqual((ushort)0,
                    ReadWord((frame.Bank << 16) | listPointer),
                    $"Draygon OAM frame {frame.Name} component {component} native hitbox count");
                componentsChecked++;
            }

            slot.SpritemapPointer = frame.Pointer;
            object?[] arguments =
                [slot, (ushort)0x100, (ushort)0x100, (ushort)0,
                    (ushort)0, true, (ushort)0];
            AssertTrue(!(bool)walker.Invoke(enemies, arguments)!,
                $"Draygon OAM frame {frame.Name} has no shot callback without ROM reads");
            arguments[5] = false;
            AssertTrue(!(bool)walker.Invoke(enemies, arguments)!,
                $"Draygon OAM frame {frame.Name} has no touch callback without ROM reads");
            draygonFrames++;
        }

        AssertEqual(48, draygonFrames,
            "all selected Draygon OAM frames have empty native hitbox lists");
        AssertEqual(12, sporeFrames,
            "remaining bank-$A5 OAM frames are selected by Spore Spawn");
        AssertEqual(DraygonCollisionDefinitions.EmptyOamFrameCount,
            draygonFrames, "compiled Draygon empty-frame set is exact");
        AssertEqual(0,
            DraygonCollisionDefinitions.HitboxesAt(
                DraygonHitboxList.OtherEmpty).Length,
            "mirrored Draygon OAM list has no compiled hitboxes");
        AssertThrows<InvalidDataException>(
            () => DraygonCollisionDefinitions.ComponentsAt(0xee65),
            "Spore Spawn frame cannot enter Draygon collision data");
        Console.WriteLine($"  Draygon OAM collision: {draygonFrames} frames and {componentsChecked} native zero-hitbox components return no shot/touch callback without ROM reads; 12 Spore Spawn frames excluded.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }
}

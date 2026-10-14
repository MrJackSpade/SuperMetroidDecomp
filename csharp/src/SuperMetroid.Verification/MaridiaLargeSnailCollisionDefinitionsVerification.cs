using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ExtendedCollisionReferenceVerification;

/// <summary>Checks Oum's extracted display selectors and compiled collision records against ROM and the installed runtime.</summary>
internal static class MaridiaLargeSnailCollisionDefinitionsVerification
{
    /// <summary>Matches each native Oum spritemap operand to an installed editable frame without cartridge reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Oum visual selectors")]
    internal static void VerifyInstalledOumVisualSelectors(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var denied = NewOumNoReadBus();
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        enemies.BindFixtureBus(denied);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Oum;
        slot.Definition = default(RoomEnemyDefinition) with
        { Bank = MaridiaLargeSnailCollisionDefinitions.Bank };
        var seen = new HashSet<ushort>();
        for (int index = 0;
             index < MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = MaridiaLargeSnailInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa20000 | operand);
            ushort selected = (ushort)EnemyInstructionInterpreter.ReadEnemyVisualSelector(slot, operand);
            AssertEqual(native, selected,
                $"Oum visual selector $A2:{operand:X4} matches native");
            AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGetDisplay(
                    MaridiaLargeSnailCollisionDefinitions.Bank, selected, out _),
                $"Oum visual selector $A2:{operand:X4} has installed art");
            seen.Add(selected);
        }
        AssertTrue(seen.SetEquals(MaridiaLargeSnailCollisionDefinitions.FramePointers.ToArray()),
            "Oum's 60 operands select exactly the 30 extracted physical frames");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Oum selector path makes no ROM reads");
        Console.WriteLine("Installed Oum: 60 native visual selectors cover 30 editable " +
            "frames with no ROM reads.");
    }

    /// <summary>Compares Oum frame pointers and hitboxes with ROM, then checks touch and shot overlap at rectangle boundaries.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Maridia large snail collision definitions")]
    internal static void VerifyMaridiaLargeSnailCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewOumNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var compiled = new RoomEnemySystem();
        compiled.BindFixtureBus(denied);
        var nativeSlot = new RoomEnemySlot(0);
        RoomEnemySlot compiledSlot = compiled.SlotTable.Slots[0];
        nativeSlot.Definition = compiledSlot.Definition =
            default(RoomEnemyDefinition) with
            { Bank = MaridiaLargeSnailCollisionDefinitions.Bank };
        nativeSlot.EnemyDefinitionPointer = (EnemyDefinitionId)0xffff;
        compiledSlot.EnemyDefinitionPointer = EnemyDefinitionId.Oum;
        AssertEqual(30, MaridiaLargeSnailCollisionDefinitions.FramePointers.Length,
            "all Oum extended frames have compiled collision");
        AssertEqual(30, MaridiaLargeSnailCollisionDefinitions.HitboxPointers.Count(),
            "all Oum hitbox lists have compiled collision");

        var seenLists = new HashSet<ushort>();
        int boxesChecked = 0;
        int probes = 0;
        foreach (ushort frame in MaridiaLargeSnailCollisionDefinitions.FramePointers)
        {
            AssertEqual((ushort)1, rom.ReadCartridgeWord((0xa20000 | frame)),
                $"Oum $A2:{frame:X4} native one-component header");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa20000 | frame + 2)),
                "Oum native component X is zero");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa20000 | frame + 4)),
                "Oum native component Y is zero");
            ushort list = MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame);
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | frame + 8)), list,
                "Oum native frame hitbox-list pointer");
            seenLists.Add(list);
            ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> boxes =
                MaridiaLargeSnailCollisionDefinitions.HitboxesAt(list);
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | list)), (ushort)boxes.Length,
                $"Oum $A2:{list:X4} native hitbox count");
            for (int index = 0; index < boxes.Length; index++)
            {
                MaridiaLargeSnailCollisionHitbox box = boxes[index];
                int record = 0xa20000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(record)), box.Left, "Oum left");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 2))), box.Top, "Oum top");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 4))), box.Right, "Oum right");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 6))), box.Bottom, "Oum bottom");
                AssertEqual(rom.ReadCartridgeWord((record + 8)), box.TouchAi, "Oum touch callback");
                AssertEqual(rom.ReadCartridgeWord((record + 10)), box.ShotAi, "Oum shot callback");
                boxesChecked++;
            }

            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                compiledSlot.Position = compiledSlot.Position.WithWholeX(originX);
                nativeSlot.Position = nativeSlot.Position.WithWholeX(originX);
                compiledSlot.Position = compiledSlot.Position.WithWholeY(originY);
                nativeSlot.Position = nativeSlot.Position.WithWholeY(originY);
                foreach (MaridiaLargeSnailCollisionHitbox box in boxes)
                {
                    ushort[] xs = BoundaryPoints(originX, box.Left, box.Right);
                    ushort[] ys = BoundaryPoints(originY, box.Top, box.Bottom);
                    foreach (ushort x in xs)
                    foreach (ushort y in ys)
                    for (int shot = 0; shot <= 1; shot++)
                    {
                        object?[] nativeArguments =
                            [nativeSlot, new Position16<RoomPixels>(x, y), Extent16<RoomPixels>.Zero, shot != 0, (ushort)0];
                        object?[] compiledArguments =
                            [compiledSlot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>((ushort)0, (ushort)0), shot != 0, (ushort)0];
                        bool nativeHit = ReferenceExtendedCollision(rom, nativeArguments);
                        bool compiledHit = ExtendedHitboxWalker.Find(compiled, compiledArguments);
                        AssertEqual(nativeHit, compiledHit,
                            $"Oum $A2:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                        AssertEqual((ushort)(nativeArguments[^1] ?? throw new InvalidOperationException("Verification requires nativeArguments[^1].")),
                            (ushort)(compiledArguments[^1] ?? throw new InvalidOperationException("Verification requires compiledArguments[^1].")),
                            $"Oum $A2:{frame:X4} callback {x:X4},{y:X4}, shot={shot}");
                        probes++;
                    }
                }
            }
        }
        AssertTrue(seenLists.SetEquals(MaridiaLargeSnailCollisionDefinitions.HitboxPointers),
            "every compiled Oum list belongs to a selected frame");
        AssertEqual(66, boxesChecked, "all Oum physical rectangles were verified");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailCollisionDefinitions.HitboxListAt(0x8000),
            "unknown Oum frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Oum hitbox list fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Oum touch and shot walkers never read ROM bytes");
        Console.WriteLine($"Oum collision: 30 frames, 66 rectangles, {probes} " +
            "native-equivalent touch/shot boundary probes with no ROM reads.");

        static ushort[] BoundaryPoints(ushort origin, short low, short high) =>
        [
            unchecked((ushort)(origin + low - 1)),
            unchecked((ushort)(origin + low)),
            unchecked((ushort)(origin + low + 1)),
            unchecked((ushort)(origin + high - 1)),
            unchecked((ushort)(origin + high)),
            unchecked((ushort)(origin + high + 1)),
        ];
    }

    /// <summary>Provides an unbacked bus that rejects any Oum visual or collision ROM access.</summary>
    private static CartridgeReadGuard NewOumNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Oum frame or collision read ROM byte ${address:X6}.")
            .RejectWrites(address => $"Installed Oum frame or collision wrote ROM byte ${address:X6}.");

    /// <summary>Runs the installed Oum collision verification through the command dispatcher.</summary>
    [VerificationCommand("--verify-oum-collision")]
    internal static int VerifyOumCollisionCommand()
    {
        Suite(nameof(VerifyMaridiaLargeSnailCollisionDefinitions), () => VerifyMaridiaLargeSnailCollisionDefinitions());
        return 0;
    }
}

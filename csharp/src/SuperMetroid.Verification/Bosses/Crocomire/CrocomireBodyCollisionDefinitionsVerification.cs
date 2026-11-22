using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireTongueCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Crocomire's compiled body collision records and production hitbox walker against the cartridge.</summary>
internal static class CrocomireBodyCollisionDefinitionsVerification
{
    /// <summary>
    /// Compare every compiled physical record with the pinned cartridge, then
    /// exercise the actual enemy collision walker against a bus that forbids
    /// cartridge reads. The whole-room census separately checks the fight's
    /// first thirty live frames with pixel and state parity.
    /// </summary>
    internal static void VerifyCrocomireBodyCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewCrocomireTongueNoReadBus();
        var compiled = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The native side uses the independent test-side ROM walker, which reads the bound Crocomire's frame word in its
        // own bank (#2327); the installed side selects Crocomire's compiled physical data. Both read the one place.
        // Crocomire answers for his own frames' hitboxes, as his room's population places him (#2554).
        RoomEnemyPlace place = CrocomirePlacement.PlaceCrocomire(compiled, new(0x1000, 0x1000)).Body;

        var frames = CrocomireBodyVisualDefinitionsConstants.Frames;
        AssertEqual(50, frames.Count,
            "all fifty selected Crocomire fight-body roots have collision");
        // The cartridge names each component's hitbox list by address; only this oracle knows those addresses.
        // Each compiled component must hold the list the ROM names: the same rectangles and callbacks, and the same
        // list instance as every other component naming that address.
        var seenLists = new Dictionary<ushort, IReadOnlyList<EnemyCollisionHitbox>>();
        int probes = 0;
        foreach (ushort frame in frames)
        {
            AssertTrue(CrocomireBodyCollision.HasFrame(frame),
                $"Crocomire body $A4:{frame:X4} has compiled collision");
            ReadOnlySpan<EnemyCollisionComponent> components =
                CrocomireBodyCollision.ComponentsAt(frame);
            // The compiled frame keeps each native component with hitboxes, once at its place (#1937).
            int?[] kept = CrocomireKeptComponents.Of(rom.ReadCartridgeWord, 0xa40000 | frame);
            AssertEqual(kept.Count(index => index is not null), components.Length,
                $"Crocomire body $A4:{frame:X4} component count");
            for (int native = 0; native < kept.Length; native++)
            {
                if (kept[native] is not int index)
                    continue;
                int address = 0xa40000 | unchecked((ushort)(frame + 2 + native * 8));
                EnemyCollisionComponent component = components[index];
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(address)), component.Offset.X,
                    $"Crocomire $A4:{frame:X4} component {index} X");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 2))), component.Offset.Y,
                    $"Crocomire $A4:{frame:X4} component {index} Y");
                ushort list = rom.ReadCartridgeWord((address + 6));
                AssertNativeHitboxList(rom, list, component.Hitboxes, $"Crocomire $A4:{frame:X4} component {index}");
                if (seenLists.TryGetValue(list, out IReadOnlyList<EnemyCollisionHitbox>? shared))
                    AssertTrue(ReferenceEquals(shared, component.Hitboxes),
                        $"Crocomire $A4:{frame:X4} component {index} shares the list every component naming $A4:{list:X4} holds");
                else
                    seenLists.Add(list, component.Hitboxes);
            }

            place.Frame = RoomEnemyPlaceFrameAccess.FrameAt(CrocomireBodyCollision.Bank, frame);
            foreach (short dx in new short[] { -112, -96, -64, -40, -16, 0, 16, 40, 64 })
            foreach (short dy in new short[] { -96, -64, -40, -16, 0, 16, 40, 64 })
            foreach (bool shot in new[] { false, true })
            {
                ushort x = unchecked((ushort)(0x1000 + dx));
                ushort y = unchecked((ushort)(0x1000 + dy));
                object?[] nativeArguments =
                    [place, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(8, 8), shot, (ushort)0];
                object?[] compiledArguments =
                    [place, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(8, 8), shot, (ushort)0];
                bool nativeHit = ReferenceExtendedCollision(rom, CrocomireBodyCollision.Bank, nativeArguments);
                bool compiledHit = ExtendedHitboxWalker.Find(compiledArguments);
                AssertEqual(nativeHit, compiledHit,
                    $"Crocomire $A4:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                AssertEqual(ExtendedHitboxWalker.NativeReaction(CrocomireBodyCollision.Bank, nativeArguments),
                    ExtendedHitboxWalker.Reaction(compiledArguments),
                    $"Crocomire $A4:{frame:X4} selected reaction");
                probes++;
            }
        }

        AssertEqual(13, seenLists.Count,
            "fifty Crocomire body frames refer to thirteen hitbox lists with hitboxes");
        // A frame is always an installed enemy frame (#2554); one that is none of Crocomire's body frames fails loudly.
        AssertThrows<InvalidDataException>(
            () => CrocomireBodyCollision.ComponentsAt(EnemyFrameId.MotherBrainBrainHitArea),
            "a frame that is no Crocomire body frame fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Crocomire body collision never reads ROM bytes");
        Console.WriteLine($"Crocomire body collision: {frames.Count} frames, " +
            $"{seenLists.Count} hitbox lists, {probes} native-equivalent probes; " +
            "installed path reads no ROM.");
    }

    /// <summary>
    /// Compares a compiled component's rectangles and callbacks with the bank-$A4 hitbox list the cartridge names at
    /// <paramref name="list"/>: its count word, then each twelve-byte rectangle and callback record.
    /// </summary>
    internal static void AssertNativeHitboxList(IImportCartridgeSource rom, ushort list,
        IReadOnlyList<EnemyCollisionHitbox> hitboxes, string at)
    {
        AssertEqual(rom.ReadCartridgeWord((0xa40000 | list)), checked((ushort)hitboxes.Count),
            $"{at}: $A4:{list:X4} hitbox count");
        for (int index = 0; index < hitboxes.Count; index++)
        {
            int address = 0xa40000 | unchecked((ushort)(list + 2 + index * 12));
            EnemyCollisionHitbox hitbox = hitboxes[index];
            AssertEqual(unchecked((short)rom.ReadCartridgeWord(address)), hitbox.Left,
                $"{at}: $A4:{list:X4} hitbox {index} left");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 2))), hitbox.Top,
                $"{at}: $A4:{list:X4} hitbox {index} top");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 4))), hitbox.Right,
                $"{at}: $A4:{list:X4} hitbox {index} right");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 6))), hitbox.Bottom,
                $"{at}: $A4:{list:X4} hitbox {index} bottom");
            AssertEqual(EnemyReactionAccess.Of(0xa4, rom.ReadCartridgeWord((address + 8)), shot: false), (EnemyReaction?)hitbox.Touch,
                $"{at}: $A4:{list:X4} hitbox {index} touch callback");
            AssertEqual(EnemyReactionAccess.Of(0xa4, rom.ReadCartridgeWord((address + 10)), shot: true), (EnemyReaction?)hitbox.Shot,
                $"{at}: $A4:{list:X4} hitbox {index} shot callback");
        }
    }

    /// <summary>Runs the focused Crocomire body collision comparison against the retail ROM.</summary>
    [VerificationCommand("--verify-crocomire-body-collision")]
    internal static int VerifyCrocomireBodyCollisionCommand()
    {
        Suite(nameof(VerifyCrocomireBodyCollisionDefinitions), () => VerifyCrocomireBodyCollisionDefinitions());
        return 0;
    }
}

using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Oum's extracted display selectors and compiled collision records against ROM and the installed runtime.</summary>
internal static class OumCollisionDefinitionsVerification
{
    /// <summary>Confirms every pose Oum's programs show is one of the extracted frames and has installed editable art.</summary>
    internal static void VerifyInstalledOumVisualSelectors(EnemyTileSourceArtwork stock)
    {
        var seen = OumAnimationsConstants.Spritemaps(Enum.GetValues<OumSegment>()).ToHashSet();
        foreach (ushort pose in seen)
            AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGetDisplay(
                    OumCollisionDefinitions.Bank, pose, out _),
                $"Oum pose ${pose:X4} has installed art");
        AssertTrue(seen.SetEquals(OumCollisionDefinitionsConstants.FramePointers.ToArray()),
            "Oum's programs show exactly the 30 extracted physical frames");
        Console.WriteLine("Installed Oum: the 30 poses its programs show are the editable extracted frames.");
    }

    /// <summary>Compares Oum frame pointers and hitboxes with ROM, then checks touch and shot overlap at rectangle boundaries.</summary>
    internal static void VerifyOumCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewOumNoReadBus();
        var compiled = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace nativePlace = compiled.Places.InUpdateOrder[1];
        RoomEnemyPlace compiledPlace = compiled.Places.InUpdateOrder[0];
        var boundDefinition = EnemyDefinitionFixtures.Blank;
        nativePlace.Bind(EnemyDefinitionId.Oum, boundDefinition);
        compiledPlace.Bind(EnemyDefinitionId.Oum, boundDefinition);
        // The native place is an Oum too: its frame's word is read in its header's bank (#2327).
        nativePlace.Bind(EnemyDefinitionId.Oum, nativePlace.KeptDefinition);
        compiledPlace.Bind(EnemyDefinitionId.Oum, compiledPlace.KeptDefinition);
        AssertEqual(30, OumCollisionDefinitionsConstants.FramePointers.Length,
            "all Oum extended frames have compiled collision");
        AssertEqual(30, OumCollisionDefinitions.HitboxPointers.Count(),
            "all Oum hitbox lists have compiled collision");

        var seenLists = new HashSet<ushort>();
        int boxesChecked = 0;
        int probes = 0;
        foreach (ushort frame in OumCollisionDefinitionsConstants.FramePointers)
        {
            AssertEqual((ushort)1, rom.ReadCartridgeWord((0xa20000 | frame)),
                $"Oum $A2:{frame:X4} native one-component header");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa20000 | frame + 2)),
                "Oum native component X is zero");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa20000 | frame + 4)),
                "Oum native component Y is zero");
            ushort list = OumCollisionDefinitions.HitboxListAt(frame);
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | frame + 8)), list,
                "Oum native frame hitbox-list pointer");
            seenLists.Add(list);
            ReadOnlySpan<EnemyCollisionHitbox> boxes =
                OumCollisionDefinitions.HitboxesAt(list);
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | list)), (ushort)boxes.Length,
                $"Oum $A2:{list:X4} native hitbox count");
            for (int index = 0; index < boxes.Length; index++)
            {
                EnemyCollisionHitbox box = boxes[index];
                int record = 0xa20000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(record)), box.Left, "Oum left");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 2))), box.Top, "Oum top");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 4))), box.Right, "Oum right");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 6))), box.Bottom, "Oum bottom");
                AssertEqual(EnemyReactionAccess.Of(0xa2, rom.ReadCartridgeWord((record + 8)), shot: false), (EnemyReaction?)box.Touch, "Oum touch callback");
                AssertEqual(EnemyReactionAccess.Of(0xa2, rom.ReadCartridgeWord((record + 10)), shot: true), (EnemyReaction?)box.Shot, "Oum shot callback");
                boxesChecked++;
            }

            nativePlace.Frame = compiledPlace.Frame = RoomEnemyPlaceFrameAccess.FrameAt(OumCollisionDefinitions.Bank, frame);
            foreach ((ushort originX, ushort originY) in
                     new VerifyOumCollisionDefinitionsCase[]
                     {
                         new VerifyOumCollisionDefinitionsCase(0x0100, 0x0100), new VerifyOumCollisionDefinitionsCase(0x0004, 0x0006), new VerifyOumCollisionDefinitionsCase(0xfffc, 0xfffa),
                     })
            {
                compiledPlace.Position = compiledPlace.Position.WithWholeX(originX);
                nativePlace.Position = nativePlace.Position.WithWholeX(originX);
                compiledPlace.Position = compiledPlace.Position.WithWholeY(originY);
                nativePlace.Position = nativePlace.Position.WithWholeY(originY);
                foreach (EnemyCollisionHitbox box in boxes)
                {
                    ushort[] xs = BoundaryPoints(originX, box.Left, box.Right);
                    ushort[] ys = BoundaryPoints(originY, box.Top, box.Bottom);
                    foreach (ushort x in xs)
                    foreach (ushort y in ys)
                    for (int shot = 0; shot <= 1; shot++)
                    {
                        object?[] nativeArguments =
                            [nativePlace, new Position16<RoomPixels>(x, y), Extent16<RoomPixels>.Zero, shot != 0, (ushort)0];
                        object?[] compiledArguments =
                            [compiledPlace, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(0, 0), shot != 0, (ushort)0];
                        bool nativeHit = ReferenceExtendedCollision(rom, OumCollisionDefinitions.Bank, nativeArguments);
                        bool compiledHit = ExtendedHitboxWalker.Find(compiledArguments);
                        AssertEqual(nativeHit, compiledHit,
                            $"Oum $A2:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                        AssertEqual(ExtendedHitboxWalker.NativeReaction(OumCollisionDefinitions.Bank, nativeArguments),
                            ExtendedHitboxWalker.Reaction(compiledArguments),
                            $"Oum $A2:{frame:X4} reaction {x:X4},{y:X4}, shot={shot}");
                        probes++;
                    }
                }
            }
        }
        AssertTrue(seenLists.SetEquals(OumCollisionDefinitions.HitboxPointers),
            "every compiled Oum list belongs to a selected frame");
        AssertEqual(66, boxesChecked, "all Oum physical rectangles were verified");
        AssertThrows<InvalidDataException>(
            () => OumCollisionDefinitions.HitboxListAt(0x8000),
            "unknown Oum frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => OumCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Oum hitbox list fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Oum touch and shot walkers never read ROM bytes");
        Console.WriteLine($"Oum collision: 30 frames, 66 rectangles, {probes} " +
            "native-equivalent touch/shot boundary probes with no ROM reads.");

        static ushort[] BoundaryPoints(int origin, int low, int high) =>
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
        Suite(nameof(VerifyOumCollisionDefinitions), () => VerifyOumCollisionDefinitions());
        return 0;
    }
}

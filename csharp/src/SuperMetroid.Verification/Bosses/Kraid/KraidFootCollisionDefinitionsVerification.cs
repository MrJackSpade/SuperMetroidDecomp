using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidFootCoordinateMappingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid foot frame collision and touch/shot callbacks against native extended hitboxes.</summary>
internal static class KraidFootCollisionDefinitionsVerification
{
    /// <summary>Probes all walking frames at normal and wrapping origins with boundary points around each rectangle.</summary>
    internal static void VerifyKraidFootCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewKraidFootCollisionNoReadBus();
        var installed = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemySlot installedFoot = installed.SlotTable.Slots[0];
        installedFoot.Bind(installedFoot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa7 });
        installedFoot.Bind(EnemyDefinitionId.KraidFoot, installedFoot.Definition);
        AssertEqual(35, KraidFootCollisionDefinitions.FrameCount,
            "Kraid foot walking extended-frame count");
        ushort[] frames =
        [
            KraidFootCollisionDefinitions.InitialFrame,
            .. Enumerable.Range(0, KraidFootCollisionDefinitions.FrameCount)
                .Select(KraidFootCollisionDefinitions.FramePointer),
        ];
        Suite(nameof(VerifyKraidFootFirstX), () => VerifyKraidFootFirstX(rom));
        Suite(nameof(VerifyKraidFootFirstY), () => VerifyKraidFootFirstY(rom));
        Suite(nameof(VerifyKraidFootSecondX), () => VerifyKraidFootSecondX(rom));
        Suite(nameof(VerifyKraidFootSecondY), () => VerifyKraidFootSecondY(rom));
        Suite(nameof(VerifyKraidFootSharedHitbox), () => VerifyKraidFootSharedHitbox(rom));

        // The typed foot programs match bank $A7 (--kraid-part-animations); every frame they show is a physical frame.
        foreach (ushort selectedFrame in KraidAnimations.Spritemaps(KraidPartProgram.FootInitial, KraidPartProgram.FootNeutral, KraidPartProgram.FootWalkingForward,
                     KraidPartProgram.FootWalkingForwardFinished, KraidPartProgram.FootLungeForward, KraidPartProgram.FootLungeForwardFinished,
                     KraidPartProgram.FootWalkingBackward, KraidPartProgram.FootWalkingBackwardLoop))
            AssertTrue(KraidFootCollisionDefinitions.TryGetComponents(selectedFrame, out _),
                $"Kraid foot frame $A7:{selectedFrame:X4} is a compiled physical frame");

        KraidFootHitboxSequence hitboxes =
            KraidFootCollisionDefinitionsConstants.HitboxesAt(KraidFootCollisionDefinitionsConstants.HitboxList);
        KraidFootCollisionHitbox box = hitboxes[0];

        int probes = 0;
        // A foot shows only installed frames; the catalog's stride also spans $8F35, which no composition shows.
        foreach (ushort frame in frames.Where(frame => RoomEnemySlotFrameAccess.Shows(0xa7, frame)))
        foreach ((ushort originX, ushort originY) in
                 new (ushort, ushort)[]
                 {
                     (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                 })
        {
            installedFoot.SpritemapPointer = frame;
            installedFoot.Position = installedFoot.Position.WithWhole(new(originX, originY));
            KraidFootCollisionDefinitions.TryGetComponents(frame, out var components);
            foreach (KraidFootCollisionComponent component in components)
            {
                ushort componentX = unchecked((ushort)(originX + component.Offset.X));
                ushort componentY = unchecked((ushort)(originY + component.Offset.Y));
                foreach (ushort x in BoundaryPoints(componentX, box.Left, box.Right))
                foreach (ushort y in BoundaryPoints(componentY, box.Top, box.Bottom))
                for (int shot = 0; shot <= 1; shot++)
                {
                    object?[] installedArguments =
                        [installedFoot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(0, 0), shot != 0, (ushort)0];
                    bool nativeHit = NativeHit(frame, originX, originY, x, y, shot != 0, out ushort nativeCallback);
                    bool installedHit = ExtendedHitboxWalker.Find(installed, installedArguments);
                    AssertEqual(nativeHit, installedHit,
                        $"Kraid foot $A7:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                    AssertEqual(nativeCallback,
                        (ushort)(installedArguments[^1] ?? throw new InvalidOperationException("Verification requires installedArguments[^1].")),
                        $"Kraid foot $A7:{frame:X4} callback {x:X4},{y:X4}, shot={shot}");
                    probes++;
                }
            }
        }

        AssertTrue(!KraidFootCollisionDefinitions.TryGetComponents(0x8000, out _),
            "uncompiled Kraid foot frame is rejected");
        AssertThrows<InvalidDataException>(
            () => KraidFootCollisionDefinitionsConstants.HitboxesAt(0x8000).ToArray(),
            "uncompiled Kraid foot hitbox list fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "all installed Kraid foot collision probes read no cartridge data");
        Console.WriteLine($"Kraid foot collision: {frames.Length} frames, " +
            $"one hitbox list and {probes} native-equivalent touch/shot probes; " +
            "installed path forbids ROM reads.");

        ushort ReadWord(ushort address) => unchecked(
            SnesLittleEndian.Word(KraidFootCollisionDefinitionsRomByte(rom, 0xa70000 | address), KraidFootCollisionDefinitionsRomByte(rom, 0xa70000 | unchecked((ushort)(address + 1)))));

        bool NativeHit(ushort frame, ushort originX, ushort originY,
            ushort x, ushort y, bool shot, out ushort callback)
        {
            // Import-only decoder: components are eight bytes; hitboxes are twelve.
            // Native $A0:9A5A and $9B7F differ at their rectangle boundaries.
            callback = 0;
            for (int part = 0; part < ReadWord(frame); part++)
            {
                ushort component = unchecked((ushort)(frame + 2 + part * 8));
                ushort cx = unchecked((ushort)(originX + ReadWord(component)));
                ushort cy = unchecked((ushort)(originY + ReadWord((ushort)(component + 2))));
                ushort list = ReadWord((ushort)(component + 6));
                for (int boxIndex = 0; boxIndex < ReadWord(list); boxIndex++)
                {
                    ushort rectangle = unchecked((ushort)(list + 2 + boxIndex * 12));
                    ushort left = unchecked((ushort)(cx + ReadWord(rectangle)));
                    ushort top = unchecked((ushort)(cy + ReadWord((ushort)(rectangle + 2))));
                    ushort right = unchecked((ushort)(cx + ReadWord((ushort)(rectangle + 4))));
                    ushort bottom = unchecked((ushort)(cy + ReadWord((ushort)(rectangle + 6))));
                    bool hit = shot
                        ? unchecked((short)(x - left)) >= 0 && unchecked((short)(x - right)) < 0 &&
                          unchecked((short)(y - top)) >= 0 && unchecked((short)(y - bottom)) < 0
                        : unchecked((short)(left - x)) < 0 && unchecked((short)(right - x)) >= 0 &&
                          unchecked((short)(top - y)) < 0 && unchecked((short)(bottom - y)) >= 0;
                    if (!hit) continue;
                    callback = ReadWord((ushort)(rectangle + (shot ? 10 : 8)));
                    return true;
                }
            }
            return false;
        }
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

    /// <summary>Reads one native bank-$A7 collision byte for the independent hitbox oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid foot collision definitions")]
    private static byte KraidFootCollisionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Rejects all cartridge reads and writes while installed Kraid foot collision runs.</summary>
    private static CartridgeReadGuard NewKraidFootCollisionNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Kraid-foot collision read ROM byte ${address:X6}.")
            .RejectWrites(address => $"Installed Kraid-foot collision wrote byte ${address:X6}.");

    /// <summary>Runs the focused Kraid foot collision comparison against the retail ROM.</summary>
    [VerificationCommand("--kraid-foot-collision")]
    internal static int KraidFootCollisionCommand()
    {
        Suite(nameof(VerifyKraidFootCollisionDefinitions), () => VerifyKraidFootCollisionDefinitions());
        return 0;
    }
}

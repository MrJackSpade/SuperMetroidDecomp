using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Reflection;
using static IntroRinkaProgramsVerification;
using static SuiteTimingVerification;

/// <summary>Represents intro rinka definitions verification.</summary>
internal static class IntroRinkaDefinitionsVerification
{
    /// <summary>Verifies intro rinka definitions.</summary>
    internal static void VerifyIntroRinkaDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyIntroRinkaPrograms), () => VerifyIntroRinkaPrograms(retail));
        // Native six-byte definitions $8B:CF21 (Rinka) and $8B:CF27 (spawner). Their first
        // word, the initialization callback, has no port counterpart.
        (IntroRinkaActorDefinition Actor, ushort NativePointer)[] actors =
        [
            (IntroRinkaDefinitions.RinkaActor, 0xcf21),
            (IntroRinkaDefinitions.SpawnerActor, 0xcf27),
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            IntroRinkaActorDefinition actual = actors[index].Actor;
            int address = IntroRinkaDefinitions.NativeBank | actors[index].NativePointer;
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 2)), actual.PreInstruction,
                $"intro Rinka actor {index} pre-instruction callback");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 4)), actual.InstructionList,
                $"intro Rinka actor {index} instruction list");
        }

        for (int parameter = 0; parameter < IntroRinkaDefinitions.RinkaCount; parameter++)
        {
            IntroRinkaPhysicalDefinition actual = IntroRinkaDefinitions.Rinka(parameter);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroRinkaDefinitions.InitialXReferenceAddress + parameter * 2)),
                actual.Position.X, $"intro Rinka {parameter} initial X");
            AssertEqual(unchecked((ushort)(CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroRinkaDefinitions.InitialYReferenceAddress + parameter * 2)) - 8)),
                actual.Position.Y, $"intro Rinka {parameter} biased initial Y");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroRinkaDefinitions.XWholeVelocityReferenceAddress + parameter * 2))),
                actual.XWholeVelocity, $"intro Rinka {parameter} signed X whole velocity");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroRinkaDefinitions.Rinka(IntroRinkaDefinitions.RinkaCount),
            "intro Rinka definition boundary");

        var guarded = NewIntroRinkaDefinitionGuard(retail);
        var system = new IntroRinkaSystem();
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0100)),
        };
        for (int frame = 0; frame < 220; frame++)
        {
            system.Step(guarded, samus, motherBrainExploding: false, explosionsAllocated: false);
            int expectedCount = frame < 74 ? 0 : frame < 202 ? 2 : 4;
            AssertEqual(expectedCount, system.LiveRinkas.Count,
                $"intro Rinka two-wave spawn count at frame {frame}");
            // The spawner holds slot 14 and its Rinkas take lower free slots, so the native
            // descending walk steps each new Rinka in its spawn call (capture update 1793).
            if (frame >= 74)
            {
                int relative = frame - 74;
                ushort expectedSprite = relative < 30
                    ? (ushort)(0x8c8d + relative / 10 * 0x16)
                    : ((relative - 30) / 10 % 4) switch
                    {
                        0 or 2 => (ushort)0x8ca3,
                        1 => (ushort)0x8c8d,
                        _ => (ushort)0x8cb9,
                    };
                AssertEqual(expectedSprite, system.Rinka(0).SpriteMapPointer,
                    $"intro first Rinka selects its native frame at call {frame}");
            }
        }
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.LiveRinkas.Count,
            "intro Rinka spawner allocates both native waves");
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.ActiveCount,
            "all intro Rinkas remain active before Mother Brain explodes");

        for (int frame = 0; frame < 100; frame++)
            system.Step(guarded, samus, motherBrainExploding: true, explosionsAllocated: false);
        AssertEqual(0, system.ActiveCount,
            "all intro Rinkas retire after Mother Brain begins exploding");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro Rinka production path never rereads compiled definitions, lists or physical tables");

        Console.WriteLine(
            "  Intro Rinka definitions: six actor, twelve physical and 48 instruction bytes match; both timed spawn waves, animation and cleanup are ROM-table independent.");
    }

    /// <summary>New intro rinka definition guard.</summary>
    private static CartridgeReadGuard NewIntroRinkaDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x8bcf21 and < 0x8bcf2d or
                >= 0x8bcdeb and < 0x8bce1b or
                >= 0x8bb8b5 and < 0x8bb8c5 or
                >= 0x8bb985 and < 0x8bb98d,
            address => $"Intro Rinka reread compiled byte ${address:X6}.");

    /// <summary>Intro rinka definitions command.</summary>
    [VerificationCommand("--intro-rinka-definitions")]
    internal static int IntroRinkaDefinitionsCommand()
    {
        Suite(nameof(VerifyIntroRinkaDefinitions), () => VerifyIntroRinkaDefinitions());
        return 0;
    }
}

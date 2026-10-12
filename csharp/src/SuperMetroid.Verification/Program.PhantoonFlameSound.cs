using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using static PhantoonInstructionProgramDefinitionsVerification;
using static SharedSlopeHeightsVerification;

internal static class PhantoonFlameSoundVerification
{
    internal static void VerifyPhantoonFlameSound()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = CreatePhantoonInstructionSystem(NewSlopeHeightNoReadBus());
        var callback = typeof(RoomEnemySystem).GetMethod("ProcessPhantoonInstructionFunction", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, bool>>(enemies);
        var begin = typeof(RoomEnemySystem).GetMethod("BeginEnemySoundRequestFrame", flags)!
            .CreateDelegate<Action>(enemies);
        var collect = typeof(RoomEnemySystem).GetMethod("CollectLegacyEnemyAudioRequests", flags)!
            .CreateDelegate<Action>(enemies);
        begin();
        callback(enemies.Slots[3], (ushort)PhantoonInstruction.SpawnCasualFlame);
        collect();
        AssertEqual(1, enemies.SoundRequests.Count, "casual flame emits one sound request");
        var flameSound = new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d), 6);
        AssertEqual(flameSound, enemies.SoundRequests[0], "native A7:CF68/CF6B queues library three sound 1D with Max6");
        AssertEqual(1, enemies.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame),
            "sound accompanies the actual casual-flame spawn");

        begin();
        callback(enemies.Slots[0], (ushort)PhantoonInstruction.PlayPhantoonMaterializationSFX);
        callback(enemies.Slots[3], (ushort)PhantoonInstruction.SpawnCasualFlame);
        callback(enemies.Slots[3], (ushort)PhantoonInstruction.SpawnCasualFlame);
        collect();
        AssertEqual(2, enemies.SoundRequests.Count(request => request == flameSound),
            "distinct flame callbacks retain their separate native queue calls");
        AssertTrue(enemies.SoundRequests.Contains(new EnemySoundRequest(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x79), 6)),
            "flame audio does not overwrite the independent materialization sound");
        Console.WriteLine("Phantoon casual flame: actual spawn emits library 3 / 1D / Max6; materialization audio remains independent.");
    }

    [VerificationCommand("--phantoon-flame-sound")]
    internal static int PhantoonFlameSoundCommand()
    {
        Suite(nameof(VerifyPhantoonFlameSound), () => VerifyPhantoonFlameSound());
        return 0;
    }
}

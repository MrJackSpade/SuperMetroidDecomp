using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks audio requests emitted by Phantoon's casual-flame and materialization callbacks.</summary>
internal static class PhantoonFlameSoundVerification
{
    /// <summary>Confirms each actual flame spawn queues library-three sound $1D with Max6 without replacing materialization audio.</summary>
    internal static void VerifyPhantoonFlameSound()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = CreatePhantoonInstructionSystem(NewSlopeHeightNoReadBus());
        Func<RoomEnemySlot, ushort, bool> callback = enemies.PhantoonBehavior.ProcessPhantoonInstructionFunction;
        var begin = new Action(enemies.BeginEnemySoundRequestFrame);
        var collect = new Action(enemies.CollectLegacyEnemyAudioRequests);
        begin();
        callback(enemies.SlotTable.Slots[3], (ushort)PhantoonInstruction.SpawnCasualFlame);
        collect();
        AssertEqual(1, enemies.Audio.SoundRequests.Count, "casual flame emits one sound request");
        var flameSound = new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d), 6);
        AssertEqual(flameSound, enemies.Audio.SoundRequests[0], "native A7:CF68/CF6B queues library three sound 1D with Max6");
        AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame),
            "sound accompanies the actual casual-flame spawn");

        begin();
        callback(enemies.SlotTable.Slots[0], (ushort)PhantoonInstruction.PlayPhantoonMaterializationSFX);
        callback(enemies.SlotTable.Slots[3], (ushort)PhantoonInstruction.SpawnCasualFlame);
        callback(enemies.SlotTable.Slots[3], (ushort)PhantoonInstruction.SpawnCasualFlame);
        collect();
        AssertEqual(2, enemies.Audio.SoundRequests.Count(request => request == flameSound),
            "distinct flame callbacks retain their separate native queue calls");
        AssertTrue(enemies.Audio.SoundRequests.Contains(new EnemySoundRequest(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x79), 6)),
            "flame audio does not overwrite the independent materialization sound");
        Console.WriteLine("Phantoon casual flame: actual spawn emits library 3 / 1D / Max6; materialization audio remains independent.");
    }

    /// <summary>Runs the focused Phantoon flame and materialization sound check.</summary>
    [VerificationCommand("--phantoon-flame-sound")]
    internal static int PhantoonFlameSoundCommand()
    {
        Suite(nameof(VerifyPhantoonFlameSound), () => VerifyPhantoonFlameSound());
        return 0;
    }
}

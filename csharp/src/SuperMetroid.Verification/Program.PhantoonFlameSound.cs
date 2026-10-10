using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Verifies that each casual-flame spawn queues its library-three sound with Max6,
    /// without replacing Phantoon's independent materialization sound request.
    /// </summary>
    private static void VerifyPhantoonFlameSound()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = CreatePhantoonInstructionSystem(new SlopeHeightNoReadBus());
        var callback = typeof(RoomEnemySystem).GetMethod("ProcessPhantoonInstructionFunction", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, byte, bool>>(enemies);
        var begin = typeof(RoomEnemySystem).GetMethod("BeginEnemySoundRequestFrame", flags)!
            .CreateDelegate<Action>(enemies);
        var collect = typeof(RoomEnemySystem).GetMethod("CollectLegacyEnemyAudioRequests", flags)!
            .CreateDelegate<Action>(enemies);
        begin();
        callback(enemies.Slots[3], PhantoonInstructionCodes.SpawnCasualFlame, 0);
        collect();
        AssertEqual(1, enemies.SoundRequests.Count, "casual flame emits one sound request");
        var flameSound = new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d), 6);
        AssertEqual(flameSound, enemies.SoundRequests[0], "native A7:CF68/CF6B queues library three sound 1D with Max6");
        AssertEqual(1, enemies.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame),
            "sound accompanies the actual casual-flame spawn");

        begin();
        callback(enemies.Slots[0], PhantoonInstructionCodes.PlayPhantoonMaterializationSFX, 0);
        callback(enemies.Slots[3], PhantoonInstructionCodes.SpawnCasualFlame, 0);
        callback(enemies.Slots[3], PhantoonInstructionCodes.SpawnCasualFlame, 0);
        collect();
        AssertEqual(2, enemies.SoundRequests.Count(request => request == flameSound),
            "distinct flame callbacks retain their separate native queue calls");
        AssertTrue(enemies.SoundRequests.Contains(new EnemySoundRequest(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x79), 6)),
            "flame audio does not overwrite the independent materialization sound");
        Console.WriteLine("Phantoon casual flame: actual spawn emits library 3 / 1D / Max6; materialization audio remains independent.");
    }
}

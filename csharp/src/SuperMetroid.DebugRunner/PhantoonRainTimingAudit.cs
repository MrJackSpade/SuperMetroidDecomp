using System.Reflection;
using SuperMetroid.Core.Game;

/// <summary>Literal $86:9A94 timer branches and sound publications, through the production rain callback.</summary>
internal static class PhantoonRainTimingAudit
{
    public static int Run(string rom)
    {
        int cases = 0;
        foreach (ushort timer in new ushort[] { 0, 1, 2, 8, 0x8000, 0x8001, 0xffff })
        {
            var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
            var enemies = runtime.Enemies;
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(RoomEnemySystem).GetMethod("SpawnPhantoonDestroyableFlame", flags)!
                .Invoke(enemies, [enemies.Phantoon!.Body, (ushort)0x0410]);
            var flame = enemies.EnemyProjectiles.Single(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);
            flame.XVelocity = timer;
            int soundsBefore = enemies.SoundRequests.Count;
            typeof(RoomEnemySystem).GetMethod("RunPhantoonRainFlame", flags)!
                .Invoke(enemies, [flame, runtime.LevelData!]);
            ushort decremented = unchecked((ushort)(timer - 1));
            bool starts = timer != 0 && (decremented == 0 || (short)decremented < 0);
            bool falls = timer == 0 || starts;
            if (flame.YVelocity != (falls ? 16 : 0))
                throw new InvalidDataException($"Rain timer {timer:X4}: velocity {flame.YVelocity}, expected {(falls ? 16 : 0)} on this same call.");
            int sounds = enemies.SoundRequests.Count - soundsBefore;
            if (sounds != (starts ? 1 : 0)) throw new InvalidDataException($"Rain timer {timer:X4}: sound count {sounds}, expected {(starts ? 1 : 0)}.");
            if (starts && enemies.SoundRequests[^1] != new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d), 6))
                throw new InvalidDataException("Rain start queued the wrong library, sound or capacity.");
            if (timer == 1)
            {
                ushort falling = flame.PreInstruction;
                int frames = 0;
                while (flame.PreInstruction == falling && frames++ < 200)
                    typeof(RoomEnemySystem).GetMethod("RunPhantoonRainFlame", flags)!
                        .Invoke(enemies, [flame, runtime.LevelData!]);
                if (flame.PreInstruction != EnemyProjectileCodePointers.RTS_869A44 ||
                    enemies.SoundRequests.Count - soundsBefore != 2 ||
                    enemies.SoundRequests[^1] != new EnemySoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d), 6))
                    throw new InvalidDataException("Rain impact failed to finish falling with its second native sound request.");
                Console.WriteLine($"Rain reached real-room terrain after {frames} further motion calls and retained both start/impact sounds.");
            }
            cases++;
        }
        Console.WriteLine($"Phantoon rain: {cases} timer branches match immediate fall and library-three sound publication.");
        return 0;
    }
}

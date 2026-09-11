using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Compares real scene actors with isolated original-CPU initializer/handler traces.</summary>
internal static class CeresActorNativeAudit
{
    public static int Run(string romPath, string nativeCsv)
    {
        var rows = File.ReadLines(nativeCsv).Skip(1).Select(line => line.Split(',').Select(int.Parse).ToArray())
            .ToDictionary(row => (row[0], row[1], row[2]));
        var scene = new CeresDestructionCinematicState(SuperMetroidAddressSpace.LoadRetailRom(romPath));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        object Field(string name) => typeof(CeresDestructionCinematicState).GetField(name, flags)!.GetValue(scene)!;
        var actors = (List<IntroDiscoverySprite>)Field("actors");
        var seen = new HashSet<IntroDiscoverySprite>(actors);
        var tracked = new List<(IntroDiscoverySprite Actor, int Group, int Param, int Birth, ushort X, ushort Y)>();
        int repeated = 0, checkedFrames = 0;
        for (int frame = 0; frame < 600; frame++)
        {
            scene.Step();
            int clock = (int)Field("explosionSpawnerFrame");
            int ordinal = 0;
            foreach (var actor in actors)
            {
                if (!seen.Add(actor)) continue;
                // The departure actor is a separate definition, not one of these three groups.
                if (scene.Phase >= CeresDestructionPhase.FlyingAwayFromExplosion) continue;
                int group = clock == CeresDestructionRomData.Timing.FirstExplosionFrame ? 0 :
                    clock == CeresDestructionRomData.Timing.FinalExplosionFrame ? 2 : 1;
                int param = group == 1 ? repeated++ : ordinal++;
                tracked.Add((actor, group, param, frame, (ushort)Field("backgroundX"), (ushort)Field("backgroundY")));
            }
            foreach (var item in tracked)
            {
                int age = frame - item.Birth;
                if (age >= 260) continue;
                int[] expected = rows[(item.Group, item.Param, age)];
                var actor = item.Actor;
                ushort x = unchecked((ushort)(expected[4] - item.X));
                ushort y = unchecked((ushort)(expected[6] - item.Y));
                if (actor.IsActive != (expected[3] != 0) || actor.SpriteMapPointer != expected[8] ||
                    actor.XPosition != x || actor.YPosition != y ||
                    actor.XSubPosition != expected[5] || actor.YSubPosition != expected[7])
                    throw new InvalidDataException($"Ceres group {item.Group}/{item.Param} age {age}: " +
                        $"actual active={actor.IsActive} XY={actor.XPosition}.{actor.XSubPosition}/{actor.YPosition}.{actor.YSubPosition} map={actor.SpriteMapPointer:X4}; " +
                        $"native active={expected[3]} XY={x}.{expected[5]}/{y}.{expected[7]} map={expected[8]:X4}.");
                checkedFrames++;
            }
        }
        if (tracked.Count != 15) throw new InvalidDataException($"Expected 15 spawner children, observed {tracked.Count}.");
        Console.WriteLine($"Ceres native actor comparison: {tracked.Count} actors, {checkedFrames} frame states match position/subposition, spritemap and lifetime.");
        return 0;
    }
}

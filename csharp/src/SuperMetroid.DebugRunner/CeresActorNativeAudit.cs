using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Compares real scene actors with isolated original-CPU initializer/handler traces.</summary>
internal static class CeresActorNativeAudit
{
    public static int Run(string romPath, string nativeCsv)
    {
        var records = File.ReadLines(nativeCsv).Skip(1).Select(line => line.Split(',')).ToArray();
        var rows = records.Select(row => row.Take(9).Select(int.Parse).ToArray())
            .ToDictionary(row => (row[0], row[1], row[2]));
        var drawRows = records.ToDictionary(row => (int.Parse(row[0]), int.Parse(row[1]), int.Parse(row[2])),
            row => (Low: Convert.FromHexString(row[9]), High: Convert.FromHexString(row[10])));
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var scene = new CeresDestructionCinematicState(bus);
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
                // Restore the native probe's zero-camera origin through the production
                // draw API. This compares byte packing, attributes and component order;
                // it does not claim coverage of clipping at the real scene's edges.
                var oam = new OamBuffer();
                oam.BeginFrame();
                actor.Draw(bus, oam, unchecked((ushort)-item.X), unchecked((ushort)-item.Y));
                var draw = drawRows[(item.Group, item.Param, age)];
                if (!oam.LowTable[..oam.NextByteOffset].SequenceEqual(draw.Low) ||
                    !oam.HighTable.SequenceEqual(draw.High))
                    throw new InvalidDataException($"Ceres group {item.Group}/{item.Param} age {age}: original-CPU OAM differs.");
            }
        }
        if (tracked.Count != 15) throw new InvalidDataException($"Expected 15 spawner children, observed {tracked.Count}.");
        Console.WriteLine($"Ceres native actor comparison: {tracked.Count} actors, {checkedFrames} frame states match position/subposition, spritemap, lifetime and normalized-origin OAM.");
        return 0;
    }
}

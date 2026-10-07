using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static int VerifyMotherBrainTubeDescent()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var enemies = new RoomEnemySystem { MotherBrainRoomColors = RepositoryInstallation.Maps.MotherBrainRoomColors };
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        var state = new MotherBrainEnemyState(enemies.Slots[0]) { Head = enemies.Slots[1] };
        typeof(RoomEnemySystem).GetField("_motherBrain", flags)!.SetValue(enemies, state);
        var tube = enemies.Slots[2];
        tube.Parameter1 = 8;
        tube.Parameter2 = 0;
        tube.XPosition = 128;
        tube.YPosition = 128;
        state.Head.YPosition = 72;
        typeof(RoomEnemySystem).GetMethod("InitializeMotherBrainFallingTube",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [tube]);
        var step = typeof(RoomEnemySystem).GetMethod("RunMotherBrainFallingTubeMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        uint expectedPosition = (uint)tube.YPosition << 16;
        int speed = 0, movingFramesBeforeHide = 0, hiddenFramesBeforeLanding = 0;
        ushort previousHeadY = state.Head.YPosition;
        int frame;
        for (frame = 0; frame < 200; frame++)
        {
            speed += 6;
            expectedPosition += (uint)speed << 8;
            ushort expectedY = (ushort)(expectedPosition >> 16);
            ushort expectedHead = (ushort)Math.Min(expectedY - 56, 196);
            step(tube);
            AssertEqual(expectedY, tube.YPosition, $"Tube native acceleration at frame {frame}");
            AssertEqual((ushort)expectedPosition, tube.YSubposition, "Tube retains fractional motion");
            AssertEqual(expectedHead, state.Head.YPosition, $"Brain follows tube every frame ({frame})");
            AssertEqual(expectedY >= 244, tube.Properties.HasAny(EnemyProperties.Invisible),
                "Tube hiding threshold does not gate head descent");
            if (expectedY < 244 && state.Head.YPosition > previousHeadY) movingFramesBeforeHide++;
            if (expectedY >= 244 && expectedHead < 196) hiddenFramesBeforeLanding++;
            previousHeadY = state.Head.YPosition;
            if (expectedHead == 196) break;
            AssertTrue(!tube.Properties.HasAny(EnemyProperties.Deleted), "Tube remains alive before landing");
        }
        AssertTrue(frame < 200 && movingFramesBeforeHide > 20 && hiddenFramesBeforeLanding > 0,
            "Trajectory includes continuous descent both before hiding and before landing");
        AssertTrue(tube.Properties.HasAny(EnemyProperties.Deleted), "Landing deletes the main tube");
        AssertEqual(MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3, state.Function, "Landing starts room rebuild");
        AssertEqual((ushort)0x20, enemies.EarthquakeTimer, "Landing retains native earthquake duration");
        Console.WriteLine($"Mother Brain tube descent: {frame + 1} exact trajectory frames, hide threshold and landing passed.");
        return 0;
    }
}

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents mother brain tube descent verification.</summary>
internal static class MotherBrainTubeDescentVerification
{
    /// <summary>Verifies mother brain tube descent.</summary>
    internal static int VerifyMotherBrainTubeDescent()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var enemies = new RoomEnemySystem { Presentation = { MotherBrainRoomColors = InstalledFixtures.Maps.MotherBrainRoomColors } };
        enemies.Presentation._cgram = new SnesCgram();
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]) { Head = enemies.SlotTable.Slots[1] };
        enemies.MotherBrainEncounter.MotherBrain = state;
        var tube = enemies.SlotTable.Slots[2];
        tube.Parameter1 = 8;
        tube.Parameter2 = 0;
        tube.Position = tube.Position.WithWhole(new(128, 128));
        state.Head.Position = state.Head.Position.WithWholeY(72);
        MotherBrainBehavior.InitializeMotherBrainFallingTube(tube);
        Action<RoomEnemySlot> step = enemies.MotherBrainBehavior.RunMotherBrainFallingTubeMain;
        uint expectedPosition = SnesSixteenSixteen.FromWhole((uint)tube.Position.Y.Whole);
        int speed = 0, movingFramesBeforeHide = 0, hiddenFramesBeforeLanding = 0;
        ushort previousHeadY = state.Head.Position.Y.Whole;
        int frame;
        for (frame = 0; frame < 200; frame++)
        {
            speed += 6;
            expectedPosition += SnesSixteenSixteen.FromEightEight((uint)speed);
            ushort expectedY = SnesFixedPosition.WholeOf(unchecked((int)expectedPosition));
            ushort expectedHead = (ushort)Math.Min(expectedY - 56, 196);
            step(tube);
            AssertEqual(expectedY, tube.Position.Y.Whole, $"Tube native acceleration at frame {frame}");
            AssertEqual((ushort)expectedPosition, tube.Position.Y.Fraction, "Tube retains fractional motion");
            AssertEqual(expectedHead, state.Head.Position.Y.Whole, $"Brain follows tube every frame ({frame})");
            AssertEqual(expectedY >= 244, tube.Properties.HasAny(EnemyProperties.Invisible),
                "Tube hiding threshold does not gate head descent");
            if (expectedY < 244 && state.Head.Position.Y.Whole > previousHeadY) movingFramesBeforeHide++;
            if (expectedY >= 244 && expectedHead < 196) hiddenFramesBeforeLanding++;
            previousHeadY = state.Head.Position.Y.Whole;
            if (expectedHead == 196) break;
            AssertTrue(!tube.Properties.HasAny(EnemyProperties.Deleted), "Tube remains alive before landing");
        }
        AssertTrue(frame < 200 && movingFramesBeforeHide > 20 && hiddenFramesBeforeLanding > 0,
            "Trajectory includes continuous descent both before hiding and before landing");
        AssertTrue(tube.Properties.HasAny(EnemyProperties.Deleted), "Landing deletes the main tube");
        AssertEqual(MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3, state.Function, "Landing starts room rebuild");
        AssertEqual((ushort)0x20, enemies.Earthquake.EarthquakeTimer, "Landing retains native earthquake duration");
        Console.WriteLine($"Mother Brain tube descent: {frame + 1} exact trajectory frames, hide threshold and landing passed.");
        return 0;
    }

    /// <summary>Mother brain tube descent command.</summary>
    [VerificationCommand("--mother-brain-tube-descent")]
    internal static int MotherBrainTubeDescentCommand()
    {
        return VerifyMotherBrainTubeDescent();
    }
}

using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Compares wrapped beam collision widths and trajectories against 1,080 native boundary records.</summary>
internal static class WrapShotWidthsVerification
{
    /// <summary>A plain solid block: the room's far wall.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>The blue door's cap: block $00C mirrored left to right, shootable.</summary>
    private static readonly RoomTerrainBlock DoorCap = RoomTerrainBlock.Of(0x00c, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock);

    /// <summary>The cap's three lower blocks: the same picture, extending to the block above.</summary>
    private static readonly RoomTerrainBlock DoorCapExtension = RoomTerrainBlock.Of(0x00c, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension);

    /// <summary>The cap once a shot opens it: the same picture, plain solid.</summary>
    private static readonly RoomTerrainBlock OpenedDoorCap = RoomTerrainBlock.Of(0x00c, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock);

    /// <summary>A vertical extension's behaviour reaching one block up (signed -1).</summary>
    private static readonly RoomBlockBehavior OneBlockUp = new(0xff);

    /// <summary>Checks shot positions, radii, lifetime, and remote door writes at adjacent one-pixel offsets.</summary>
    internal static void VerifyWrapShotWidths(string tracePath)
    {
        var native = File.ReadLines(tracePath).Skip(1).ToArray();
        AssertEqual(1080, native.Length, "Complete native width matrix");
        int[] widths = [144, 128, 64], heights = [80, 16, 192];
        int[] originsX = [2260, 2004, 38], originsY = [546, 34, 1606], targets = [0x1561, 0x301, 0x2981];
        int cursor = 0, mismatches = 0;
        for (int room = 0; room < 3; room++)
        for (int beam = 0; beam < 3; beam++)
        for (int offset = -1; offset <= 1; offset++)
        {
            int width = widths[room], height = heights[room], target = targets[room];
            bool left = room == 2;
            var words = new RoomTerrainBlock[width * height];
            var bts = new RoomBlockBehavior[words.Length];
            for (int row = 0; row < height; row++) words[row * width + (left ? 0 : width - 1)] = Solid;
            words[target] = DoorCap; bts[target] = RoomBlockBehaviorValues.BlueDoorFacingRight;
            for (int row = 1; row < 4; row++) { words[target + row * width] = DoorCapExtension; bts[target + row * width] = OneBlockUp; }
            var level = CreateRoom(new(checked((ushort)width), checked((ushort)height)), words, bts);
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(originsX[room] + offset), (ushort)originsY[room])),
                PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                EquippedBeams = (SamusBeamFlags)((ushort)(beam == 0 ? 0 : beam == 1 ? 1 : left ? 5 : 9)) };
            var shots = CreateProjectileFixture();
            var bombs = CreateBombFixture();
            var plms = PlmFixtures.Unbound();
            int hit = -1;
            for (int frame = 0; frame < 40; frame++)
            {
                ControllerButton input = frame == 0 ? ControllerButton.X : 0;
                bombs.StepFrame(new SamusTerrain { Level = level }, samus, input, input);
                shots.StepFrame(new SamusTerrain { Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)(left ? 0 : width * 16 - 256), (ushort)Math.Max(0, originsY[room] - 128)), bombs);
                var shot = shots.Slots[0];
                RoomTerrainBlock targetWord = level.GetCollisionBlockByIndex(target).Terrain;
                if (hit < 0 && targetWord == OpenedDoorCap) hit = frame;
                string actual = $"{room},{beam},{offset},{frame},{shot.Position.X.Whole:X4},{shot.Position.X.Fraction:X4},{shot.Position.Y.Whole:X4},{shot.Position.Y.Fraction:X4},{shot.Type:X4},{shot.InstructionPointer:X4},{shot.Radius.X:X4},{shot.Radius.Y:X4},{RoomLevelWordCodec.Encode(targetWord):X4}";
                string expected = native[cursor++];
                if (actual != expected && mismatches++ < 8) Console.WriteLine($"Width mismatch:\n{actual}\n{expected}");
            }
            Console.WriteLine($"Width room={room} beam={beam} offset={offset}: hit={hit}");
            int expectedHit = room == 2 ? (beam == 0 ? -1 : 5) : (beam == 2 && offset <= 0 ? 9 : -1);
            AssertEqual(expectedHit, hit, "Native width and adjacent one-pixel activation boundary");
        }
        AssertEqual(0, mismatches, "Native beam-width trajectory, radii, lifetime and remote-door setup writes");
        Console.WriteLine("PASS 1080 original-cartridge width-boundary records.");
    }

    /// <summary>Runs the checked-in width-boundary trace comparison.</summary>
    [VerificationCommand("--wrap-shot-widths")]
    internal static int WrapShotWidthsCommand()
    {
        Suite(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv"));
        return 0;
    }
}

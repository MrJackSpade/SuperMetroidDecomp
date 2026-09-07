using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyChozoStatueState()
    {
        var loaded = DebuggerFixtureLoader.Load("issue-353-gravity-chozo-hands", 9);
        var runtime = loaded.Game.RuntimeForVerification!;
        // Old states may contain a request published by the enemy initializer before
        // runtime integration existed. A normal frame must consume that pending work;
        // inspecting the saved terrain alone would only prove the historical defect.
        loaded.Game.Step(0);
        var level = runtime.LevelData!;
        var samus = runtime.Samus!;
        var hand = level.GetCollisionBlock(0x4a, 0x17);
        Console.WriteLine($"Saved Chozo setup: room={runtime.ActiveRoom!.Pointer:X4} " +
            $"Samus={samus.XPosition:X4},{samus.YPosition:X4} pose={samus.Pose:X2}; " +
            $"hand={hand.LevelWord:X4}/BTS={hand.Behavior:X2}; " +
            $"pending=[{string.Join(",", runtime.Enemies.ChozoStatuePlmRequests)}]");
        // $AA:E725 spawns $D6EE at this exact room block. Its synchronous $84:D616
        // setup writes B080 (solid special-air hand trigger), not a sprite hitbox.
        if (hand.CollisionType != RoomCollisionType.SpecialBlock || hand.Behavior != 0x80)
            throw new InvalidDataException("Saved Chozo hand lacks the cartridge-authored special collision trigger.");
    }
}

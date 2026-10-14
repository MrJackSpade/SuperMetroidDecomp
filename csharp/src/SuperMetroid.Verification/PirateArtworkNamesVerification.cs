using static ExtendedFrameSequenceVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Checks published Walking and Wall Pirate artwork keys and their ordering.</summary>
internal static class PirateArtworkNamesVerification
{
    /// <summary>Checks 37 walking, firing, looking, and flinching frame names.</summary>
    internal static void VerifyWalkingPirateArtworkNames()
    {
        string[] expected = [
        "walking_pirate_flinch_left", "walking_pirate_flinch_right",
        "walking_pirate_walk_left_0", "walking_pirate_walk_left_1",
        "walking_pirate_walk_left_2", "walking_pirate_walk_left_3",
        "walking_pirate_walk_left_4", "walking_pirate_walk_left_5",
        "walking_pirate_walk_left_6", "walking_pirate_walk_left_7",
        "walking_pirate_fire_left_0", "walking_pirate_fire_left_1",
        "walking_pirate_fire_left_2", "walking_pirate_fire_left_3",
        "walking_pirate_fire_left_4", "walking_pirate_fire_left_5",
        "walking_pirate_look_left_0", "walking_pirate_look_left_1",
        "walking_pirate_look_left_2", "walking_pirate_look_shared",
        "walking_pirate_walk_right_0", "walking_pirate_walk_right_1",
        "walking_pirate_walk_right_2", "walking_pirate_walk_right_3",
        "walking_pirate_walk_right_4", "walking_pirate_walk_right_5",
        "walking_pirate_walk_right_6", "walking_pirate_walk_right_7",
        "walking_pirate_fire_right_0", "walking_pirate_fire_right_1",
        "walking_pirate_fire_right_2", "walking_pirate_fire_right_3",
        "walking_pirate_fire_right_4", "walking_pirate_fire_right_5",
        "walking_pirate_look_right_0", "walking_pirate_look_right_1",
        "walking_pirate_look_right_2",
    ];
        Suite(nameof(VerifyPirateArtworkNames), () => VerifyPirateArtworkNames(expected, PirateArtworkNameDefinitions.Walking));
    }

    /// <summary>Checks 18 wall-climb and firing-jump frame names.</summary>
    internal static void VerifyWallPirateArtworkNames()
    {
        string[] expected = [
        "wall_pirate_fire_jump_left_0", "wall_pirate_fire_jump_left_1",
        "wall_pirate_fire_jump_left_2", "wall_pirate_fire_jump_left_3",
        "wall_pirate_climb_left_0", "wall_pirate_climb_left_1",
        "wall_pirate_climb_left_2", "wall_pirate_climb_left_3",
        "wall_pirate_climb_left_4", "wall_pirate_fire_jump_right_0",
        "wall_pirate_fire_jump_right_1", "wall_pirate_fire_jump_right_2",
        "wall_pirate_fire_jump_right_3", "wall_pirate_climb_right_0",
        "wall_pirate_climb_right_1", "wall_pirate_climb_right_2",
        "wall_pirate_climb_right_3", "wall_pirate_climb_right_4",
    ];
        Suite(nameof(VerifyPirateArtworkNames), () => VerifyPirateArtworkNames(expected, PirateArtworkNameDefinitions.Wall));
    }

    /// <summary>Compares preserved published keys and rejects out-of-range frame indices.</summary>
    private static void VerifyPirateArtworkNames(string[] expected, Func<int, string> actual)
    {
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual(index), "published Pirate artwork key");
        foreach (int invalid in new[] { -1, expected.Length, int.MaxValue })
        {
            bool rejected = false;
            try { _ = actual(invalid); }
            catch (IndexOutOfRangeException) { rejected = true; }
            AssertTrue(rejected, "Pirate artwork key bounds");
        }
    }

    /// <summary>Runs both Pirate key lists and the extended-frame sequence audit.</summary>
    [VerificationCommand("--lookup-pirate-artwork-names")]
    internal static int LookupPirateArtworkNamesCommand()
    {
        Suite(nameof(VerifyWalkingPirateArtworkNames), () => VerifyWalkingPirateArtworkNames());
        Suite(nameof(VerifyWallPirateArtworkNames), () => VerifyWallPirateArtworkNames());
        Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
        Console.WriteLine("Pirate artwork names: all 55 published keys and bounds pass.");
        return 0;
    }
}

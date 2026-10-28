namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Cartridge values and helpers of <c>PirateArtworkNameDefinitions</c> that only verification reads.</summary>
internal static class PirateArtworkNameDefinitionsConstants
{
    /// <summary>Two flinch keys, left walk/fire/look, the shared look pose,
    /// then right walk/fire/look. Each facing has eight walk, six fire and
    /// three look ordinals. The shared pose has no facing or ordinal suffix.</summary>
    internal static string Walking(int index)
    {
        if ((uint)index >= EnemyExtendedFrameDefinitions.WalkingFrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 2) return index == 0 ? "walking_pirate_flinch_left" : "walking_pirate_flinch_right";
        if (index == 19) return "walking_pirate_look_shared";
        bool right = index >= 20;
        int pose = index - (right ? 20 : 2);
        string action = pose < 8 ? "walk" : pose < 14 ? "fire" : "look";
        int ordinal = pose < 8 ? pose : pose < 14 ? pose - 8 : pose - 14;
        return FormattableString.Invariant($"walking_pirate_{action}_{(right ? "right" : "left")}_{ordinal}");
    }
    /// <summary>Each facing has four fire/jump keys followed by five climb keys;
    /// all nine left-facing identities precede the nine right-facing identities.</summary>
    internal static string Wall(int index)
    {
        if ((uint)index >= EnemyExtendedFrameDefinitions.WallFrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        int pose = index % 9;
        string action = pose < 4 ? "fire_jump" : "climb";
        int ordinal = pose < 4 ? pose : pose - 4;
        return FormattableString.Invariant($"wall_pirate_{action}_{(index < 9 ? "left" : "right")}_{ordinal}");
    }
}

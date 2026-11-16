namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Cartridge values and helpers of <see cref="KraidPartFrameToolingDefinitions"/> that only verification reads.</summary>
internal static class KraidPartFrameToolingDefinitionsConstants
{
    /// <summary>The pose the extended spritemap <paramref name="spritemap"/> stores, or null when it stores none of Kraid's.</summary>
    internal static KraidPartPose? PoseAt(ushort spritemap)
    {
        foreach ((ushort stored, KraidPartPose pose) in KraidPartFrameToolingDefinitions.Records)
        {
            if (stored == spritemap)
                return pose;
        }
        return null;
    }
    /// <summary>The extended spritemaps of the foot's and arm's poses (#2210): every record of theirs the animations show.</summary>
    internal static IEnumerable<ushort> LimbSpritemaps => KraidPartFrameToolingDefinitions.Records.Select(record => record.Spritemap)
        .Where(spritemap => spritemap is >= KraidPartFrameToolingDefinitions.FootStart and <= KraidPartFrameToolingDefinitions.ArmLast or KraidPartFrameToolingDefinitions.FootStanding);
}

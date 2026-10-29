namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="DrawColours"/> members production does not use.</summary>
internal static class DrawColoursAccess
{
    /// <summary>Exposes <see cref="DrawColours"/> state that only verification reads.</summary>
    extension(DrawColours self)
    {
        /// <summary>
        /// The key a pixel inked in colour group <paramref name="inkGroup"/> of part group <paramref name="partGroup"/> takes.
        /// </summary>
        public int Key(ImageColourGroup inkGroup, PartColourGroup partGroup) =>
            self.FollowsPartGroups ? partGroup.Code : inkGroup.Number;
    }
}

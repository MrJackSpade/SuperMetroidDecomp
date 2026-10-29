using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Assets;

/// <summary>Fixture colourings held in a numbered sprite set, for checks whose subject's colours no owner names (#1464).</summary>
internal static class SpriteColouringFixtures
{
    /// <summary>Colourings a fixture or a native row names by number.</summary>
    extension(SpriteColouring)
    {
        /// <summary>
        /// A fixture's colouring held in sprite set <paramref name="set"/> in every room, whose owner the fixture does not
        /// name: an encounter's colours filling that whole set.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The set is not a sprite set.</exception>
        internal static SpriteColouring InFixtureSet(int set) => new EncounterSpriteColouring(
            new SpriteColourRun(Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set), 0, ColourSets.SetColours));
    }
}

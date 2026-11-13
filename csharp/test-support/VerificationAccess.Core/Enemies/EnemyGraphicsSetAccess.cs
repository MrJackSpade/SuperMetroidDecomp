using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification access at the native boundary (#1513, #1599): the import's graphics set a room's enemies draw under, and
/// where the import lays a set's members out among the sprite characters, for checks that compare against the cartridge's
/// own drawing. Gameplay never sees a set; it draws the variants installed for the room's population.
/// </summary>
internal static class EnemyGraphicsSetAccess
{
    /// <summary>The graphics set a room's enemies draw under.</summary>
    extension(SuperMetroid.Core.World.RoomEnemySystem self)
    {
        /// <summary>
        /// The import's graphics set the room's enemies draw under: the one its population's rooms load, or the session
        /// start's while the room has placed no enemies.
        /// </summary>
        internal RoomEnemyGraphicsSetDefinition GraphicsSet => FixturePopulationSets.SetOf(self.Presentation.DrawnPopulation);

        /// <summary>
        /// Draws the room's enemies and the objects they spawn under <paramref name="set"/>, as a room drawing under it would,
        /// without loading a population: from then on they draw in the variants of a population drawing under the set.
        /// </summary>
        internal void DrawUnder(RoomEnemyGraphicsSetDefinition set) => self.Presentation.DrawPopulation(FixturePopulationSets.Drawing(set));
    }

    /// <summary>Where a set's members land among the sprite characters.</summary>
    extension(RoomEnemyGraphicsSetDefinition self)
    {
        /// <summary>The import's layout of the set's members, in record order.</summary>
        internal IReadOnlyList<GraphicsSetMemberCharacters> Entries => GraphicsSetCharacterLayout.Of(FixtureGraphicsSetMembers.MembersOf(self)).Members;

        /// <summary>The set's member <paramref name="enemy"/>, where the import lays it out.</summary>
        /// <exception cref="InvalidDataException">The set does not load that enemy.</exception>
        internal GraphicsSetMemberCharacters Entry(EnemyDefinitionId enemy) =>
            self.Entries.Where(entry => entry.DefinitionId == enemy).Cast<GraphicsSetMemberCharacters?>().FirstOrDefault()
                ?? throw new InvalidDataException($"Enemy graphics set {self.Name} does not load {enemy}.");

        /// <summary>The character index the cartridge draws <paramref name="enemy"/> from under the set.</summary>
        internal int TileBaseOf(EnemyDefinitionId enemy) => GraphicsSetCharacterLayout.Of(FixtureGraphicsSetMembers.MembersOf(self)).TileBaseOf(enemy);

        /// <summary>
        /// The owner whose spawned-object sheet an object the cartridge draws from character index
        /// <paramref name="tileBase"/> takes its pictures from under the set: the shared sheet at character zero,
        /// otherwise the first member drawing from that index, or Mother Brain's tank glass at its characters.
        /// </summary>
        /// <exception cref="InvalidDataException">No member of the set draws from that index.</exception>
        internal SpawnedObjectOwner OwnerDrawingAt(int tileBase)
        {
            if (tileBase == 0)
                return SpawnedObjectOwner.Shared;
            GraphicsSetCharacterLayout layout = GraphicsSetCharacterLayout.Of(FixtureGraphicsSetMembers.MembersOf(self));
            foreach (GraphicsSetMemberCharacters member in layout.Members)
            {
                if (member.DefinitionId is { } enemy && layout.TileBaseOf(enemy) == tileBase)
                    return SpawnedObjectOwner.Of(enemy);
            }
            if (tileBase == EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters
                && layout.Members.Any(member => member.DefinitionId == EnemyDefinitionId.MotherBrainBody))
                return SpawnedObjectOwner.MotherBrainTankGlass;
            throw new InvalidDataException($"No member of graphics set {self.Name} draws from character {tileBase}.");
        }
    }

    extension(RoomEnemySlot self)
    {
        /// <summary>The character index the cartridge draws the slot's occupant from under <paramref name="enemies"/>' graphics set.</summary>
        internal int NativeTileBaseIn(SuperMetroid.Core.World.RoomEnemySystem enemies) => enemies.GraphicsSet.TileBaseOf(self.DefinitionId);
    }

    extension(SpriteGraphics self)
    {
        /// <summary>
        /// The character index the cartridge draws a sprite object with these graphics from under <paramref name="enemies"/>'
        /// graphics set: zero for the shared sheet, the spawner's index for its own, the tank glass's characters for its.
        /// </summary>
        internal int NativeTileBaseIn(SuperMetroid.Core.World.RoomEnemySystem enemies) =>
            self.Owner.IsMotherBrainTankGlass ? EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters
                : self.Owner.Spawner is { } spawner ? enemies.GraphicsSet.TileBaseOf(spawner) : 0;
    }
}

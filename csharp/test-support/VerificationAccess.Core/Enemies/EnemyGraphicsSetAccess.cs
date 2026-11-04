using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification access at the native boundary (#1513): where the import lays a loaded graphics set's members out among
/// the sprite characters, for checks that compare against the cartridge's own drawing. Gameplay never sees it.
/// </summary>
internal static class EnemyGraphicsSetAccess
{
    extension(EnemyGraphicsSet self)
    {
        /// <summary>The import's layout of the loaded set's members, in record order.</summary>
        internal IReadOnlyList<GraphicsSetMemberCharacters> Entries => GraphicsSetCharacterLayout.Of(self.LoadedSet).Members;

        /// <summary>The loaded set's member <paramref name="enemy"/>, where the import lays it out.</summary>
        /// <exception cref="InvalidDataException">The room's graphics set does not load that enemy.</exception>
        internal GraphicsSetMemberCharacters Entry(EnemyDefinitionId enemy) =>
            self.Entries.Where(entry => entry.DefinitionId == enemy).Cast<GraphicsSetMemberCharacters?>().FirstOrDefault()
                ?? throw new InvalidDataException($"Enemy graphics set {self.LoadedSet.Name} does not load {enemy}.");

        /// <summary>The character index the cartridge draws <paramref name="enemy"/> from under the loaded set.</summary>
        internal int TileBaseOf(EnemyDefinitionId enemy) => GraphicsSetCharacterLayout.Of(self.LoadedSet).TileBaseOf(enemy);

        /// <summary>
        /// The owner whose spawned-object sheet an object the cartridge draws from character index
        /// <paramref name="tileBase"/> takes its pictures from under the loaded set: the shared sheet at character zero,
        /// otherwise the first member drawing from that index, or Mother Brain's tank glass at its characters.
        /// </summary>
        /// <exception cref="InvalidDataException">No member of the loaded set draws from that index.</exception>
        internal SpawnedObjectOwner OwnerDrawingAt(int tileBase)
        {
            if (tileBase == 0)
                return SpawnedObjectOwner.Shared;
            GraphicsSetCharacterLayout layout = GraphicsSetCharacterLayout.Of(self.LoadedSet);
            foreach (GraphicsSetMemberCharacters member in layout.Members)
            {
                if (layout.TileBaseOf(member.DefinitionId) == tileBase)
                    return SpawnedObjectOwner.Of(member.DefinitionId);
            }
            if (tileBase == EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters
                && layout.Members.Any(member => member.DefinitionId == EnemyDefinitionId.MotherBrainBody))
                return SpawnedObjectOwner.MotherBrainTankGlass;
            throw new InvalidDataException($"No member of graphics set {self.LoadedSet.Name} draws from character {tileBase}.");
        }
    }

    extension(RoomEnemySlot self)
    {
        /// <summary>The character index the cartridge draws the slot's occupant from under <paramref name="enemies"/>' loaded set.</summary>
        internal int NativeTileBaseIn(SuperMetroid.Core.World.RoomEnemySystem enemies) => enemies.GraphicsSet.TileBaseOf(self.DefinitionId);
    }

    extension(SpriteGraphics self)
    {
        /// <summary>
        /// The character index the cartridge draws a sprite object with these graphics from under <paramref name="enemies"/>'
        /// loaded set: zero for the shared sheet, the spawner's index for its own, the tank glass's characters for its.
        /// </summary>
        internal int NativeTileBaseIn(SuperMetroid.Core.World.RoomEnemySystem enemies) =>
            self.Owner.IsMotherBrainTankGlass ? EnemyCharacterBaseDefinitions.MotherBrainTankGlassCharacters
                : self.Owner.Spawner is { } spawner ? enemies.GraphicsSet.TileBaseOf(spawner) : 0;
    }
}

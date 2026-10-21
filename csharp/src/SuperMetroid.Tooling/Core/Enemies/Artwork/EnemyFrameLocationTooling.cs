using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Development-tool views of where the cartridge stores an enemy frame, for oracles that compare installed frames with
/// cartridge data; never linked by player hosts.
/// </summary>
internal static class EnemyFrameLocationTooling
{
    /// <summary>The cartridge location of a frame.</summary>
    extension(EnemyFrameId frame)
    {
        /// <summary>The bank-local word the frame is stored at.</summary>
        internal ushort Pointer => EnemyFrameCartridgeLookupTooling.Word(frame);
    }

    /// <summary>The cartridge location of a composition's frame.</summary>
    extension(EnemySpritemapDefinition definition)
    {
        /// <summary>The bank-local word the composition's frame is stored at.</summary>
        internal ushort Pointer => EnemyFrameCartridgeLookupTooling.Word(definition.Frame);

        /// <summary>The enemy bank holding the composition's frame.</summary>
        internal byte Bank => EnemyFrameCartridgeLookupTooling.Bank(definition.Frame);
    }
}

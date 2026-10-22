using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The upload of one row of Mother Brain's rotting corpse, after the rot step it shows (#1392).</summary>
/// <param name="Transfer">The row's native transfer.</param>
/// <param name="Step">The rot steps the corpse had taken when the row was uploaded.</param>
public readonly record struct MotherBrainCorpseRotUpload(MotherBrainSpriteTileTransferRequest Transfer, int Step);

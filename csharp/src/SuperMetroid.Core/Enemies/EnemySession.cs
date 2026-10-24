using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy-side words that outlive each room, as the cartridge's WRAM does across enemy loading (#1329): the
/// elevator journey, the shared fade counter, Ceres's escape progress, the critical-energy drop bias, the language
/// flag, the random enemy counter, the death quota an empty population keeps, the staging surfaces and the Mode 7
/// transfer residue; and the host's installed enemy artwork, which every room's enemies draw from.
/// </summary>
public sealed class EnemySession
{
    /// <summary>The elevator journey words ($0E16, $0E18 and the travel direction) carried through an elevator door.</summary>
    internal ElevatorJourneyState Elevator { get; } = new();

    /// <summary>
    /// The shared <c>PaletteChangeNumerator</c>. Enemies and the runtime's door and death
    /// fades advance this one counter, as the cartridge's fades all advance one WRAM word.
    /// </summary>
    public GradualColorChangeCounter GradualColorChange { get; } = new();

    /// <summary>
    /// How far Ceres's escape has progressed, consumed by the Ceres doors and the elevator shaft. A fresh station load
    /// begins before Ridley escapes; Ceres Ridley publishes his escape and the self-destruct, and the elevator door's
    /// destruction publishes the last stage.
    /// </summary>
    public CeresEscapeStatus CeresStatus { get; set; }

    /// <summary>
    /// WRAM $7E:0E1E biases drops in the 30..49 energy grace band; room transitions
    /// retain it, and false is the cartridge's cleared initial value.
    /// </summary>
    internal bool CriticalEnergyDropBias { get; set; }

    /// <summary>Language flag sampled by $A6:C0D9 when the warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public bool JapaneseText { get; set; }

    /// <summary>
    /// Enemy-pass random counter read by native AI and projectile routines, 0 through 65535. Its one advance wraps
    /// from 65535 back to zero, and every reader takes its low bits or copies it whole, so a wider store holds exactly
    /// the counter the cartridge keeps.
    /// </summary>
    internal int RandomEnemyCounter { get; set; }

    /// <summary>Required room kill count from the nonempty population's terminator for enemy-clear triggers; native empty-population initialization retains the previous quota. 0 through 255.</summary>
    internal int DeathQuota { get; set; }

    /// <summary>The session's enemy staging surfaces; they outlive each room, as the cartridge's WRAM does.</summary>
    internal EnemyStagingSurfaces Staging { get; } = new();

    /// <summary>The session's Mode 7 transfer queue residue, which Ceres Ridley's getaway writes; the runtime binds it.</summary>
    internal Mode7TransferQueueResidue? FloorQueueResidue { get; set; }

    /// <summary>
    /// The gameplay OBJ character table's bindings, which enemy-format sprites draw from. Uploads that bypass the NMI
    /// queue (Draygon's Evir sheet, Crocomire's skeleton chunks) record themselves here; the runtime shares its table.
    /// </summary>
    internal ObjCharacterUploads ObjCharacters { get; } = new();

    /// <summary>
    /// Installed enemy presentation. Drawing or a named artwork transfer requires its corresponding catalog; null
    /// does not enable a cartridge-backed fallback. The host reinstalls it after a debugger restore.
    /// </summary>
    [field: NonSerialized]
    internal EnemyTileArtworkCatalog? TileArtwork { get; set; }

    /// <summary>The installed pre-coloured enemy sheets; artwork, never saved. The host reinstalls them after a restore.</summary>
    [field: NonSerialized]
    internal IEnemySheetSource? Sheets { get; set; }

    /// <summary>Shows <paramref name="source"/> over the sprite characters where the installed catalogue places it (#1406).</summary>
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    internal void ShowCharacters(ObjCharacterSource source) =>
        ObjCharacters.Record((Sheets ?? throw new InvalidOperationException("Showing an encounter's characters requires installed enemy sheets."))
            .Uploads.Upload(source));

}

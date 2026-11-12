using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The shared selection shades (<see cref="EnemySheetShades.Shared"/>) an enemy's draws never select, by enemy (#1643): an
/// enemy sheet installs only the looks its enemy is drawn in (#1513), so a look no draw of the enemy selects is not
/// installed on its sheets. A look such an enemy's sheets would take from its graphics set's colours, which differ from set
/// to set, would otherwise keep apart sheets whose pictures and shown looks are the same.
/// </summary>
internal static class EnemyUnselectedShadeDefinitions
{
    /// <summary>
    /// The shared looks each enemy's draws never select. The common enemy draw selects the frozen colours ($0C00) only while
    /// an enemy's freeze timer runs ($A0:94A9 to $A0:94B8), and the projectiles' colours ($0A00) only for an enemy whose
    /// palette bits are those (as $A0:8C3F gives an enemy its graphics set does not load); otherwise it draws in the enemy's
    /// own palette bits ($A0:94BD). A freeze timer starts only in the normal enemy shot reaction ($A0:A7F3, $A0:A899), the
    /// Metroid's ($A3:F02A), and the copies that linked parts of the Squeept, Geruta, Holtz, Dragon, Evir, Magdollite,
    /// Powamp and Ki-Hunters take from their own ($A2:BEEE, $A2:C293, $A2:C740, $A2:E802, $A8:8B35, $A8:B42F, $A8:C609,
    /// $A8:F71D). The Chozo statue's shot reaction returns at once ($AA:E7DC, its header's at $A0:F0FF), so it is never
    /// frozen; its initialization clears its palette bits ($AA:E74C) and nothing writes them again, so it never selects
    /// the projectiles' colours. The elevator's shot reaction also returns at once ($A3:804C, its header's at $A0:D73F), so
    /// it is never frozen.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, PictureShade[]> NeverSelected { get; } = new Dictionary<EnemyDefinitionId, PictureShade[]>
    {
        [EnemyDefinitionId.Chozo] = [EnemyShades.Frozen, EnemyShades.ProjectileLook],
        [EnemyDefinitionId.Elevator] = [EnemyShades.Frozen],
    };
}

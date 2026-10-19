namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Bounded instruction-data reads when Gravity Speed Booster inherits Screw Attack phases eight or ten. Phase ten's
/// grapple-handler code ($9B:C90A-$C929) is read from the player's cartridge into <see cref="SamusOverreadCatalog"/>.
/// </summary>
internal static class SpeedBoosterPaletteOverrunDefinitions
{
    /// <summary>
    /// Every colour of the expansion overrun: <c>$91:DD5B</c>'s unrolled <c>LDA $0000..$001E,X</c> runs under DB=$9B with
    /// X=$68AD, so it reads <c>$9B:68AD-$68CC</c> in the unconnected expansion window. Open bus returns the memory data
    /// register, which holds the operand's high byte, $00; all sixteen words are $0000.
    /// </summary>
    internal const ushort ExpansionOpenBusWord = 0x0000;
}

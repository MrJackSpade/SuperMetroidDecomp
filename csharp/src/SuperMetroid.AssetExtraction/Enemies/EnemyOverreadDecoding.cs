using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.Magdollite;
using static SuperMetroid.AssetExtraction.Enemies.EnemyOverreadSourceDefinitions;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Decodes what each enemy read past its authored data yields from the player's cartridge (#1500): it runs each read
/// the way the original game does, over the cartridge, for every game situation that can reach it, and keeps only the
/// result, keyed by that situation.
/// </summary>
internal static class EnemyOverreadDecoding
{
    /// <summary>Decodes every enemy over-read into the installed document.</summary>
    /// <exception cref="InvalidDataException">A read gives different answers for situations the game cannot tell apart.</exception>
    internal static EnemyOverreadDocument Decode(IImportCartridgeSource cartridge) =>
        new(EnemyOverreadFormat.Version, MagdolliteApexGrowthBySlot(cartridge), UnauthoredSparkLaunch(cartridge));

    /// <summary>
    /// Whether a Magdollite arm in each enemy slot grows its final segment at full height. The maximum-rise branch
    /// compares the rise with the threshold word the arm's slot offset selects, taking the sign of the sixteen-bit
    /// difference. The rise that first reaches full height lies from the full height to one fastest rising step past it,
    /// a step being the fastest linear speed's whole pixels plus one carried from the fraction; every such rise must give
    /// the slot's one answer.
    /// </summary>
    /// <exception cref="InvalidDataException">A slot's word gives different answers for rises an arm can reach full height with.</exception>
    private static bool[] MagdolliteApexGrowthBySlot(IImportCartridgeSource cartridge)
    {
        int fastestStep = EnemyLinearSpeedTable.Speed(EnemyLinearSpeedTable.RowCount - 1).WholePixels + 1;
        var grows = new bool[EnemySlotOrder.Capacity];
        for (int slot = 0; slot < grows.Length; slot++)
        {
            int threshold = cartridge.ReadCartridgeWord(MagdolliteArmHeightThresholds + slot * EnemyRecordBytes);
            bool growsAtFullHeight = CartridgeWordReads.SignedWord(MagdollitePhaseDefinitions.MaximumRise - threshold) >= 0;
            for (int rise = MagdollitePhaseDefinitions.MaximumRise; rise <= MagdollitePhaseDefinitions.MaximumRise + fastestStep; rise++)
            {
                if (CartridgeWordReads.SignedWord(rise - threshold) >= 0 != growsAtFullHeight)
                {
                    throw new InvalidDataException(
                        $"Magdollite's apex growth for slot {slot} differs between rises it can reach full height with.");
                }
            }
            grows[slot] = growsAtFullHeight;
        }
        return grows;
    }

    /// <summary>
    /// The drift of the falling spark's launch selection past its authored records: the whole and fractional words the
    /// selection's record offset reads after the last record.
    /// </summary>
    private static EnemyOverreadDocument.Drift UnauthoredSparkLaunch(IImportCartridgeSource cartridge)
    {
        int record = FallingSparkLaunchTable + FallingSparkLaunchDefinitions.Launches.Length * FallingSparkLaunchBytes;
        int whole = cartridge.ReadCartridgeWord(record);
        int fraction = cartridge.ReadCartridgeWord(record + sizeof(ushort));
        return new EnemyOverreadDocument.Drift(CartridgeWordReads.SignedWord(whole), fraction);
    }
}

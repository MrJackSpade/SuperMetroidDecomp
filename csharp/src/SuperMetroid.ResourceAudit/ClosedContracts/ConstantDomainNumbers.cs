using Microsoft.CodeAnalysis;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>
/// The number a constant argument selects in a reviewed index domain (#1481). A Samus pose's own value is arbitrary,
/// and the reviewed pose domains are pose numbers, so a pose constant selects by its number; any other constant is its
/// own number.
/// </summary>
internal static class ConstantDomainNumbers
{
    /// <summary>
    /// The domain number of <paramref name="value"/>, a constant of <paramref name="type"/>, or null when it is a pose
    /// constant that names no pose.
    /// </summary>
    internal static int? Of(ITypeSymbol? type, object value)
    {
        if (type?.ToDisplayString() != typeof(SamusPoseId).FullName) return Convert.ToInt32(value);
        var pose = (SamusPoseId)Convert.ToInt32(value);
        return Enum.IsDefined(pose) ? SamusPoseNumbers.Number(pose) : null;
    }
}

using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One bank-$84 collision PLM selected by the special-block tables at
/// <c>$94:9139-$92F7</c> when Samus touches Speed Booster terrain.
/// </summary>
public readonly record struct SpeedBoosterBlockPlmDefinition(
    ushort HeaderPointer,
    ushort InstructionPointer);

/// <summary>Typed catalog for all five retail Speed Booster collision-block variants.</summary>
public static class SpeedBoosterBlockPlmDefinitions
{
    /// <summary>Brinstar BTS <c>$82</c>: respawning, slower crumble.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition BrinstarSlowRespawning = new(
        RoomPlmHeaders.SpeedBlockBrinstarSlowRespawning,
        RoomPlmInstructionLists.SpeedBlockBrinstarSlowRespawning);

    /// <summary>Brinstar BTS <c>$83</c>: permanent, slower crumble.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition BrinstarSlowPermanent = new(
        RoomPlmHeaders.SpeedBlockBrinstarSlowPermanent,
        RoomPlmInstructionLists.SpeedBlockBrinstarSlowPermanent);

    /// <summary>Area-independent BTS <c>$0E</c>: standard respawning block.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition Respawning = new(
        RoomPlmHeaders.SpeedBlockRespawning,
        RoomPlmInstructionLists.SpeedBlockRespawning);

    /// <summary>Brinstar BTS <c>$84</c>: Dachora-room standard respawning block.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition DachoraRespawning = new(
        RoomPlmHeaders.SpeedBlockDachoraRespawning,
        RoomPlmInstructionLists.SpeedBlockDachoraRespawning);

    /// <summary>Area-independent BTS <c>$0F</c> and Brinstar BTS <c>$85</c>: permanent block.</summary>
    public static readonly SpeedBoosterBlockPlmDefinition Permanent = new(
        RoomPlmHeaders.SpeedBlockPermanent,
        RoomPlmInstructionLists.SpeedBlockPermanent);

    /// <summary>
    /// Resolves precisely the two area-independent and four Brinstar area-table entries
    /// that point at setup <c>$84:CDEA</c>. Other areas' negative BTS entries are unrelated.
    /// </summary>
    public static bool TryResolve(
        RoomBlockBehavior bts,
        AreaId area,
        out SpeedBoosterBlockPlmDefinition definition)
    {
        if (!bts.UsesAreaReactionTable)
        {
            if (bts.Value == 0x0e)
            {
                definition = Respawning;
                return true;
            }
            if (bts.Value == 0x0f)
            {
                definition = Permanent;
                return true;
            }
        }
        else if (area == AreaId.Brinstar)
        {
            definition = bts.AreaReactionIndex switch
            {
                2 => BrinstarSlowRespawning,
                3 => BrinstarSlowPermanent,
                4 => DachoraRespawning,
                5 => Permanent,
                _ => default,
            };
            return bts.AreaReactionIndex is >= 2 and <= 5;
        }

        definition = default;
        return false;
    }
}

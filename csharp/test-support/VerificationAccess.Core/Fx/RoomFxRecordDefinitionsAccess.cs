
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxRecordDefinitions"/> members production does not use.</summary>
internal static class RoomFxRecordDefinitionsAccess
{
    /// <summary>Enumerates selected room-FX records in ascending native pointer order without caching a second table.</summary>
    extension(RoomFxRecordDefinitions)
    {
        /// <summary>Enumerates every selected record in ascending native identity order without a stored cache.</summary>
        internal static IEnumerable<RoomFxRecordDefinition> All
        {
            get
            {
                for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                    if (((RoomFxRecordDefinition?)(RoomFxRecordDefinitions.SelectRecord((ushort)((ushort)pointer)))) is { } record)
                        yield return record;
            }
        }
    }
}

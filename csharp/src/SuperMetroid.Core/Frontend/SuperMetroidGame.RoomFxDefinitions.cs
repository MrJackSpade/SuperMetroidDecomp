namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private bool useCompiledRoomFxRecords;

    /// <summary>Opt into application-owned room-FX setup records for an installed game.</summary>
    public void BindCompiledRoomFxRecords(bool enabled)
    {
        useCompiledRoomFxRecords = enabled;
        if (runtime is not null) runtime.UseCompiledRoomFxRecords = enabled;
    }
}

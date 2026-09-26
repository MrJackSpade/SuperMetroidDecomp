namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Installed hosts use typed retail FX definitions; bare synthetic probes retain their bus data.</summary>
    [field: NonSerialized]
    public bool UseCompiledRoomFxRecords { get; set; }
}

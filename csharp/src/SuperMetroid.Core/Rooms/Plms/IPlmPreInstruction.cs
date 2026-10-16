namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The pre-instruction a PLM slot runs before its timer on each handler pass (native <c>$1CD7,x</c>, which
/// <c>PLM_Handler</c> calls through <c>JSR ($1CD7,x)</c>). A slot holds one shared behavior object; installs and clears
/// replace it, and "is it X?" compares against the shared instance.
/// </summary>
internal interface IPlmPreInstruction
{
    /// <summary>The bank-$84 routine this behavior ports: its identity in native PLM RAM.</summary>
    ushort NativeRoutine { get; }

    /// <summary>Runs the routine for <paramref name="slot"/> on this handler pass.</summary>
    /// <param name="slot">The slot whose pre-instruction this is.</param>
    /// <param name="context">The handler pass.</param>
    void Run(PlmSlot slot, PlmStepContext context);
}

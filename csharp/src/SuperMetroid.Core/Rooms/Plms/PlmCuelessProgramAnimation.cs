using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A PLM animation whose programs only draw, loop and delete: <see cref="PlmNoCue"/> has no cue to publish.</summary>
/// <typeparam name="TProgram">The family's program identity.</typeparam>
/// <typeparam name="TFrame">The family's draw identity.</typeparam>
/// <param name="program">The program to start.</param>
internal abstract class PlmCuelessProgramAnimation<TProgram, TFrame>(TProgram program)
    : PlmProgramAnimation<TProgram, TFrame, PlmNoCue>(program)
    where TProgram : struct, Enum
    where TFrame : struct
{
    /// <summary>Rejects a cue: <see cref="PlmNoCue"/> defines none, so a cueless program set cannot hold a cue step.</summary>
    [SuppressMessage("Design", "SME6426", Justification =
        "The cue contract passes the slot and step context every cue-publishing PLM family reads; PlmNoCue has no members, so this override is the one place a cueless family states that no cue exists.")]
    private protected sealed override TProgram? Publish(PlmNoCue cue, PlmSlot slot, PlmStepContext context) =>
        throw new InvalidOperationException($"{GetType().Name}'s programs publish no cues; got {cue}.");
}

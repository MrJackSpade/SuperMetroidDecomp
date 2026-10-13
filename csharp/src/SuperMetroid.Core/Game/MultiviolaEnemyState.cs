using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of Multiviola's four velocity words. Retail recomputes these words from the
/// population angle and speed every active frame; exposing the native layout makes the two
/// deliberately imperfect reflection negations visible in a debugger.
/// </summary>
public sealed class MultiviolaEnemyState
{
    /// <summary>The enemy slot whose C through F words hold the two velocity components.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds the typed velocity view to an existing native enemy slot.</summary>
    internal MultiviolaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Signed 16.16 velocity: native <c>$0FAC,x</c>/<c>$0FAE,x</c> hold the horizontal whole and
    /// fraction words, <c>$0FB0,x</c>/<c>$0FB2,x</c> the vertical ones.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableC, _slot.VariableD, _slot.VariableE, _slot.VariableF);
        internal set
        {
            _slot.VariableC = value.X.WholeWord;
            _slot.VariableD = value.X.Fraction;
            _slot.VariableE = value.Y.WholeWord;
            _slot.VariableF = value.Y.Fraction;
        }
    }
}

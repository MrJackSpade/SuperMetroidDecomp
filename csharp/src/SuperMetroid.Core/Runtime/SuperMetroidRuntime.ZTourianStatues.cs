using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    // Keep new members after the existing partial declarations: old binary debugger
    // fixtures contain compiler-generated closure identities numbered by member order.
    // The field migration handles this new owner without renumbering those old closures.
    private TourianStatueSequence? _tourianStatues;
    /// <summary>Room-owned native statue tile programs and HDMA descent.</summary>
    public TourianStatueSequence TourianStatues => _tourianStatues ??= new();
}

using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>The enemy system and its reflected instruction interpreter call the Shitroid program runs through (#633).</summary>
internal readonly record struct ShitroidInterpreter
{
    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Process.</summary>
    public required MethodInfo Process { get; init; }

    /// <summary>Process arguments.</summary>
    public required object?[] ProcessArguments { get; init; }
}

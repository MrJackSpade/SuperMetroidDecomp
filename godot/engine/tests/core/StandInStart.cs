using Engine.Content;
using Engine.Core;
using Engine.Input;

namespace Engine.Tests.Core;

/// <summary>Starting a game loop for a check that needs no assets.</summary>
public static class StandInStart
{
    /// <summary>Starts <paramref name="loop"/> with an assets folder nothing is read from.</summary>
    public static void StartStandIn(this GameLoop loop, GameDefinition definition, IInputSource source) =>
        loop.Start(definition, source, new ContentLibrary(Path.GetTempPath()));
}

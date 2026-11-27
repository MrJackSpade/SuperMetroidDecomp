using Engine.Core;
using Game.Compatibility;
using Godot;

namespace Game.Tests.Movies;

/// <summary>
/// The game as a movie's host begins it: the game's own definition, with the one thing a host of a recording
/// supplies in place of the game's own, the timing of the hardware the recording was made on. Its sound program
/// answers when the cartridge's answered (<see cref="CapturedSoundProgram"/>).
/// </summary>
public partial class MovieGame : SuperMetroid
{
    /// <summary>The sound program of the game last begun; null before one is.</summary>
    internal CapturedSoundProgram? Program { get; private set; }

    /// <summary>The definition <paramref name="game"/>, everything it was given kept, as a movie's host begins it.</summary>
    public static MovieGame Of(GameDefinition game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var movie = new MovieGame();
        foreach (Godot.Collections.Dictionary property in game.GetPropertyList())
        {
            var usage = (PropertyUsageFlags)property["usage"].AsInt64();
            if (!usage.HasFlag(PropertyUsageFlags.ScriptVariable) || !usage.HasFlag(PropertyUsageFlags.Storage))
                continue;
            StringName name = property["name"].AsStringName();
            movie.Set(name, game.Get(name));
        }

        return movie;
    }

    /// <inheritdoc/>
    public override ISoundFlow CreateSoundFlow(Engine.Content.ContentLibrary content)
    {
        Program = new CapturedSoundProgram(new SoundProgramClock(CartridgeSounds.Libraries));
        return new CartridgeSounds(content, Program);
    }
}

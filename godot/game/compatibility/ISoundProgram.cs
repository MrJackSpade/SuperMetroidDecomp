namespace Game.Compatibility;

/// <summary>
/// The cartridge's sound program, as the libraries' exchanges with it meet it (<see cref="SoundLibraryQueue"/>): each
/// library asks it for one sound at a time, or withdraws what it asked, and the program reports back what it has
/// taken up. When it takes something up is the timing of the hardware it ran on. The game's own is the program by
/// its clock (<see cref="SoundProgramClock"/>). A host that knows when a real one answered, as one playing a
/// recording of the cartridge does, gives the flow its own (<see cref="CartridgeSounds"/>), through the game's
/// definition (<see cref="SuperMetroid.CreateSoundFlow"/>).
/// </summary>
public interface ISoundProgram
{
    /// <summary>What <paramref name="library"/> reports back as taken up: a sound, or nothing.</summary>
    string? Reported(string library);

    /// <summary>Sets what <paramref name="library"/> is asked for now; nothing withdraws what was asked.</summary>
    void Ask(string library, string? sound);

    /// <summary>Loading music stops the program: what every library is asked for is dropped.</summary>
    void DropAsked();

    /// <summary>Runs the program through one update, once the update's turns have been taken.</summary>
    void Advance();
}

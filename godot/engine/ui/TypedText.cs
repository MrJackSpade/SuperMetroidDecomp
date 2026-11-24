using Engine.Core;
using Godot;

namespace Engine.Ui;

/// <summary>
/// Text shown one character at a time, with a sound for each: narration, captions, warnings, credits. It holds no
/// words of its own; it is given them.
/// </summary>
public partial class TypedText : PixelText
{
    /// <summary>How many updates pass between one character and the next.</summary>
    [Export]
    public int UpdatesPerCharacter { get; set; } = 2;

    /// <summary>The sound asked for as each character that is not a space appears, by name; empty for none.</summary>
    [Export]
    public string Sound { get; set; } = "";

    /// <summary>How many letters appear for each sound: 1 sounds every letter, 2 every second one.</summary>
    [Export]
    public int LettersPerSound { get; set; } = 1;

    private string _text = "";
    private int _letters;
    private int _shown;
    private int _wait;

    /// <summary>How many characters are showing.</summary>
    public int Shown => _shown;

    /// <summary>Whether all of the text is showing.</summary>
    public bool IsDone => _shown >= _text.Length;

    /// <summary>Starts typing <paramref name="text"/> from its first character.</summary>
    public void Type(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
        _shown = 0;
        _wait = 0;
        _letters = 0;
        Text = text;
        VisibleCharacters = 0;
    }

    /// <summary>Shows all of the text at once.</summary>
    public void Finish()
    {
        _shown = _text.Length;
        VisibleCharacters = _shown;
    }

    /// <summary>Plays one update: the next character appears when its wait is over, asking <paramref name="sounds"/> for its sound.</summary>
    public void Step(SoundRequests? sounds)
    {
        if (IsDone)
            return;
        if (_wait > 0)
        {
            _wait--;
            return;
        }

        char appeared = _text[_shown];
        _shown++;
        VisibleCharacters = _shown;
        _wait = Math.Max(0, UpdatesPerCharacter - 1);
        // Going on to the next line takes no time of its own.
        while (_shown < _text.Length && char.IsControl(_text[_shown]))
            _shown++;
        VisibleCharacters = _shown;
        if (char.IsWhiteSpace(appeared))
            return;
        _letters++;
        if (Sound.Length > 0 && _letters % Math.Max(1, LettersPerSound) == 0)
            sounds?.Request(Sound, channel: "text");
    }
}

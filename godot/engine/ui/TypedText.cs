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

    /// <summary>
    /// How many characters at the start are each followed by the next on the very next update, before the wait
    /// between characters begins to be kept.
    /// </summary>
    [Export]
    public int UndelayedCharacters { get; set; }

    private string _text = "";
    private int _letters;
    private int _typed;
    private int _shown;
    private int _wait;

    /// <summary>How many characters are showing.</summary>
    public int Shown => _shown;

    /// <summary>Whether all of the text is showing.</summary>
    public bool IsDone => _shown >= _text.Length;

    /// <summary>Whether all of the text is showing and the wait after its last character has passed: the next update would have typed another.</summary>
    public bool IsOver => IsDone && _wait == 0;

    /// <summary>Starts typing <paramref name="text"/> from its first character.</summary>
    public void Type(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
        _shown = 0;
        _wait = 0;
        _letters = 0;
        _typed = 0;
        Text = text;
        VisibleCharacters = 0;
    }

    /// <summary>Shows all of the text at once.</summary>
    public void Finish()
    {
        _shown = _text.Length;
        _wait = 0;
        VisibleCharacters = _shown;
    }

    /// <summary>Plays one update: the next character appears when its wait is over, asking <paramref name="sounds"/> for its sound.</summary>
    public void Step(SoundRequests? sounds)
    {
        if (_wait > 0)
        {
            _wait--;
            return;
        }

        if (IsDone)
            return;
        char appeared = _text[_shown];
        _shown++;
        VisibleCharacters = _shown;
        _wait = _typed < UndelayedCharacters ? 0 : Math.Max(0, UpdatesPerCharacter - 1);
        _typed++;
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

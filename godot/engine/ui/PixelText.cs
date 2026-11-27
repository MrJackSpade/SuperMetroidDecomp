using Godot;

namespace Engine.Ui;

/// <summary>
/// Words drawn in a <see cref="PixelFont"/>: each character in its own cell, line under line. It can show only the
/// first so many characters, for words that are typed out. It holds no words of its own; it is given them.
/// </summary>
/// <remarks>
/// A line ends at a line feed. Characters are counted as the text has them, line feeds included, so the count of
/// those showing is an index into the text. With no font it can hold words but cannot draw them.
/// </remarks>
public partial class PixelText : Control, IRecoloured
{
    private Recolouring? _recolouring;

    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<Shared.ColourSwap> swaps) => (_recolouring ??= new Recolouring(this)).Recolour(source, swaps);

    /// <inheritdoc/>
    public bool ShowsPicturesIn(string place) => _font is { } font && Recolouring.LiesIn(font.PicturePath, place);

    private PixelFont? _font;
    private string _text = "";
    private int _visibleCharacters = -1;
    private HorizontalAlignment _alignment = HorizontalAlignment.Left;

    /// <summary>The font the words are drawn in.</summary>
    public PixelFont? Font
    {
        get => _font;
        set
        {
            _font = value;
            Refresh();
        }
    }

    /// <summary>The words.</summary>
    public string Text
    {
        get => _text;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _text = value;
            Refresh();
        }
    }

    /// <summary>How many of the text's characters show, from its first; less than nought shows them all.</summary>
    public int VisibleCharacters
    {
        get => _visibleCharacters;
        set
        {
            _visibleCharacters = value;
            QueueRedraw();
        }
    }

    /// <summary>
    /// Where each line lies across the node's width: at its left, its middle or its right. A line in the middle
    /// lies on the grid of the font's cells, a half cell to the left when it cannot lie evenly.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => _alignment;
        set
        {
            _alignment = value;
            QueueRedraw();
        }
    }

    /// <summary>The words take no part in what the pointer does.</summary>
    public PixelText()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
    }

    /// <inheritdoc/>
    public override Vector2 _GetMinimumSize()
    {
        if (_font is null || _text.Length == 0)
            return Vector2.Zero;
        string[] lines = _text.Split((char)10);
        return new Vector2(lines.Max(line => line.Length) * _font.Cell.X, (lines.Length - 1) * _font.LineHeight + _font.Cell.Y);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_text.Length == 0)
            return;
        PixelFont font = _font ?? throw new InvalidOperationException($"The words \"{_text}\" are to be drawn, and \"{Name}\" was given no font.");
        int showing = _visibleCharacters < 0 ? _text.Length : Math.Min(_visibleCharacters, _text.Length);
        int index = 0;
        int row = 0;
        foreach (string line in _text.Split((char)10))
        {
            float left = _alignment switch
            {
                HorizontalAlignment.Center => ((int)Size.X / font.Cell.X - line.Length) / 2 * font.Cell.X,
                HorizontalAlignment.Right => Size.X - line.Length * font.Cell.X,
                _ => 0,
            };
            for (int column = 0; column < line.Length && index + column < showing; column++)
            {
                if (font.RegionOf(line[column]) is not { } region)
                    continue;
                var at = new Rect2(left + column * font.Cell.X, row * font.LineHeight, font.Cell.X, font.Cell.Y);
                DrawTextureRectRegion(font.Picture, at, region);
            }

            // The line feed that ended the line is a character too.
            index += line.Length + 1;
            row++;
        }
    }

    private void Refresh()
    {
        UpdateMinimumSize();
        QueueRedraw();
    }
}

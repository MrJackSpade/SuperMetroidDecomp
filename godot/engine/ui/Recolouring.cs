using Engine.Shared;
using Godot;

namespace Engine.Ui;

/// <summary>
/// The changed colours of one drawn thing: what each source has asked to be shown as what, and the shader that shows
/// it. A thing that can be recoloured keeps one and hands its own drawing to it. The shader swaps exact colours, so a
/// picture's shapes are untouched and nothing is drawn twice; with nothing asked for the thing has no shader at all.
/// A colour may be shown see-through (<see cref="ColourSwap.To"/>): its pixels are then drawn that much less opaque.
/// A colour may also be added to or taken from every colour the thing shows, after its swaps (<see cref="Shift"/>).
/// A swap may be of one region of a picture only: the shader reads the region of each pixel from the picture's map
/// of regions, which a picture that has one is drawn with (<see cref="Content.ContentLibrary.LoadTexture"/>).
/// The thing's rows may also be waved (<see cref="Wave"/>): each row reads its picture some pixels further on, by a
/// table of offsets gone round row by row, as the cartridge moves a layer's lines one by one. Only a thing drawn
/// from one picture can be waved so, and only one that is waved is drawn with the part of the shader that does it.
/// </summary>
/// <remarks>
/// Where two sources change the same colour, the one that asked last is shown, as the last to write a colour of a
/// palette is. A thing shows at most
/// <see cref="MostSwaps"/> changed colours at once.
/// </remarks>
public sealed class Recolouring
{
    /// <summary>The most colours one thing can show changed at once.</summary>
    public const int MostSwaps = 64;

    private const string Code = """
        shader_type canvas_item;
        uniform int swap_count = 0;
        uniform vec3 swap_from[64];
        uniform vec3 swap_to[64];
        uniform float swap_opacity[64];
        uniform int swap_region[64];
        uniform vec3 shift = vec3(0.0);
        varying vec4 tint;
        void vertex() {
            tint = COLOR;
        }
        void fragment() {
            vec4 shown = texture(TEXTURE, UV);
            // A picture's map of regions is drawn with it as its specular texture: a region is as far from white as
            // its number.
            int region = int(round((1.0 - texture(SPECULAR_SHININESS_TEXTURE, UV).r) * 255.0));
            for (int i = 0; i < 64; i++) {
                if (i >= swap_count) {
                    break;
                }
                if (swap_region[i] >= 0 && swap_region[i] != region) {
                    continue;
                }
                vec3 apart = abs(shown.rgb - swap_from[i]);
                if (max(apart.r, max(apart.g, apart.b)) < 0.002) {
                    shown.rgb = swap_to[i];
                    shown.a *= swap_opacity[i];
                    break;
                }
            }
            shown.rgb = clamp(shown.rgb + shift, 0.0, 1.0);
            COLOR = shown * tint;
        }
        """;

    /// <summary>
    /// What is put into the shader for a thing whose rows are waved: where the picture is read is moved, for the row
    /// of the thing the pixel lies on, by the table's entry for that row.
    /// </summary>
    private const string WaveHead = """
        uniform int wave_count = 0;
        uniform float wave_offsets[64];
        uniform float wave_phase = 0.0;
        uniform float wave_first = 0.0;
        uniform bool wave_down = false;
        vec2 waved(vec2 uv, vec2 own_place, vec2 pixel) {
            float row = floor(own_place.y);
            if (wave_count < 1 || row < wave_first) {
                return uv;
            }
            int entry = min(int(mod(wave_phase + row, float(wave_count))), wave_count - 1);
            float by = wave_offsets[entry];
            return wave_down ? uv + vec2(0.0, by * pixel.y) : uv + vec2(by * pixel.x, 0.0);
        }

        """;

    /// <summary>The most entries a wave's table can have.</summary>
    public const int MostWaveEntries = 64;

    /// <summary>The shader of a thing whose rows are waved, made from the one shader the first time it is wanted.</summary>
    private static string? _wavedCode;

    /// <summary>
    /// The one shader with the waving of rows put into it: its head before the shader's own, the thing's own place
    /// handed on from each corner, and both readings of the picture made through it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The shader no longer has one of the places the waving is put in at.</exception>
    private static string WavedCode()
    {
        if (_wavedCode is not null)
            return _wavedCode;
        string code = Code;
        foreach ((string from, string to) in new[]
        {
            ("varying vec4 tint;", WaveHead + "varying vec4 tint;\nvarying vec2 own;"),
            ("tint = COLOR;", "tint = COLOR;\n    own = VERTEX;"),
            ("texture(TEXTURE, UV)", "texture(TEXTURE, waved(UV, own, TEXTURE_PIXEL_SIZE))"),
            ("texture(SPECULAR_SHININESS_TEXTURE, UV)", "texture(SPECULAR_SHININESS_TEXTURE, waved(UV, own, TEXTURE_PIXEL_SIZE))"),
        })
        {
            int at = code.IndexOf(from, StringComparison.Ordinal);
            if (at < 0 || code.IndexOf(from, at + from.Length, StringComparison.Ordinal) >= 0)
                throw new InvalidOperationException($"The shader that changes colours has not exactly one \"{from}\" for the waving of rows to be put in at.");
            code = code.Replace(from, to, StringComparison.Ordinal);
        }

        return _wavedCode = code;
    }

    /// <summary>The wave of the thing's rows, or null for none.</summary>
    private RowWave? _wave;

    /// <summary>The table of the wave the material was last given, so that it is given a table only when the table changes.</summary>
    private IReadOnlyList<int>? _waveTableShown;

    /// <summary>Whether the material has the shader that waves rows.</summary>
    private bool _showsWaved;

    private Shader? _plain, _waved;

    /// <summary>
    /// Waves the thing's rows as <paramref name="wave"/> says, in place of whatever wave it had, or stops waving them
    /// for null. A wave by itself, with no colour changed, is drawn through the shader as a change of colours is.
    /// </summary>
    /// <exception cref="InvalidOperationException">The wave's table has more entries than a thing can be waved by.</exception>
    public void Wave(RowWave? wave)
    {
        if (wave == _wave)
            return;
        if (wave is { } given && (given.Offsets.Count < 1 || given.Offsets.Count > MostWaveEntries))
            throw new InvalidOperationException($"\"{_item.Name}\" is waved by a table of {given.Offsets.Count} entries; a table has 1 to {MostWaveEntries}.");
        _wave = wave;
        if (_material is null || !ReferenceEquals(_item.Material, _material) || _showsWaved != (wave is not null))
            Show();
        else if (wave is { } shown)
            ShowWave(_material, shown);
    }

    /// <summary>Hands the shader a wave.</summary>
    private void ShowWave(ShaderMaterial material, RowWave wave)
    {
        if (!ReferenceEquals(_waveTableShown, wave.Offsets))
        {
            var offsets = new float[MostWaveEntries];
            for (int index = 0; index < wave.Offsets.Count; index++)
                offsets[index] = wave.Offsets[index];
            material.SetShaderParameter("wave_count", wave.Offsets.Count);
            material.SetShaderParameter("wave_offsets", offsets);
            _waveTableShown = wave.Offsets;
        }

        material.SetShaderParameter("wave_phase", (float)wave.Phase);
        material.SetShaderParameter("wave_first", (float)wave.FirstRow);
        material.SetShaderParameter("wave_down", wave.Down);
    }

    /// <summary>What each source asked for, the source that asked last first.</summary>
    private readonly List<(string Source, IReadOnlyList<ColourSwap> Swaps)> _asked = [];

    /// <summary>What each source asked added to or taken from every colour.</summary>
    private readonly List<(string Source, ColourShift Shift)> _shifted = [];

    /// <summary>The thing drawn.</summary>
    private readonly CanvasItem _item;

    /// <summary>The material that shows the changes, made the first time any is asked for.</summary>
    private ShaderMaterial? _material;

    /// <summary>The changed colours of <paramref name="item"/>.</summary>
    public Recolouring(CanvasItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _item = item;
    }

    /// <summary>Whether the picture at <paramref name="picture"/> is <paramref name="place"/> or lies in the folder that names.</summary>
    public static bool LiesIn(string picture, string place) =>
        picture == place || picture.StartsWith(place + "/", StringComparison.Ordinal);

    /// <summary>
    /// Keeps <paramref name="swaps"/> as what <paramref name="source"/> asks for, in place of what it asked before, and
    /// shows the thing so. An empty list takes back all the source asked for.
    /// </summary>
    /// <exception cref="InvalidOperationException">More colours are changed at once than a thing can show.</exception>
    public void Recolour(string source, IReadOnlyList<ColourSwap> swaps)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(swaps);
        int at = _asked.FindIndex(asked => asked.Source == source);
        if (at < 0 && swaps.Count == 0)
            return;
        if (at >= 0)
            _asked.RemoveAt(at);

        // The shader shows the first change it finds of a colour, so the source asking now goes before the rest.
        if (swaps.Count > 0)
            _asked.Insert(0, (source, swaps));

        Show();
    }

    /// <summary>
    /// Keeps <paramref name="shift"/> as what <paramref name="source"/> asks added to or taken from every colour the
    /// thing shows, in place of what it asked before, and shows the thing so; <see cref="ColourShift.None"/> takes
    /// back what the source asked. What several sources ask is added together.
    /// </summary>
    public void Shift(string source, ColourShift shift)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        int at = _shifted.FindIndex(asked => asked.Source == source);
        if (at < 0 && shift.IsNone)
            return;
        if (at >= 0)
            _shifted.RemoveAt(at);
        if (!shift.IsNone)
            _shifted.Add((source, shift));
        Show();
    }

    /// <summary>Hands the shader every colour changed and what is added to them all, or takes the shader away when nothing is asked.</summary>
    private void Show()
    {
        if (_asked.Count == 0 && _shifted.Count == 0 && _wave is null)
        {
            if (ReferenceEquals(_item.Material, _material))
                _item.Material = null;
            return;
        }

        var from = new Vector3[MostSwaps];
        var to = new Vector3[MostSwaps];
        var opacity = new float[MostSwaps];
        var region = new int[MostSwaps];
        int count = 0;
        foreach ((string source, IReadOnlyList<ColourSwap> swaps) in _asked)
        {
            foreach (ColourSwap swap in swaps)
            {
                if (count == MostSwaps)
                    throw new InvalidOperationException($"\"{_item.Name}\" is asked to show more than {MostSwaps} colours changed at once; the last asked for is one of \"{source}\".");
                from[count] = new Vector3(swap.From.R, swap.From.G, swap.From.B);
                to[count] = new Vector3(swap.To.R, swap.To.G, swap.To.B);
                opacity[count] = swap.To.A;
                region[count] = swap.Region;
                count++;
            }
        }

        // A thing whose rows are waved is drawn with the shader that waves them, and any other with the one without.
        _material ??= new ShaderMaterial();
        _showsWaved = _wave is not null;
        _material.Shader = _showsWaved ? _waved ??= new Shader { Code = WavedCode() } : _plain ??= new Shader { Code = Code };
        _waveTableShown = null;
        if (_wave is { } wave)
            ShowWave(_material, wave);
        _material.SetShaderParameter("swap_count", count);
        _material.SetShaderParameter("swap_from", from);
        _material.SetShaderParameter("swap_to", to);
        _material.SetShaderParameter("swap_opacity", opacity);
        _material.SetShaderParameter("swap_region", region);
        _material.SetShaderParameter("shift", new Vector3(_shifted.Sum(asked => asked.Shift.Red), _shifted.Sum(asked => asked.Shift.Green), _shifted.Sum(asked => asked.Shift.Blue)));
        if (!ReferenceEquals(_item.Material, _material))
            _item.Material = _material;
    }
}

using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Ui;

/// <summary>
/// The colours of some pictures that stay lit while everything else is dark: a way of drawing that shows only those
/// colours of whatever is drawn with it and nothing of the rest. What stays lit under a cover of the whole screen is
/// drawn again over the cover with it, as a room's tiles are through a door's passage.
/// </summary>
/// <remarks>
/// Its data is a list of colours, each six hexadecimal digits as the pictures have it; one lit only in one region of
/// the pictures has the region's number after a stroke, as a picture's map of regions numbers it
/// (<see cref="ContentLibrary.LoadTexture"/>): <c>[ "deadbd", "ffffff/2" ]</c>. At most <see cref="Most"/> colours.
/// </remarks>
public sealed class LitColours
{
    /// <summary>The most colours one list can keep lit.</summary>
    public const int Most = 32;

    private const string Code = """
        shader_type canvas_item;
        uniform int lit_count = 0;
        uniform vec3 lit_colour[32];
        uniform int lit_region[32];
        varying vec4 tint;
        void vertex() {
            tint = COLOR;
        }
        void fragment() {
            vec4 shown = texture(TEXTURE, UV);
            // A picture's map of regions is drawn with it as its specular texture: a region is as far from white as
            // its number.
            int region = int(round((1.0 - texture(SPECULAR_SHININESS_TEXTURE, UV).r) * 255.0));
            bool lit = false;
            for (int i = 0; i < 32; i++) {
                if (i >= lit_count) {
                    break;
                }
                if (lit_region[i] >= 0 && lit_region[i] != region) {
                    continue;
                }
                vec3 apart = abs(shown.rgb - lit_colour[i]);
                if (max(apart.r, max(apart.g, apart.b)) < 0.002) {
                    lit = true;
                    break;
                }
            }
            COLOR = lit ? shown * tint : vec4(0.0);
        }
        """;

    private readonly (Color Colour, int Region)[] _lit;
    private ShaderMaterial? _material;

    private LitColours((Color Colour, int Region)[] lit) => _lit = lit;

    /// <summary>Reads a list of lit colours.</summary>
    public static LitColours Read(IReadOnlyList<DataNode> colours)
    {
        ArgumentNullException.ThrowIfNull(colours);
        if (colours.Count > Most)
            throw colours[Most].Problem($"is one colour more than the {Most} a list can keep lit");
        var lit = new (Color Colour, int Region)[colours.Count];
        for (int index = 0; index < colours.Count; index++)
        {
            string written = colours[index].AsText();
            int stroke = written.IndexOf('/');
            int region = ColourSwap.Everywhere;
            if (stroke >= 0 && (!int.TryParse(written[(stroke + 1)..], out region) || region < 0))
                throw colours[index].Problem($"names the region \"{written[(stroke + 1)..]}\", which is not a region's number");
            string hex = stroke < 0 ? written : written[..stroke];
            if (hex.Length != 6 || !hex.All(Uri.IsHexDigit))
                throw colours[index].Problem($"is \"{written}\", not a colour of six hexadecimal digits");
            lit[index] = (new Color(hex), region);
        }

        return new LitColours(lit);
    }

    /// <summary>The way of drawing that shows these colours alone, made the first time it is asked for.</summary>
    public ShaderMaterial Material()
    {
        if (_material is not null)
            return _material;
        var colours = new Vector3[Most];
        var regions = new int[Most];
        for (int index = 0; index < _lit.Length; index++)
        {
            colours[index] = new Vector3(_lit[index].Colour.R, _lit[index].Colour.G, _lit[index].Colour.B);
            regions[index] = _lit[index].Region;
        }

        _material = new ShaderMaterial { Shader = new Shader { Code = Code } };
        _material.SetShaderParameter("lit_count", _lit.Length);
        _material.SetShaderParameter("lit_colour", colours);
        _material.SetShaderParameter("lit_region", regions);
        return _material;
    }
}

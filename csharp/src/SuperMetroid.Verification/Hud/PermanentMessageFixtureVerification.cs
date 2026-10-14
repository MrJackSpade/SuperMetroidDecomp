using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Verification.Hud;

/// <summary>Builds a persistent message fixture with altered title, panel, and notice artwork.</summary>
internal static class PermanentMessageFixtureVerification
{
    /// <summary>Loads installed message documents, changes their border and sample content, and binds all three presentations.</summary>
    internal static GameplayMessageBoxState CreatePermanentMessageFixture()
    {
        string directory = RepositoryInstallation.Installation.MapDirectory;
        JsonNode Read(string name) => RequiredJson.Parse(File.ReadAllText(Path.Combine(directory, name)));
        static JsonObject Cell(int word) => new() { ["raw"] = word };
        static void SetBorder(JsonNode document)
        {
            var border = new JsonArray();
            for (int cell = 0; cell < 32; cell++) border.Add(Cell(0x3801));
            document["border"] = border;
        }
        static MemoryStream Stream(JsonNode document) => new(Encoding.UTF8.GetBytes(document.ToJsonString()));

        var titles = Read(GameplayMessageTitleDefinitions.FileName);
        SetBorder(titles);
        var energy = titles.Required("titles").Required(GameplayMessageIds.EnergyTank.ToString());
        energy["text"] = "TEST";
        energy["palette"] = 6;

        var panels = Read(GameplayMessagePanelDefinitions.FileName);
        SetBorder(panels);
        var template = new JsonArray();
        for (int cell = 0; cell < 128; cell++)
        {
            bool transparent = cell is < 32 and (< GameplayMessagePanelDefinitions.OuterLeftColumns or
                >= (32 - GameplayMessagePanelDefinitions.OuterRightColumns));
            template.Add(Cell(transparent ? GameplayMessageTitleDefinitions.TransparentWord : 0x3801));
        }
        panels.Required("panels").Required(GameplayMessageIds.MissileTank.ToString())["template"] = template;

        var notices = Read(GameplayMessageNoticeDefinitions.FileName);
        SetBorder(notices);
        var noticeTemplate = new JsonArray();
        for (int cell = 0; cell < 96; cell++) noticeTemplate.Add(Cell(0x3820 + cell / 32));
        notices.Required("notices").Required(GameplayMessageIds.MapDataAccessCompleted.ToString())["template"] = noticeTemplate;

        var message = new GameplayMessageBoxState();
        message.BindPresentation(GameplayMessageTitlePresentation.Load(Stream(titles)),
            GameplayMessagePanelPresentation.Load(Stream(panels)),
            GameplayMessageNoticePresentation.Load(Stream(notices)));
        return message;
    }
}

using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static GameplayMessageBoxState CreatePermanentMessageFixture()
    {
        string directory = RepositoryInstallation.Installation.MapDirectory;
        JsonNode Read(string name) => JsonNode.Parse(File.ReadAllText(Path.Combine(directory, name)))!;
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
        var energy = titles["titles"]![GameplayMessageIds.EnergyTank.ToString()]!;
        energy["text"] = "TEST";
        energy["palette"] = 6;

        var panels = Read(GameplayMessagePanelDefinitions.FileName);
        SetBorder(panels);
        var template = new JsonArray();
        for (int cell = 0; cell < 128; cell++)
        {
            bool transparent = cell < 32 && (cell < GameplayMessagePanelDefinitions.OuterLeftColumns ||
                cell >= 32 - GameplayMessagePanelDefinitions.OuterRightColumns);
            template.Add(Cell(transparent ? GameplayMessageTitleDefinitions.TransparentWord : 0x3801));
        }
        panels["panels"]![GameplayMessageIds.MissileTank.ToString()]!["template"] = template;

        var notices = Read(GameplayMessageNoticeDefinitions.FileName);
        SetBorder(notices);
        var noticeTemplate = new JsonArray();
        for (int cell = 0; cell < 96; cell++) noticeTemplate.Add(Cell(0x3820 + cell / 32));
        notices["notices"]![GameplayMessageIds.MapDataAccessCompleted.ToString()]!["template"] = noticeTemplate;

        var message = new GameplayMessageBoxState();
        message.BindPresentation(GameplayMessageTitlePresentation.Load(Stream(titles)),
            GameplayMessagePanelPresentation.Load(Stream(panels)),
            GameplayMessageNoticePresentation.Load(Stream(notices)));
        return message;
    }
}
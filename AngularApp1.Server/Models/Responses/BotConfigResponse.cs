namespace AngularApp1.Server.Models.Responses
{
    public class BotConfigResponse
    {
        public string Id { get; set; } = null!;
        public string AppName { get; set; } = null!;
        public string Config1 { get; set; } = null!;
        public string Config2 { get; set; } = null!;
        public string Config3 { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
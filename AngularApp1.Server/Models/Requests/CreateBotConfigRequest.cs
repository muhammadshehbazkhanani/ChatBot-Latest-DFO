namespace AngularApp1.Server.Models.Requests
{
    public class CreateBotConfigRequest
    {
        public string AppName { get; set; } = null!;
        public string Config1 { get; set; } = null!;
        public string Config2 { get; set; } = null!;
        public string Config3 { get; set; } = null!;
    }
}
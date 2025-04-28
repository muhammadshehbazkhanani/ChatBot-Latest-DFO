namespace AngularApp1.Server.Models.Responses
{
      public class DialogflowResponse
    {
        public string FulfillmentText { get; set; }
        public string IntentName { get; set; }
        public List<string> Messages { get; set; }
        public string ResultBranch { get; set; }
        public bool IsMultipleMessages { get; set; }
        public string Diagnostics { get; set; }
        public bool IsCustomPayloadRequest { get; set; } = false;
        public object CustomPayload { get; set; }
    }
}

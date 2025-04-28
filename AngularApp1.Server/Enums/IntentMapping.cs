namespace AngularApp1.Server.Enums
{
    public enum IntentType
    {
        StandardBotEndConversation,
        StandardBotMultipleMessages,
        StandardBotBranchOverride,
        OverrideIntent,
        Unknown
    }
    public class IntentMappings
    {
        public static readonly Dictionary<IntentType, string> BranchMap = new()
        {
            { IntentType.StandardBotEndConversation, "ReturnControlToScript" },
            { IntentType.StandardBotMultipleMessages, "PromptAndCollectNextResponse" },
            { IntentType.StandardBotBranchOverride, "ReturnControlToScript" },
            { IntentType.OverrideIntent, "ReturnControlToScript" },
            { IntentType.Unknown, "PromptAndCollectNextResponse" }
        };

        public static IntentType GetIntentType(string intentDisplayName)
        {
            return intentDisplayName switch
            {
                "StandardBotEndConversation" => IntentType.StandardBotEndConversation,
                "StandardBotMultipleMessages" => IntentType.StandardBotMultipleMessages,
                "StandardBotBranchOverride" => IntentType.StandardBotBranchOverride,
                "OverrideIntent" => IntentType.OverrideIntent,
                _ => IntentType.Unknown
            };
        }
    }
}

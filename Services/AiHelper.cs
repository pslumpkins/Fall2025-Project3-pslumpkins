using System.Text.Json;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using VaderSharp2;

namespace Fall2025_Project3_pslumpkins.Services;

    public class AiHelper
    {
        public static async Task<List<string>> GetList(AzureOpenAIClient client, string deployment, string systemPrompt, string userTopic, int count)
        {
            ChatClient chat = client.GetChatClient(deployment);
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage($$"""
                    Return exactly {{count}} items as strict JSON:
                    { "items": ["text1", "text2", "..."] }
                    Topic: {{userTopic}}
                    """)
            };

            var opts = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 800,
                Temperature = 0.8f
            };

            var response = chat.CompleteChat(messages, opts);

            var text = string.Concat(response.Value.Content.Select(p => p.Text));

            using var doc = JsonDocument.Parse(text);

            return doc.RootElement.GetProperty("items").EnumerateArray().Select(e => e.GetString() ?? "").ToList();
        }
    }


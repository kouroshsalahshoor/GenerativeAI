using Azure;
using Azure.AI.TextAnalytics;
using Azure.AI.Translation.Text;
using BethanysPieShop.Contracts.Repositories;
using BethanysPieShop.Contracts.Services;
using BethanysPieShop.Model;
using BethanysPieShop.Util;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace BethanysPieShop.Services
{
    public class TicketDataService : ITicketDataService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IPieRepository _pieRepository;
        private readonly OpenAIClient _openAIClient;
        private readonly TextAnalyticsClient _textAnalyticsClient;
        private readonly TextTranslationClient _textTranslationClient;
        private readonly IOptions<ModelSettings> _modelSettings;
        private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public TicketDataService(ITicketRepository TicketRepository, OpenAIClient openAIClient, IOptions<ModelSettings> modelSettings, IPieRepository pieRepository, TextTranslationClient textTranslationClient, TextAnalyticsClient textAnalyticsClient)
        {
            _ticketRepository = TicketRepository;
            _openAIClient = openAIClient;
            _modelSettings = modelSettings;
            _pieRepository = pieRepository;
            _textTranslationClient = textTranslationClient;
            _textAnalyticsClient = textAnalyticsClient;
        }

        public async Task<Ticket> AddMessageToTicket(int ticketId, TicketMessage ticketMessage)
        {
            Pie p = null;

            Ticket ticket = await _ticketRepository.GetTicketById(ticketId) ?? throw new Exception("Ticket not found");

            if (ticket.PieId != null)
            {
                p = await _pieRepository.GetPieById(ticket.PieId.Value);
            }

            await GetTicketMessageSentiment(ticketMessage);
            await GetTicketLanguage(ticketMessage);

            ticket.TicketMessages.Add(ticketMessage);

            ticket.LastModifiedDate = DateTime.Now;

            await SummarizeTicket(ticket, p);

            return await _ticketRepository.UpdateTicket(ticket);
        }

        public async Task<Ticket> AddTicket(Ticket ticket)
        {
            Pie p = null;

            if (ticket.PieId != null && ticket.PieId != 0)
            {
                p = await _pieRepository.GetPieById(ticket.PieId.Value);
            }

            ticket.CreatedDate = DateTime.Now;
            ticket.LastModifiedDate = DateTime.Now;

            await SummarizeTicket(ticket, p);

            await GetTicketMessageSentiment(ticket.TicketMessages.First());
            await GetTicketLanguage(ticket.TicketMessages.First());

            return await _ticketRepository.AddTicket(ticket);
        }

        public async Task DeleteTicket(int ticketId)
        {
            await _ticketRepository.DeleteTicket(ticketId);
        }

        public async Task<IEnumerable<Ticket>> GetAllTickets()
        {
            return await _ticketRepository.GetAllTickets();
        }

        public async Task<Ticket> GetTicketDetails(int ticketId)
        {
            return await _ticketRepository.GetTicketById(ticketId);
        }

        public async Task UpdateTicket(Ticket ticket)
        {
            await _ticketRepository.UpdateTicket(ticket);
        }

        public async Task<IEnumerable<Ticket>> GetTicketByCustomerId(string customerId)
        {
            return await _ticketRepository.GetTicketByCustomerId(customerId);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatus(TicketStatus ticketStatus)
        {
            return await _ticketRepository.GetTicketsByStatus(ticketStatus);
        }

        private async Task SummarizeTicket(Ticket ticket, Pie pie)
        {
            string[] satisfactionScores = ["Furious", "Unhappy", "Disappointed", "Indifferent", "Happy", "Delighted"];

            var prompt = $$"""
                You are assisting with a customer support ticketing system.
                Your task is to provide summaries of ticket messages to enable the support agent to quickly understand the context of the ticket.

                Below are the details of the current support ticket:

                Pie: {{pie?.Name ?? "Not specified"}}

                The ongoing message log is as follows:

                {{CombineTicketMessages(ticket.TicketMessages)}}

                Generate the following summaries:

                1. A concise summary, limited to 30 words, that captures as much essential information as possible. Avoid repeating the customer or product name, as this is already known. Specifically, highlight any particular questions or details mentioned, rather than just stating that questions or information were given. Summarize key points, such as pending questions or specific responses required, with a focus on the ticket’s current status and what kind of response from the next support agent would be most beneficial.

                2. A short, 8-word summary that acts as the ticket's title. The aim is to emphasize what's unique about this ticket.

                3. Lastly, write a 10-word summary of the most recent message from the CUSTOMER, disregarding any agent responses. Based solely on this, evaluate the customer's satisfaction level using one of these satisfaction scores:
                {{string.Join(", ", satisfactionScores)}}.
                Pay close attention to the customer's tone, as their emotional state is key to this assessment.

                The summaries will be for internal use by customer support agents only.

                Return the result as JSON in the following format: {
                  "summary": "string",
                  "title": "string",
                  "customerSatisfaction": "string"
                }
                Ensure that no markdown or HTML is included in your output.
                """;


            var messageList = new List<ChatMessage>
            {
                new SystemChatMessage(prompt)
            };

            var chatService = _openAIClient.GetChatClient(_modelSettings.Value.TextModelName);
            var chatResponse = await chatService.CompleteChatAsync(messageList);

            var responseText = chatResponse.Value.Content.First().Text;

            var deserializedResponse = DeserializeResponse<ChatResponse>(responseText, SerializerOptions)!;

            var ticketSummary = deserializedResponse.Summary;
            var ticketTitle = deserializedResponse.Title;

            int? sentimentScore = Array.FindIndex(satisfactionScores, score => score == (deserializedResponse.CustomerSatisfaction ?? string.Empty));

            if (sentimentScore == -1)
            {
                sentimentScore = null;
            }

            ticket.CustomerSentimentScore = sentimentScore;
            ticket.Title = ticketTitle;
            ticket.Summary = ticketSummary;
        }


        private static TOutput? DeserializeResponse<TOutput>(string jsonContent, JsonSerializerOptions settings)
        {
            var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);
            var jsonReader = new Utf8JsonReader(jsonBytes.AsSpan());
            return JsonSerializer.Deserialize<TOutput>(ref jsonReader, settings);
        }

        private string CombineTicketMessages(List<TicketMessage> messages)
        {
            var stringBuilder = new StringBuilder();

            foreach (var ticket in messages)
            {
                var role = ticket.IsSupportMessage ? "support" : "customer";
                stringBuilder.AppendLine($"<message role=\"{role}\">{ticket.Message}</message>");
            }

            return stringBuilder.ToString();
        }

        private async Task GetTicketLanguage(TicketMessage ticketMessage)
        {
            Azure.AI.TextAnalytics.DetectedLanguage detectedLanguage = _textAnalyticsClient.DetectLanguage(ticketMessage.Message);
            ticketMessage.Language = detectedLanguage.Iso6391Name;

            string targetLanguage = "en";

            if (detectedLanguage.Iso6391Name != targetLanguage)
            {
                Response<IReadOnlyList<TranslatedTextItem>> response = await _textTranslationClient.TranslateAsync(targetLanguage, ticketMessage.Message);
                IReadOnlyList<TranslatedTextItem> translations = response.Value;
                TranslatedTextItem translation = translations.FirstOrDefault();
                ticketMessage.MessageEn = translation?.Translations?.FirstOrDefault()?.Text;
            }
        }

        private async Task GetTicketMessageSentiment(TicketMessage ticketMessage)
        {
            Response<DocumentSentiment> response = await _textAnalyticsClient.AnalyzeSentimentAsync(ticketMessage.Message, options: new AnalyzeSentimentOptions());
            DocumentSentiment sentiment = response.Value;

            if ((sentiment.ConfidenceScores.Positive >= sentiment.ConfidenceScores.Negative) && (sentiment.ConfidenceScores.Positive >= sentiment.ConfidenceScores.Neutral))
            {
                ticketMessage.TicketMessageSentiment = TicketMessageSentiment.Positive;
            }
            else if (sentiment.ConfidenceScores.Negative >= sentiment.ConfidenceScores.Positive && sentiment.ConfidenceScores.Negative >= sentiment.ConfidenceScores.Neutral)
            {
                ticketMessage.TicketMessageSentiment = TicketMessageSentiment.Negative;
            }
            else
            {
                ticketMessage.TicketMessageSentiment = TicketMessageSentiment.Neutral;
            }
        }

        private class ChatResponse
        {
            public required string Summary { get; set; }
            public required string Title { get; set; }
            public string? CustomerSatisfaction { get; set; }
        }
    }
}
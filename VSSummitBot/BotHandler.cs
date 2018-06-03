using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace VSSummitBot
{
    public class BotHandler : IBot
    {
        public async Task OnTurn(ITurnContext turnContext)
        {
            await this.ShowWelcomeMessage(turnContext);
            await this.Resolve(turnContext);
        }

        public async Task Resolve(ITurnContext context)
        {
            var luisResult = context.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
            if (luisResult != null)
            {
                (string topIntent, _) = luisResult.GetTopScoringIntent();
                var intentsResult = new List<string>();
                foreach (var intent in luisResult.Intents)
                {
                    var intentScore = (double)intent.Value["score"];
                    intentsResult.Add($"* '{intent.Key}', score {intentScore}");
                }
                await context.SendActivity(string.Join("\n\n", intentsResult));
            }
        }

        private async Task ShowWelcomeMessage(ITurnContext turnContext)
        {
            if (turnContext.Activity is IConversationUpdateActivity conversationUpdated)
            {
                var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl));
                foreach (var member in conversationUpdated.MembersAdded ?? Array.Empty<ChannelAccount>())
                {
                    if (member.Id == conversationUpdated.Recipient.Id)
                    {
                        var reply = turnContext.Activity.CreateReply("Olá, seja bem vindo!");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
            }
        }
    }
}

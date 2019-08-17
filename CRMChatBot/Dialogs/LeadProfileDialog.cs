using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace CRMChatBot.Dialogs
{
    public class LeadProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<LeadProfile> _leadProfileAccessor;

        public LeadProfileDialog(UserState userState)
            : base(nameof(LeadProfileDialog))
        {
            _leadProfileAccessor = userState.CreateProperty<LeadProfile>("LeadProfile");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                TypeStepAsync,
                FNameStepAsync,
                LNameStepAsync,
                TopicStepAsync,
                EmailStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> TypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter type number"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Item based", "Service-maintenance Based" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> FNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["type"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your first name.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> LNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["fname"] = (string)stepContext.Result;
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your last name.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> TopicStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["lname"] = (string)stepContext.Result;
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter topic name.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["topic"] = (string)stepContext.Result;
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your email.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;

            // Get the current profile object from user state.
            var leadProfile = await _leadProfileAccessor.GetAsync(stepContext.Context, () => new LeadProfile(), cancellationToken);

            leadProfile.Type = (string)stepContext.Values["type"];
            leadProfile.FName = (string)stepContext.Values["fname"];
            leadProfile.LName = (string)stepContext.Values["lname"];
            leadProfile.Topic = (string)stepContext.Values["topic"];
            leadProfile.Email = (string)stepContext.Values["email"];

            var msg = $@"Your data is as follow: 
                        {Environment.NewLine} Your Topic is: {leadProfile.Topic} 
                        {Environment.NewLine} Your Type is: {leadProfile.Type} 
                        {Environment.NewLine} Your Name is: {leadProfile.FName} {leadProfile.LName} 
                        {Environment.NewLine} Your Email is: {leadProfile.Email}.";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmptyBot1
{
    public class ChoiceDialog : ComponentDialog
    {
        internal string userName;
        SchedulerViewModel viewModel;
        List<Choice> choices;
        MainDialog mainDialog;

        public ChoiceDialog(string name, SchedulerViewModel vm, List<Choice> options, MainDialog md)
        {
            this.userName = name;
            this.viewModel = vm;
            this.choices = options;
            this.mainDialog = md;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ChoiceEchoStepAsync,
                FinalStepAsync
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        }



        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = MessageFactory.Text("What else can i do for you " + "" + userName + "?", InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Prompt = promptMessage, Choices = choices }, cancellationToken);
        }

        private async Task<DialogTurnResult> ChoiceEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            mainDialog.processChoiceDialog.userName = this.userName;
            if (this.mainDialog.processChoiceDialog == null)
            {
                this.mainDialog.processChoiceDialog = new ProcessChoiceDialog("", this.viewModel, this.choices, this.mainDialog);
            }
            AddDialog(this.mainDialog.processChoiceDialog);
            return await stepContext.BeginDialogAsync(nameof(ProcessChoiceDialog), (stepContext.Result as FoundChoice).Value.ToLower(), cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            //await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
            return await stepContext.EndDialogAsync();
        }

    }
}

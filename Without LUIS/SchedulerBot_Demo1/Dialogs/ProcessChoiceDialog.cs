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
    public class ProcessChoiceDialog : ComponentDialog
    {
        internal string userName;
        SchedulerViewModel viewModel;
        List<Choice> choices;
        MainDialog mainDialog;
        string choosenOption = "";

        public ProcessChoiceDialog(string name, SchedulerViewModel vm, List<Choice> options, MainDialog md)
        {
            this.userName = name;
            this.viewModel = vm;
            this.choices = options;
            this.mainDialog = md;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                DateTimeEchoStepAsync,
                FinalStepAsync
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            if(md.viewAppointmentDialog == null)
            {
                md.viewAppointmentDialog = new ViewAppointmentDialog("", this.viewModel, this.mainDialog, this.choices);
            }
            AddDialog(md.viewAppointmentDialog);

            if (md.addAppointmentDialog == null)
            {
                md.addAppointmentDialog = new AddAppointmentDialog("", this.viewModel, this.mainDialog, this.choices);
            }
            AddDialog(md.addAppointmentDialog);

        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            if (!string.IsNullOrEmpty(stepContext.Options.ToString()) && stepContext.Options as string == "view")
            {
                var promptMessage = MessageFactory.Text("Enter the date in (mm/dd/yyyy) format to view your scheduled appointments.", InputHints.ExpectingInput);
                choosenOption = "view";
                return await stepContext.PromptAsync(nameof(DateTimePrompt), new PromptOptions { Prompt = promptMessage, }, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(stepContext.Options.ToString()) && (stepContext.Options as string).ToLower() == "exit")
            {
                choosenOption = "no thnaks";
                return await stepContext.EndDialogAsync(stepContext.Options);
            }
            else if (!string.IsNullOrEmpty(stepContext.Options.ToString()) && stepContext.Options as string == "add")
            {
                var promptMessage = MessageFactory.Text("Enter the date in (mm/dd/yyyy) format to add an appointment.", InputHints.ExpectingInput);
                choosenOption = "add";
                return await stepContext.PromptAsync(nameof(DateTimePrompt), new PromptOptions { Prompt = promptMessage, }, cancellationToken);

            }
            else if (!string.IsNullOrEmpty(stepContext.Options.ToString()) && stepContext.Options as string == "cancel")
            {
                var promptMessage = MessageFactory.Text("Enter the date of the appointment to be cancelled, in (mm/dd/yyyy) format.", InputHints.ExpectingInput);
                choosenOption = "cancel";
                return await stepContext.PromptAsync(nameof(DateTimePrompt), new PromptOptions { Prompt = promptMessage, }, cancellationToken);

            }

            else
            {
                await stepContext.Context.SendActivityAsync("Sorry " + userName + " that operation is not yet supported");
                this.mainDialog.choiceDialog.userName = this.userName;
                return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), stepContext.Options.ToString().ToLower(), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> DateTimeEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var val = ((stepContext.Result as List<DateTimeResolution>)[0] as DateTimeResolution).Value;
            char[] split = new char[] { '-' };
            var date = val.Split(split)[2];

            if (choosenOption == "view")
            {
                return await stepContext.BeginDialogAsync(nameof(ViewAppointmentDialog), date, cancellationToken);
            }
            else if(choosenOption == "add")
            {
                return await stepContext.BeginDialogAsync(nameof(AddAppointmentDialog), date, cancellationToken);
            }
            else if (choosenOption == "cancel")
            {
                return await stepContext.BeginDialogAsync(nameof(CancelAppointmentDialog), date, cancellationToken);
            }
            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            //await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
            return await stepContext.EndDialogAsync();
        }

    }
}

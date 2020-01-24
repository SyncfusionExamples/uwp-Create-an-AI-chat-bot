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
    public class CancelAppointmentDialog : ComponentDialog
    {
        internal string userName;
        SchedulerViewModel viewModel;
        List<Choice> choices;
        MainDialog mainDialog;
        List<Meeting> appointmentList;
        string appointmentTitle = "";
        int appointmentFromTime;
        int appointmentToTime;

        public CancelAppointmentDialog(string name, SchedulerViewModel vm, MainDialog md, List<Choice> options)
        {
            this.userName = name;
            this.viewModel = vm;
            this.mainDialog = md;
            this.choices = options;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                TitleEchoStepAsync,
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            if (this.mainDialog.choiceDialog == null)
            {
                this.mainDialog.choiceDialog = new ChoiceDialog("", this.viewModel, this.choices, this.mainDialog);
            }
            AddDialog(this.mainDialog.choiceDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            appointmentList = this.viewModel.Meetings.Where(x => x.From.Day.ToString() == stepContext.Options.ToString()).ToList<Meeting>();

            if (appointmentList.Count() > 0)
            {
                await stepContext.Context.SendActivityAsync("You have " + appointmentList.Count().ToString() + (appointmentList.Count().ToString().ToLower() == "one" ? " appointment." : " appointments."));
                int i = 1;
                foreach (var v in appointmentList)
                {
                    await stepContext.Context.SendActivityAsync(i + "." + v.EventName + " from " + (v.From.Hour % 12 == 0 ? 12 : v.From.Hour % 12) + (v.From.Hour < 12 ? " AM" : " PM") + " to " + (v.To.Hour % 12 == 0 ? 12 : v.To.Hour % 12) + (v.To.Hour < 12 ? " AM" : " PM"));
                    i++;
                }

                var promptMessage = MessageFactory.Text("Which appointment would you like to cancel ?", InputHints.ExpectingInput);
                this.mainDialog.choiceDialog.userName = this.userName;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Sorry " + userName + " there are no appointments on the specified date");
                this.mainDialog.choiceDialog.userName = this.userName;
                return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> TitleEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int index = Convert.ToInt32(stepContext.Result);
            var actualAppointment = appointmentList[index - 1];
            this.viewModel.Meetings.Remove(actualAppointment);
            this.mainDialog.choiceDialog.userName = this.userName;

            await stepContext.Context.SendActivityAsync("The appointment " + actualAppointment.EventName + " on " + actualAppointment.From.Date.Day + " th January from " +
            (actualAppointment.From.Hour % 12 == 0 ? 12 : actualAppointment.From.Hour % 12) + (actualAppointment.From.Hour < 12 ? " AM" : " PM") + " to " +
            (actualAppointment.To.Hour % 12 == 0 ? 12 : actualAppointment.To.Hour % 12) + (actualAppointment.To.Hour < 12 ? " AM" : " PM") + " has been cancelled");
            return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), null, cancellationToken);
        }
    }
}

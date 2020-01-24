using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmptyBot1
{
    public class ViewAppointmentDialog : ComponentDialog
    {
        internal string userName;
        SchedulerViewModel viewModel;
        List<Choice> choices;
        MainDialog mainDialog;

        public ViewAppointmentDialog(string name, SchedulerViewModel vm, MainDialog md, List<Choice> options)
        {
            this.userName = name;
            this.viewModel = vm;
            this.mainDialog = md;
            this.choices = options;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
            }));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            if(this.mainDialog.choiceDialog == null)
            {
                this.mainDialog.choiceDialog = new ChoiceDialog("", this.viewModel, this.choices, this.mainDialog);
            }
            AddDialog(this.mainDialog.choiceDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentList = this.viewModel.Meetings.Where(x => x.From.Day.ToString() == stepContext.Options.ToString());

            if (appointmentList.Count() > 0)
            {
                await stepContext.Context.SendActivityAsync("You have " + appointmentList.Count().ToString() + (appointmentList.Count().ToString().ToLower() == "one" ? " appointment." : " appointments."));
                int i = 1;
                foreach (var v in appointmentList)
                {
                    await stepContext.Context.SendActivityAsync(i + "." + v.EventName + " from " + (v.From.Hour % 12 == 0 ? 12 : v.From.Hour % 12) + (v.From.Hour < 12 ? " AM" : " PM") + " to " + (v.To.Hour % 12 == 0 ? 12 : v.To.Hour % 12) + (v.To.Hour < 12 ? " AM" : " PM"));
                    i++;
                }
                i = 1;

                this.mainDialog.choiceDialog.userName = this.userName;
                return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Sorry " + userName + " there are no appointments on the specified date");
                this.mainDialog.choiceDialog.userName = this.userName;
                return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), null, cancellationToken);
            }
        }
    }
}
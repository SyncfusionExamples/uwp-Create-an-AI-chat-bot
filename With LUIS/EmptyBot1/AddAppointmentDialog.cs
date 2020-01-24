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
    public class AddAppointmentDialog : ComponentDialog
    {
        internal string userName;
        SchedulerViewModel viewModel;
        List<Choice> choices;
        MainDialog mainDialog;
        int appointmentDate;
        string appointmentTitle = "";
        int appointmentFromTime;
        int appointmentToTime;

        public AddAppointmentDialog(string name, SchedulerViewModel vm, MainDialog md, List<Choice> options)
        {
            this.userName = name;
            this.viewModel = vm;
            this.mainDialog = md;
            this.choices = options;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                TitleEchoStepAsync,
                FromTimeEchoStepAsync,
                AddAppointmentStepAsync
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
            if(stepContext.Options as object[] != null)
            {
                var data = stepContext.Options as object[];
                appointmentTitle = data[3].ToString();
                appointmentDate = Convert.ToInt32(data[2]);
                appointmentFromTime = Convert.ToInt32(data[0]);
                appointmentToTime = Convert.ToInt32(data[1]);
                viewModel.Meetings.Add(new Meeting()
                {
                    EventName = appointmentTitle,
                    From = new DateTime(2020, 01, appointmentDate, appointmentFromTime, 0, 0),
                    To = new DateTime(2020, 01, appointmentDate, appointmentToTime, 0, 0),
                    Color = System.Drawing.Color.Red
                });

              
                await stepContext.Context.SendActivityAsync("New appointment added");
                await stepContext.Context.SendActivityAsync(appointmentTitle + " on " + appointmentDate + " th January from " +
                (appointmentFromTime % 12 == 0 ? 12 : appointmentFromTime % 12) + (appointmentFromTime < 12 ? " AM" : " PM") + " to " +
                (appointmentToTime % 12 == 0 ? 12 : appointmentToTime % 12) + (appointmentToTime < 12 ? " AM" : " PM"));
                return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), null, cancellationToken);

            }

            appointmentDate = Convert.ToInt32(stepContext.Options);

            var promptMessage = MessageFactory.Text("What is the title of the new appointment ?", "What is the title of the new appointment", InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> TitleEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            appointmentTitle = (string)stepContext.Result;
            if (!string.IsNullOrEmpty(appointmentTitle))
            {
                var promptMessage = MessageFactory.Text("What time is the new appointment ?", "What time is the new appointment", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FromTimeEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var val = (string)stepContext.Result;
            string[] split = new string[] { "AM", "PM" };
            var time = val.Split(split, StringSplitOptions.None)[0];
            appointmentFromTime = Convert.ToInt32(time);
            if (!string.IsNullOrEmpty(appointmentTitle))
            {
                var promptMessage = MessageFactory.Text("What is the duration of the new appointment ?", "What is the duration of the new appointment", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AddAppointmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var val = (string)stepContext.Result;
            string[] split = new string[] { "hours", "hour"};
            var time = val.Split(split, StringSplitOptions.None)[0];
            appointmentToTime = Convert.ToInt32(time) + appointmentFromTime;

            this.AddAppointment();

            await stepContext.Context.SendActivityAsync("New appointment added");
            await stepContext.Context.SendActivityAsync(appointmentTitle + " on " + appointmentDate + " th January from " +
            (appointmentFromTime % 12 == 0 ? 12 : appointmentFromTime % 12) + (appointmentFromTime < 12 ? " AM" : " PM") + " to " +
            (appointmentToTime % 12 == 0 ? 12 : appointmentToTime % 12) + (appointmentToTime < 12 ? " AM" : " PM"));
            return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), null, cancellationToken);
        }

        private void AddAppointment()
        {
            viewModel.Meetings.Add(new Meeting()
            {
                EventName = this.appointmentTitle,
                From = new DateTime(2020,01,appointmentDate,appointmentFromTime,0,0),
                To = new DateTime(2020, 01, appointmentDate, appointmentToTime, 0, 0),
                Color = System.Drawing.Color.Red
            });
        }
    }
}

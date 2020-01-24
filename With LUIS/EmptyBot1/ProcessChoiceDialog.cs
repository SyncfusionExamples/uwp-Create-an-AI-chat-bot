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
            if (md.viewAppointmentDialog == null)
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
            var luisResults = await this.mainDialog._luisRecognizer.RecognizeAsync<Appointment>(stepContext.Context, cancellationToken);

            string date = "";
            if (luisResults != null && luisResults.TopIntent().intent == Intent.Showappoitments)
            {
                var luisResult = stepContext.Options as Appointment;
                choosenOption = "view";
                var appointment = new AppointmentModel();
                if (luisResult.Entities.datetime.Count() > 0)
                {
                    appointment.Date = luisResult.Entities.datetime[0].Expressions[0];
                    char[] split = new char[] { '-' };
                    date = appointment.Date.Split(split)[2];
                }

                return await stepContext.BeginDialogAsync(nameof(ViewAppointmentDialog), date, cancellationToken);
            }
            else if(luisResults != null && luisResults.TopIntent().intent == Intent.AddAppointments)
            {
                var luisResult = stepContext.Options as Appointment;
                if(luisResult == null)
                {
                    luisResult = luisResults;
                }
                choosenOption = "view";
                var appointment = new AppointmentModel();
                if (luisResult.Entities.datetime.Count() > 0)
                {
                    appointment.Date = luisResult.Entities.datetime[0].Expressions[0];
                    char[] split = new char[] { ',' };
                    string date1 = appointment.Date.Split(split)[0];
                    string date2 = appointment.Date.Split(split)[1];
                    char[] splitTime = new char[] { 'T' };
                    string time1 = date1.Split(splitTime)[1];
                    string time2 = date2.Split(splitTime)[1];

                    string temp = date1.Split(splitTime)[0];
                    char[] splitDate = new char[] { '-' };
                    date = temp.Split(splitDate)[2];

                    object[] arr = new object[4];
                    arr[0] = time1;
                    arr[1] = time2;
                    arr[2] = date;
                    arr[3] = luisResult.Entities.title[0];

                    return await stepContext.BeginDialogAsync(nameof(AddAppointmentDialog), arr, cancellationToken);
                }
                return await stepContext.BeginDialogAsync(nameof(AddAppointmentDialog), date, cancellationToken);
            }
            else if(luisResults != null && luisResults.TopIntent().intent == Intent.CancelAppointments)
            {
                return await stepContext.BeginDialogAsync(nameof(CancelAppointmentDialog), date, cancellationToken);
            }
            else if(luisResults != null && luisResults.TopIntent().intent == Intent.View)
            {
                var promptMessage = MessageFactory.Text("Enter the date in (mm/dd/yyyy) format to view your scheduled appointments.", InputHints.ExpectingInput);
                choosenOption = "view";
                return await stepContext.PromptAsync(nameof(DateTimePrompt), new PromptOptions { Prompt = promptMessage, }, cancellationToken);
            }
            else if(luisResults != null && luisResults.TopIntent().intent == Intent.Cancel)
            {
                var promptMessage = MessageFactory.Text("Enter the date of the appointment to be cancelled, in (mm/dd/yyyy) format.", InputHints.ExpectingInput);
                choosenOption = "cancel";
                return await stepContext.PromptAsync(nameof(DateTimePrompt), new PromptOptions { Prompt = promptMessage, }, cancellationToken);
            }
            else if(luisResults != null && luisResults.TopIntent().intent == Intent.Add)
            {
                var promptMessage = MessageFactory.Text("Enter the date in (mm/dd/yyyy) format to add an appointment.", InputHints.ExpectingInput);
                choosenOption = "add";
                return await stepContext.PromptAsync(nameof(DateTimePrompt), new PromptOptions { Prompt = promptMessage, }, cancellationToken);
            }
            else if(luisResults != null && luisResults.TopIntent().intent == Intent.Exit)
            {
                choosenOption = "no thnaks";
                return await stepContext.EndDialogAsync(stepContext.Options);
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
            if(stepContext.Result == null)
            {
                return await stepContext.EndDialogAsync();
            }

            var val = ((stepContext.Result as List<DateTimeResolution>)[0] as DateTimeResolution).Value;
            char[] split = new char[] { '-' };
            var date = val.Split(split)[2];

            if (choosenOption == "view")
            {
                return await stepContext.BeginDialogAsync(nameof(ViewAppointmentDialog), date, cancellationToken);
            }
            else if (choosenOption == "add")
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

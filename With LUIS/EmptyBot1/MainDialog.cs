// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;

namespace EmptyBot1
{
    public class MainDialog : ComponentDialog
    {
        internal readonly AppointmentRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        SchedulerViewModel viewModel;
        List<Choice> choices = new List<Choice>();
        string userName = "";
        internal ProcessChoiceDialog processChoiceDialog;
        public ChoiceDialog choiceDialog;
        public ViewAppointmentDialog viewAppointmentDialog;
        public AddAppointmentDialog addAppointmentDialog;
        public CancelAppointmentDialog cancelAppointmentDialog;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(AppointmentRecognizer luisRecognizer , ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {

            _luisRecognizer = luisRecognizer;
            Logger = logger;

            viewModel = new SchedulerViewModel();

            choices.Add(new Choice("Add"));
            choices.Add(new Choice("Cancel"));
            choices.Add(new Choice("View"));
            choices.Add(new Choice("Exit"));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt)));

            processChoiceDialog = new ProcessChoiceDialog("", viewModel, choices, this);
            AddDialog(processChoiceDialog);

            if (this.choiceDialog == null)
            {
                choiceDialog = new ChoiceDialog("", viewModel, choices, this);
            }
            AddDialog(choiceDialog);

            if (this.viewAppointmentDialog == null)
            {
                viewAppointmentDialog = new ViewAppointmentDialog("", viewModel, this, this.choices);
            }
            AddDialog(viewAppointmentDialog);

            if (this.addAppointmentDialog == null)
            {
                addAppointmentDialog = new AddAppointmentDialog("", viewModel, this, this.choices);
            }
            AddDialog(addAppointmentDialog);

            if (this.cancelAppointmentDialog == null)
            {
                cancelAppointmentDialog = new CancelAppointmentDialog("", viewModel, this, this.choices);
            }
            AddDialog(cancelAppointmentDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GreetStepAsync ,
                NameEchoStepAsync,
                ChoiceEchoStepAsync,
                ConformationStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            var val = stepContext.Result;
            await stepContext.Context.SendActivityAsync("Hello ! I am your appointment scheduler bot.");
            var promptMessage = MessageFactory.Text("What would you like to do ?", "Hello i am your appointment scheduler bot. Say Hi to begin", InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> GreetStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var luisResult = await _luisRecognizer.RecognizeAsync<Appointment>(stepContext.Context, cancellationToken);

            this.processChoiceDialog.userName = this.userName;
            return await stepContext.BeginDialogAsync(nameof(ProcessChoiceDialog), luisResult, cancellationToken);
        }

        private async Task<DialogTurnResult> NameEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Result == null)
            {
                await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
                return await stepContext.EndDialogAsync();
            }
            var val = (string)stepContext.Result;
            userName = val;
            if (!string.IsNullOrEmpty(val))
            {
                var promptMessage = MessageFactory.Text("Hi " + val + " What can i do for you ?", "Hi " + userName + ", What can i do for you" , InputHints.AcceptingInput);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Prompt = promptMessage, Choices = choices }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ChoiceEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            this.processChoiceDialog.userName = this.userName;
            return await stepContext.BeginDialogAsync(nameof(ProcessChoiceDialog), (stepContext.Result as FoundChoice).Value.ToLower(), cancellationToken);
        }

        private async Task<DialogTurnResult> ConformationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (stepContext.Result != null && stepContext.Result.ToString().ToLower() == "no thanks")
            {
                await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
                return await stepContext.EndDialogAsync();
            }
            else
            {
                this.choiceDialog.userName = this.userName;
                return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), stepContext.Result.ToString().ToLower(), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RestartStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await ChoiceEchoStepAsync(stepContext, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "Test", cancellationToken);
        }
    }

    public class AppointmentModel
    {
        public string Date { get; set; }
    }
}



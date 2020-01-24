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
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
 
namespace EmptyBot1
{
    public class MainDialog : ComponentDialog
    {
        SchedulerViewModel viewModel;
        List<Choice> choices = new List<Choice>();
        string userName = "";
        internal ProcessChoiceDialog processChoiceDialog;
        public ChoiceDialog choiceDialog;
        public ViewAppointmentDialog viewAppointmentDialog;
        public AddAppointmentDialog addAppointmentDialog;
        public CancelAppointmentDialog cancelAppointmentDialog;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog( )
            : base(nameof(MainDialog))
        {

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
                viewAppointmentDialog = new ViewAppointmentDialog("", viewModel, this,this.choices);
            }
            AddDialog(viewAppointmentDialog);

            if(this.addAppointmentDialog == null)
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
            var val = stepContext.Result;

            await stepContext.Context.SendActivityAsync("Hello ! I am your appointment scheduler bot");
            var promptMessage = MessageFactory.Text("Say Hi to begin", "Hello i am your appointment scheduler bot. Say Hi to begin", InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> GreetStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var val = stepContext.Result.ToString().ToLower();
            if (val.Equals("hi") || val.Equals("bonjour") || val.Equals("hello"))
            {
                var promptMessage = MessageFactory.Text("What is your name ?", "What is your name", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
  
            }
        }

        private async Task<DialogTurnResult> NameEchoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var val = (string)stepContext.Result;
            userName = val;
            if (!string.IsNullOrEmpty(val))
            {
                var promptMessage = MessageFactory.Text("Hi " + val + ", What can i do for you ?", "Hi " + userName + ", What can i do for you" , InputHints.AcceptingInput);
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

            //if (stepContext.Result != null && stepContext.Result.ToString().ToLower() == "no thanks")
            //{
            //    await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
            //    return await stepContext.EndDialogAsync();
            //}
            //else
            //{
            //    this.choiceDialog.userName = this.userName;
            //    return await stepContext.BeginDialogAsync(nameof(ChoiceDialog), stepContext.Result.ToString().ToLower(), cancellationToken);
            //}
            await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
            return await stepContext.EndDialogAsync();

            //var promptMessage = MessageFactory.Text("What else can i do for you " + "" + userName + "?", InputHints.ExpectingInput);
            //return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Prompt = promptMessage, Choices = choices }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            await stepContext.Context.SendActivityAsync("Thank you for contacting me. Looking forward to help you again " + userName);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "Test", cancellationToken);
        }
    }
}

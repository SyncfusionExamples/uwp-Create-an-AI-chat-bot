using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Syncfusion.XForms.Chat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ScheduleGettingStarted
{
    /// <summary>
    /// A Class used to communicate with Azure bot service.
    /// </summary>
    public class BotService
    {
        /// <summary>
        /// Http client.
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// Conversation details.
        /// </summary>
        private Conversation conversation;

        /// <summary>
        /// Direct line address to establish connection.
        /// </summary>
        private string BotBaseAddress = "https://directline.botframework.com/v3/directline/conversations/";

        /// <summary>
        /// Direct line key to establish connection to syncfusion bot.
        /// </summary>
        private string directLineKey = "Enter your Azure bot's direct line key here";

        /// <summary>
        /// water mark used to get newly added message or actvity.
        /// </summary>
        private string watermark = string.Empty;

        /// <summary>
        /// Initializes a new instance of <see cref="BotService"/> class.
        /// </summary>
        /// <param name="viewModel">view model as paramter.</param>
        public BotService(SchedulerViewModel viewModel)
        {
            this.ViewModel = viewModel;
            InitializeHttp();
        }

        /// <summary>
        /// Gets or set the flight booking view model.
        /// </summary>
        internal SchedulerViewModel ViewModel { get; set; }
        
        
        /// <summary>
        /// Activity is created and message is send to bot.
        /// </summary>
        /// <param name="text">text from current user.</param>
        internal void SendMessageToBot(string text)
        {
            Activity activity = new Activity()
            {
                From = new ChannelAccount()
                {
                    Id = this.ViewModel.CurrentUser.Name
                },

                Text = text,
                Type = "message"
            };

            PostActvity(activity);
        }

        /// <summary>
        /// Reading bot response message.
        /// </summary>
        /// <returns></returns>
        internal async Task ReadBotMessagesAsync()
        {
            try
            {
                string conversationUrl = this.BotBaseAddress + this.conversation.ConversationId + "/activities?watermark=" + this.watermark;
                using (HttpResponseMessage messagesReceived = await this.httpClient.GetAsync(conversationUrl, HttpCompletionOption.ResponseContentRead))
                {
                    string messagesReceivedData = await messagesReceived.Content.ReadAsStringAsync();
                    ActivitySet messagesRoot = JsonConvert.DeserializeObject<ActivitySet>(messagesReceivedData);

                    if (messagesRoot != null)
                    {
                        this.watermark = messagesRoot.Watermark;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            foreach (Activity activity in messagesRoot.Activities)
                            {
                                if (activity.From.Id == "GarvisBot" && activity.Type == "message")
                                {
                                    this.BotReply(activity);
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while reading Bot activity. exception message - {0}", ex.Message);
            }

            this.ViewModel.ShowTypingIndicator = false;
            this.ViewModel.ShowBusyIndicator = false;
        }

        /// <summary>
        /// Initialize the Http client and initiate conversation if internet connection is available.
        /// </summary>
        private void InitializeHttp()
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(this.BotBaseAddress);
            this.httpClient.DefaultRequestHeaders.Accept.Clear();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.directLineKey);
            this.httpClient.Timeout = Timeout.InfiniteTimeSpan;

            
            
                SetupConversation();
            
        }

        /// <summary>
        /// Starts conversation to azure bot.
        /// </summary>
        private async void SetupConversation()
        {
            HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(new Conversation()), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await this.httpClient.PostAsync("/v3/directline/conversations", contentPost);
                if (response.IsSuccessStatusCode)
                {
                    string conversationInfo = await response.Content.ReadAsStringAsync();
                    this.conversation = JsonConvert.DeserializeObject<Conversation>(conversationInfo);
                    await Task.Delay(2000);

                    Activity activity = new Activity();
                    activity.From = new ChannelAccount()
                    {
                        Id = ViewModel.CurrentUser.Name,
                        Name = ViewModel.CurrentUser.Name,
                    };

                    activity.Type = "add";
                    activity.Action = "add";
                    this.PostActvity(activity);
                }
            }
            catch(Exception ee) 
            {
                
            }
        }

        /// <summary>
        /// current user message is passed to Bot. 
        /// </summary>
        /// <param name="activity"></param>
        private async void PostActvity(Activity activity)
        {
            StringContent contentPost = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");
            string conversationUrl = this.BotBaseAddress + this.conversation.ConversationId + "/activities";

            try
            {
                await this.httpClient.PostAsync(conversationUrl, contentPost);
                await this.ReadBotMessagesAsync();
            }
            catch { }
        }

        /// <summary>
        /// Used to identify what type of message should be added in message collection. 
        /// </summary>
        /// <param name="activity">bot reply message is received as activity.</param>
        private void BotReply(Activity activity)
        {
            if (!string.IsNullOrEmpty(activity.Text))
            {

                if (activity.SuggestedActions == null || activity.SuggestedActions?.Actions.Count == 0)
                {
                    this.AddTextMessage(activity.Text);
                }
                else
                {
                    this.AddTextMessage(activity.Text, activity.SuggestedActions);
                }

                if(activity.Value != null)
                {

                    var meeting = JsonConvert.DeserializeObject<Meeting>(activity.Value.ToString(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore  });

                    if (meeting != null)
                    {
                        if (activity.Text == "New appointment added")
                        {
                            meeting.Color = Color.Red;

                            this.ViewModel.Meetings.Add(meeting);
                        }
                        else
                        {
                            var remove = this.ViewModel.Meetings.Where(X=>X.From.Date == System.DateTime.Now.Date).ToList();
                            if (remove != null)
                            {
                                this.ViewModel.Meetings.Remove(remove[1]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Text message is created and added to the message collection.
        /// </summary>
        /// <param name="text">new message text.</param>
        /// <param name="suggestedActions">suggestion value for message</param>
        private void AddTextMessage(string text, SuggestedActions suggestedActions = null)
        {
            TextMessage message = new TextMessage();
            message.Text = text;
            message.Author = this.ViewModel.Bot;

            if (suggestedActions != null)
            {
                ChatSuggestions suggestions = new ChatSuggestions();
                var suggestionItems = new ObservableCollection<ISuggestion>();
                foreach (CardAction action in suggestedActions.Actions)
                {
                    var suggestion = new Suggestion();
                    suggestion.Text = action.Title;
                    if (!string.IsNullOrEmpty(action.Image))
                    {
                        suggestion.Image = action.Image;
                    }

                    suggestionItems.Add(suggestion);
                }

                suggestions.Items = suggestionItems;
                message.Suggestions = suggestions;
            }

            ViewModel.Messages.Add(message);
        }

        /// <summary>
        /// Calendar message is created and added to the message collection.
        /// </summary>
        /// <param name="text">text for calendar message.</param>
        private void AddCalendarMessage(string text)
        {
            CalendarMessage message = new CalendarMessage();
            message.Text = text;
            message.Author = this.ViewModel.Bot;
            message.SelectedDate = DateTime.Now;
            ViewModel.Messages.Add(message);
        }
    }
}

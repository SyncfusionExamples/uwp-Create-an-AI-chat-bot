using Syncfusion.XForms.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
namespace ScheduleGettingStarted
{
    /// <summary>   
    /// Represents collection of appointments.   
    /// </summary>
    public class SchedulerViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Meeting> Meetings { get; set; }
        List<string> eventNameCollection;
        List<Color> colorCollection;

        public event PropertyChangedEventHandler PropertyChanged;

        public SchedulerViewModel()
        {
            Meetings = new ObservableCollection<Meeting>();
            CreateEventNameCollection();
            CreateColorCollection();
            CreateAppointments();
            this.Messages = new ObservableCollection<object>();
            this.Bot = new Author() { Name = "Barry", Avatar = "Robot.png" };
            this.ShowBusyIndicator = true;
            this.IsConnectionNotEstablished = false;
            this.BotService = new BotService(this);
            this.TypingIndicator = new ChatTypingIndicator();
            this.TypingIndicator.Authors.Add(this.Bot);
            this.TypingIndicator.AvatarViewType = AvatarViewType.Image;
            this.TypingIndicator.Text = "Barry is typing ...";
            this.CurrentUser = new Author() { Name = "Nancy", Avatar = "People_Circle16.png" };
        }
        /// <summary>
        /// Creates meetings and stores in a collection.  
        /// </summary>
        private void CreateAppointments()
        {
            Random randomTime = new Random();
            List<Point> randomTimeCollection = GettingTimeRanges();
            DateTime date;
            DateTime DateFrom = DateTime.Now.AddDays(-10);
            DateTime DateTo = DateTime.Now.AddDays(10);
            DateTime dataRangeStart = DateTime.Now.AddDays(-3);
            DateTime dataRangeEnd = DateTime.Now.AddDays(3);

            for (date = DateFrom; date < DateTo; date = date.AddDays(1))
            {
                if (date.Date != System.DateTime.Now.Date)
                {
                    if ((DateTime.Compare(date, dataRangeStart) > 0) && (DateTime.Compare(date, dataRangeEnd) < 0))
                    {
                        for (int AdditionalAppointmentIndex = 0; AdditionalAppointmentIndex < 3; AdditionalAppointmentIndex++)
                        {
                            Meeting meeting = new Meeting();
                            int hour = (randomTime.Next((int)randomTimeCollection[AdditionalAppointmentIndex].X, (int)randomTimeCollection[AdditionalAppointmentIndex].Y));
                            meeting.From = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                            meeting.To = (meeting.From.AddHours(1));
                            meeting.EventName = eventNameCollection[randomTime.Next(9)];
                            meeting.Color = colorCollection[randomTime.Next(9)];
                            if (AdditionalAppointmentIndex % 3 == 0)
                                meeting.AllDay = true;
                            Meetings.Add(meeting);
                        }
                    }
                    else
                    {
                        Meeting meeting = new Meeting();
                        meeting.From = new DateTime(date.Year, date.Month, date.Day, randomTime.Next(9, 11), 0, 0);
                        meeting.To = (meeting.From.AddHours(1));
                        meeting.EventName = eventNameCollection[randomTime.Next(9)];
                        meeting.Color = colorCollection[randomTime.Next(9)];
                        Meetings.Add(meeting);
                    }
                }
            }

            for (int i = 1; i <= 3; i++)
            {
                Meeting meeting = new Meeting();
                meeting.From = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, 7 + i + i, 0, 0);
                meeting.To = (meeting.From.AddHours(1));
                meeting.EventName = eventNameCollection[i];
                meeting.Color = colorCollection[randomTime.Next(9)];
                Meetings.Add(meeting);
            }
        }

        /// <summary>  
        /// Creates event names collection.  
        /// </summary>  
        private void CreateEventNameCollection()
        {
            eventNameCollection = new List<string>();
            eventNameCollection.Add("General Meeting");
            eventNameCollection.Add("Plan Execution");
            eventNameCollection.Add("Project Plan");
            eventNameCollection.Add("Consulting");
            eventNameCollection.Add("Performance Check");
            eventNameCollection.Add("Yoga Therapy");
            eventNameCollection.Add("Plan Execution");
            eventNameCollection.Add("Project Plan");
            eventNameCollection.Add("Consulting");
            eventNameCollection.Add("Performance Check");
        }

        /// <summary>  
        /// Creates color collection.  
        /// </summary>  
        private void CreateColorCollection()
        {
            colorCollection = new List<Color>();
            colorCollection.Add(Color.FromHex("#FF339933"));
            colorCollection.Add(Color.FromHex("#FF00ABA9"));
            colorCollection.Add(Color.FromHex("#FFE671B8"));
            colorCollection.Add(Color.FromHex("#FF1BA1E2"));
            colorCollection.Add(Color.FromHex("#FFD80073"));
            colorCollection.Add(Color.FromHex("#FFA2C139"));
            colorCollection.Add(Color.FromHex("#FFA2C139"));
            colorCollection.Add(Color.FromHex("#FFD80073"));
            colorCollection.Add(Color.FromHex("#FF339933"));
            colorCollection.Add(Color.FromHex("#FFE671B8"));
            colorCollection.Add(Color.FromHex("#FF00ABA9"));
        }

        /// <summary>
        /// Gets the time ranges.
        /// </summary>
        private List<Point> GettingTimeRanges()
        {
            List<Point> randomTimeCollection = new List<Point>();
            randomTimeCollection.Add(new Point(9, 11));
            randomTimeCollection.Add(new Point(12, 14));
            randomTimeCollection.Add(new Point(15, 17));
            return randomTimeCollection;
        }


        #region Chat 

        /// <summary>
        /// current user of chat.
        /// </summary>
        private Author currentUser;

        /// <summary>
        /// bot author.
        /// </summary>
        private Author bot;

        /// <summary>
        /// Indicates the typing indicator visibility. 
        /// </summary>
        private bool showTypingIndicator;

        /// <summary>
        /// used to check network connection state.
        /// </summary>
        private bool isConnectionnotEstablished;

        /// <summary>
        /// Chat typing indicator.
        /// </summary>
        private ChatTypingIndicator typingIndicator;

        /// <summary>
        /// chat conversation messages.
        /// </summary>
        private ObservableCollection<object> messages;

        /// <summary>
        /// used to define busy indicator visible state.
        /// </summary>
        private bool showBusyIndicator;

        /// <summary>
        /// Gets or sets the Chat typing indicator value.
        /// </summary>
        public ChatTypingIndicator TypingIndicator
        {
            get
            {
                return this.typingIndicator;
            }
            set
            {
                this.typingIndicator = value;
                RaisePropertyChanged("TypingIndicator");
            }
        }

        /// <summary>
        /// Gets or sets the message conversation.
        /// </summary>
        public ObservableCollection<object> Messages
        {
            get
            {
                return this.messages;
            }
            set
            {
                this.messages = value;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the typing indicator is visible or not.
        /// </summary>
        public bool ShowTypingIndicator
        {
            get
            {
                return this.showTypingIndicator;
            }
            set
            {
                this.showTypingIndicator = value;
                RaisePropertyChanged("ShowTypingIndicator");
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the busy indicator is visible or not.
        /// </summary>
        public bool ShowBusyIndicator
        {
            get
            {
                return this.showBusyIndicator;
            }
            set
            {
                this.showBusyIndicator = value;
                RaisePropertyChanged("ShowBusyIndicator");
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the internet connection is established or not.
        /// </summary>
        public bool IsConnectionNotEstablished
        {
            get
            {
                return this.isConnectionnotEstablished;
            }
            set
            {
                this.isConnectionnotEstablished = value;
                RaisePropertyChanged("IsConnectionNotEstablished");
            }
        }

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public Author CurrentUser
        {
            get
            {
                return this.currentUser;
            }
            set
            {
                this.currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }

        /// <summary>
        /// Get or sets the bot author.
        /// </summary>
        public Author Bot
        {
            get
            {
                return this.bot;
            }
            set
            {
                this.bot = value;
                RaisePropertyChanged("Bot");
            }
        }

        /// <summary>
        /// Gets or sets the bot service.
        /// </summary>
        internal BotService BotService { get; set; }

        #endregion

        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name</m>
        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}

using Syncfusion.XForms.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ScheduleGettingStarted
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SchedulerPage : ContentPage
	{
		public SchedulerPage ()
		{
			InitializeComponent ();
            viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
		}

        private  async void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var chatItem in e.NewItems)
                {
                    TextMessage textMessage = chatItem as TextMessage;
                    if (textMessage != null && textMessage.Author == this.viewModel.CurrentUser)
                    {
                        textMessage.ShowAvatar = true;
                        this.viewModel.ShowTypingIndicator = true;
                        this.viewModel.BotService.SendMessageToBot(textMessage.Text);
                    }
                    else
                    {
                        await Task.Delay(50);
                        this.sfChat.ScrollToMessage(chatItem);
                    }
                }
            }
        }

        private void Add_Clicked(object sender, EventArgs e)
        {
            //(this.schedule.DataSource as ObservableCollection<Meeting>).Add(new Meeting()
            //{
            //    BackgroundColor = Color.Red,
            //    AllDay = false,
            //    From = System.DateTime.Now,
            //    To = System.DateTime.Now.AddHours(4),
            //    EventName = "Testing for bot"
            //});
            //this.viewModel.Meetings.Add(new Meeting()
            //{
            //    color = Color.Red,
            //    AllDay = false,
            //    From = System.DateTime.Now,
            //    To = System.DateTime.Now.AddHours(4),
            //    EventName = "Testing for bot",
            //    //Content = new Label() { BackgroundColor=Color.Yellow, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand}
            //});
            var a = this.viewModel.Meetings.LastOrDefault(x => x.From.Day == DateTime.Now.Day);
            a.EventName = "testing runtime change";
            a.Color = Color.LemonChiffon;
            a.From = System.DateTime.Now.AddHours(4);
            a.To = System.DateTime.Now.AddHours(6);
            //this.viewModel.Meetings.Remove(a);
        }
    }
}
using System;
using System.ComponentModel;
using Xamarin.Forms;
namespace ScheduleGettingStarted
{
    /// <summary>   
    /// Represents custom data properties.   
    /// </summary>  
    public class Meeting : INotifyPropertyChanged
    {
        private string eventName;
        private string organizer;
        private string contactID;
        private int capacity;
        private DateTime from;
        private DateTime to;


        private bool allDay;
        private Color color;

        public string EventName
        {
            get
            {
                return eventName;
            }
            set
            {
                eventName = value;
                RaisePropertyChanged("EventName");
            }
        }
        public string Organizer
        {
            get
            {
                return organizer;
            }
            set
            {
                organizer = value;
                RaisePropertyChanged("Organizer");
            }
        }
        public string ContactID
        {
            get
            {
                return contactID;
            }
            set
            {
                contactID = value;
                RaisePropertyChanged("ContactID");
            }
        }
        public int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                capacity = value;
                RaisePropertyChanged("Capacity");
            }
        }
        public DateTime From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
                RaisePropertyChanged("From");
            }
        }
        public DateTime To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
                RaisePropertyChanged("To");
            }
        }


        public bool AllDay
        {
            get
            {
                return allDay;
            }
            set
            {
                allDay = value;
                RaisePropertyChanged("AllDay");
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                RaisePropertyChanged("Color");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(String Name)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }
    }
}

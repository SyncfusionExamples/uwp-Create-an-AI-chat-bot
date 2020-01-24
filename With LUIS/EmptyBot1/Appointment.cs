using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmptyBot1
{
    public enum Intent
    {
        Cancel,
        Showappoitments,
        AddAppointments,
        CancelAppointments,
        View,
        Add,
        Exit,
        None
    }
    public class Appointment : IRecognizerConvert
    {
        public string Text;
        public string AlteredText;
        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<Appointment>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
        }

        public class _Entities
        {
            public DateTimeSpec[] datetime;
            public string[] title;
        }

        public Dictionary<Intent, IntentScore> Intents;



        public _Entities Entities;

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}

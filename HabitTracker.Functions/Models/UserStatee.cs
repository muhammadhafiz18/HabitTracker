using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Functions.Models
{
    public class UserStatee
    {
        public long UserId { get; set; }
        public string State { get; set; }
        public string Langauge { get; set; }
        public string NewHabitName { get; set; }
        public string NewHabitFrequency { get; set; }
        public string HabitIdForRemoving { get; set; }
    }
}

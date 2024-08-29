using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Functions.Models
{
    public class Habit
    {
        public string HabitId { get; set; }
        public string Name { get; set; }
        public string FrequencyRu { get; set; }
        public string FrequencyUz { get; set; }
        public string FrequencyWithNumbers { get; set; }
        public string AlertTime { get; set; }
        public string TillAlertDayLeft { get; set; }
        public bool isCompleted { get; set; } = false;
    }
}

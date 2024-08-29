using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Functions.Models
{
    public class UserData
    {
        public string UserId { get; set; }
        public string UserName { get; set; } = "noName";
        public string Language { get; set; } = "Uz";
        public List<Habit> Habits { get; set; }
    }
}

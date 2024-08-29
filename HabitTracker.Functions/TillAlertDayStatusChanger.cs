using HabitTracker.Functions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Functions
{
    public class TillAlertDayStatusChanger
    {
        public static async Task TillAlertDayStatusChangerAsync(ILogger logger)
        {
            var userDetails = new UserDetails();
            List<UserData> userDataFromJson = await userDetails.UserDetailGetter();

            foreach (var user in userDataFromJson)
            {
                await userDetails.UserDetailRemover(user);
                foreach (var habit in user.Habits)
                {
                    if (habit.TillAlertDayLeft != null && habit.TillAlertDayLeft != "0")
                    {
                        habit.TillAlertDayLeft = Convert.ToString(int.Parse(habit.TillAlertDayLeft) - 1);
                       logger.LogInformation($"{user.UserId} {user.UserName}'s habit named {habit.Name}'s status is changed. {habit.TillAlertDayLeft} left till next notification...");
                    }
                }
                await userDetails.UserDetailAdder(user);
            }
        }
    }
}

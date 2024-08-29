using HabitTracker.Functions.Models;
using Telegram.Bot.Types;

namespace HabitTracker.Functions
{
    public class UserDetailBuilder
    {
        public UserData UserDataBuilder(long chatId, string userName="noName", string language="Uz", string habitId = "0", string habitName = "noHabitName", bool isCompleted = false) {
            var newUser = new UserData();

            newUser = new UserData
            {
                UserId = $"{chatId}",
                UserName = userName,
                Language = language,
                Habits = new List<Habit>
                {
                    new Habit {
                    HabitId = habitId,
                    Name = habitName
                       
                    }
                }
            };


            return newUser;
        }
    }
}

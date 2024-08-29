using System;
using HabitTracker.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace HabitTracker.Functions
{
    public class HabitSender
    {
        private static readonly string BotToken = "YourBotTokenHere";
        private static readonly TelegramBotClient BotClient = new TelegramBotClient(BotToken);
        private readonly ILogger _logger;

        public HabitSender(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HabitSender>();
        }

        [Function("HabitSender")]
        public async Task Run([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string timeZoneId = "Central Asia Standard Time";

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Convert UTC time to local time in Uzbekistan
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            DateTime oneHourBefore = localTime.AddHours(-1);
            string timeFormat = "HH:mm:ss";
            string formattedTime = oneHourBefore.ToString(timeFormat);
            string lastTwoChars = formattedTime.Length >= 2 ? formattedTime.Substring(formattedTime.Length - 2) : formattedTime;
            if (lastTwoChars == "59")
            {
                DateTime oneSecondBefore = utcNow.AddSeconds(+1);
                formattedTime = oneSecondBefore.ToString(timeFormat);
            }   

            _logger.LogInformation($"now it is {formattedTime} in Tashkent");

            var userDetails = new UserDetails();

            if (formattedTime == "00:00:00")
            {
                await TillAlertDayStatusChanger.TillAlertDayStatusChangerAsync(_logger);
            }

            List<UserData> userDataFromJson = await userDetails.UserDetailGetter();

            foreach (var user in userDataFromJson)
            {
                await userDetails.UserDetailRemover(user);
                foreach (var habit in user.Habits)
                {
                    if (habit.AlertTime == formattedTime && habit.TillAlertDayLeft == "0" && habit.isCompleted == false)
                    {
                        var doneHabitUz = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("🚀 Ha", "🚀 Ha"),
                                InlineKeyboardButton.WithCallbackData("❌ Yo'q", "❌ Yo'q")
                            },
                        });

                        var doneHabitRu = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("🚀 Да", "🚀 Да"),
                                InlineKeyboardButton.WithCallbackData("❌ Нет", "❌ Нет")
                            }
                        });

                        await BotClient.SendTextMessageAsync(
                            chatId: user.UserId,
                            text: user.Language == "Uz" ? $"Bugun {habit.Name} vazifasini qilishingiz kerak edi\n\nBajardingizmi?" : $"Сегодня вам предстоит выполнить {habit.Name} задание. Вы сделали это?",
                            replyMarkup: user.Language == "Uz" ? doneHabitUz : doneHabitRu
                            );
                        habit.TillAlertDayLeft = habit.FrequencyWithNumbers;
                        _logger.LogInformation($"{user.UserId} {user.UserName}'s habit is sent. Habit name: <{habit.Name}>. {habit.TillAlertDayLeft} left till next notification...");
                    }
                }
                await userDetails.UserDetailAdder(user);
            }
        }
    }
}

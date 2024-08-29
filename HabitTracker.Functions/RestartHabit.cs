using HabitTracker.Functions.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace HabitTracker.Functions
{
    public class RestartHabit
    {
        public static async Task RestartHabitAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, HabitTracker.Functions.Models.UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            List<string> allHabitsUz = new List<string>();

            for (int i = 1; i < currentUserData.Habits.Count(); i++)
            {
                if (currentUserData.Habits[i].isCompleted == true)
                {
                    allHabitsUz.Add($"{currentUserData.Habits[i].HabitId}. {currentUserData.Habits[i].Name}");

                }
                else
                {
                    continue;
                }
            }

            if (allHabitsUz.Count() == 0)
            {
                await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "Sizda hali ko'nikib bo'lingan odatlar yo'q." : "У вас еще нет привычек, к которым вы привыкли.",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
                    );
                return;
            }

            List<string> allHabitsRu = new List<string>(allHabitsUz);
            allHabitsRu.Add("🏠 Вернуться в Главное Меню");
            allHabitsUz.Add("🏠 Asosiy menyuga qaytish");

            ReplyKeyboardMarkup removeHabitKeyboardShowesAllHabits;

            removeHabitKeyboardShowesAllHabits = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? allHabitsUz : allHabitsRu);

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "Qaytadan boshlamoqchi bo'lgan ishingizni tanlang" : "Выберите привычку, которую вы хотите повторить",
                replyMarkup: removeHabitKeyboardShowesAllHabits,
                parseMode: ParseMode.Markdown
                );

            var userState = new UserState();

            UserStatee currentUserState = new UserStatee
            {
                UserId = chatId,
                State = "Waiting_for_habit_name_that_want_to_do_again"
            };

            await userState.UserDetailAdder(currentUserState);

        }
    }
}

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
    public class RemoveHabitCommand
    {
        public static async Task RemoveHabitCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, HabitTracker.Functions.Models.UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            if (currentUserData.Habits.Count() == 1)
            {
                await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "Odatlar ro'yxati bo'sh" : "Список привычек пуст",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
                    );
                return;
            }

            List<string> allHabitsUz = new List<string>();

            for (int i = 1; i < currentUserData.Habits.Count(); i++)
            {
                allHabitsUz.Add($"{currentUserData.Habits[i].HabitId}. {currentUserData.Habits[i].Name}");
            }

            List<string> allHabitsRu = new List<string>(allHabitsUz);
            allHabitsRu.Add("🏠 Вернуться в Главное Меню");
            allHabitsUz.Add("🏠 Asosiy menyuga qaytish");

            ReplyKeyboardMarkup removeHabitKeyboardShowesAllHabits;

            removeHabitKeyboardShowesAllHabits = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? allHabitsUz : allHabitsRu);

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "O'chirish uchun odat tanlang" : "Выберите привичка для удаления",
                replyMarkup: removeHabitKeyboardShowesAllHabits,
                parseMode: ParseMode.Markdown
                );

            var userState = new UserState();

            UserStatee currentUserState = new UserStatee
            {
                UserId = chatId,
                State = "Waiting_for_habit_name_for_removing"
            };

            await userState.UserDetailAdder(currentUserState);

        }
    }
}

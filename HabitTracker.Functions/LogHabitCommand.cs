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
    public class LogHabitCommand
    {
        public static async Task LogHabitCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, HabitTracker.Functions.Models.UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            List<string> allHabitsUz = new List<string>();

            for (int i = 1; i < currentUserData.Habits.Count(); i++)
            {
                if (currentUserData.Habits[i].isCompleted == false)
                {
                    allHabitsUz.Add($"{currentUserData.Habits[i].HabitId}. {currentUserData.Habits[i].Name}");

                } else
                {
                    continue;
                }
            }

            if (allHabitsUz.Count() == 0)
            {
                await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "Odatlar ro'yxati bo'sh.\n\nYangi odat qo'shish uchun \"Yangi odat qo'shish tugmasini bosing\"" : "Список прычики пуст.\n\nЧтобы добавить новую привычку, нажмите «Добавить новую привычку».",
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
                text: currentUserData.Language == "Uz" ? "Ko'nikib bo'lgan odatingizni tanlang" : "Выберите привычку, к которой вы привыкли\r\n",
                replyMarkup: removeHabitKeyboardShowesAllHabits,
                parseMode: ParseMode.Markdown
                );

            var userState = new UserState();

            UserStatee currentUserState = new UserStatee
            {
                UserId = chatId,
                State = "Waiting_for_habit_name_that_used_to"
            };

            await userState.UserDetailAdder(currentUserState);

        }
    }
}

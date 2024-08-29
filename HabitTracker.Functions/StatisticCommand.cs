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
    public class StatisticCommand
    {
        public static async Task StatisticCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, HabitTracker.Functions.Models.UserData currentUserData, ReplyKeyboardMarkup replyKeyboard)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            int totalNumberOfHabits = currentUserData.Habits.Count() - 1;

            if (totalNumberOfHabits == 0)
            {
                await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "Odatlar soni 0 ta" : "Общая количества привирчек 0",
                replyMarkup: replyKeyboard,

                parseMode: ParseMode.Markdown
            );
            }
            else
            {
                var viewHabitInlineKeyboardUz = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("📄 Odatlarni ko'rish", "📄 Odatlarni ko'rish")
                }
            });

                var viewHabitInlineKeyboardRu = new InlineKeyboardMarkup(new[]
                {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("📄 Посмотреть Привычки", "📄 Посмотреть Привычки")
                }
            });

                string responseMessage = currentUserData.Language == "Uz" ? $"Umumiy shakillantirilishi kerak bo'lgan odatlar soni: {totalNumberOfHabits} ta" : $"Общая количества привирчек {totalNumberOfHabits}";
                await BotClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: responseMessage,
                    replyMarkup: currentUserData.Language == "Uz" ? viewHabitInlineKeyboardUz : viewHabitInlineKeyboardRu,

                    parseMode: ParseMode.Markdown
                );
            }
        }
    }
}

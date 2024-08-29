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
    public class ManualCommand
    {
        public static async Task ManualCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, HabitTracker.Functions.Models.UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            string responseMessage = currentUserData.Language == "Uz" ? "Ushbu Telegram botning qo'llanmasi: https://telegra.ph/Habit-Tracker-qollanmasi-08-09" : "Руководства этого Телеграм бота: https://telegra.ph/Rukovodstvo-dlya-Habit-Tracker-08-09";
            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: responseMessage,
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
    }
}

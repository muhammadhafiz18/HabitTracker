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
    public class FeedbackCommand
    {
        public static async Task FeedbackCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, HabitTracker.Functions.Models.UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: currentUserData.Language == "Uz" ? "Taklif yoki shikoyatlaringizni @HabitTrackerFeedback_bot ga yuborishingiz mumkin" : "Вы можете отправлять отзывы или жалобы на @HabitTrackerFeedback_bot",
                replyMarkup: inlineKeyboard
            );
        }
    }
}

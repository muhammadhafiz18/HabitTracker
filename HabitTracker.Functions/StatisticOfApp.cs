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
    public class StatisticOfApp
    {
        public static async Task StatisticOfAppAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, List<HabitTracker.Functions.Models.UserData> userDatas, HabitTracker.Functions.Models.UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            string responseMessage = currentUserData.Language == "Uz" ? $"Umumiy foydalanuvchilar soni: {userDatas.Count()-1}" : $"Общая количества пользователей: {userDatas.Count()-1}";
            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: responseMessage,
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
    }
}

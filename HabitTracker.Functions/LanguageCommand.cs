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
    public class LanguageCommand
    {
        public static async Task LanguageCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, string currentLangauge)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            string responseMessage = currentLangauge == "Uz" ? "Iltimos yangi til tanlang" : "Пожалуйста выберите новый язык";
            var langaugeChooser = new LanguageChooser();
            await langaugeChooser.LanguageChooserAsync(update, BotClient, "waiting_for_new_language", responseMessage);
        }
    }
}

using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using HabitTracker.Functions.Models;

namespace HabitTracker.Functions
{
    public class StartCommand
    {
        public static async Task StartCommandAsync(TelegramBotClient botClient, Update update, ILogger _logger, ReplyKeyboardMarkup replyKeyboardMarkup, HabitTracker.Functions.Models.UserData currentUser)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");


            if (currentUser != null)
            {
                string responseMessage = currentUser.Language == "Uz" ? "Xush kelibsiz" : "Добро пожаловать";
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: responseMessage,
                    replyMarkup: replyKeyboardMarkup,
                    parseMode: ParseMode.Markdown
                );
            }
            else
            {
                var languageChooser = new LanguageChooser();
                // Ask for nickname
                await languageChooser.LanguageChooserAsync(update, botClient, "waiting_for_language", "Til/Язык");
                
                
            }
        }
    }
}

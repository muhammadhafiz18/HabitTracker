using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using HabitTracker.Functions.Models;

namespace HabitTracker.Functions
{
    public class LanguageChooser
    {
        public async Task LanguageChooserAsync(Update update, TelegramBotClient botClient, string currentState, string message) 
        {
            var userStatee = new UserStatee(); 
            var userState = new UserState();

            userStatee.UserId = update.Message.Chat.Id;
            userStatee.State = currentState;

            await userState.UserDetailAdder(userStatee);

            List<string> languageKeyboardLabels = new List<string>() { "🇺🇿 O'zbekcha", "🇷🇺 Русский" };
            var languageKeyboard = KeyboardBuilder.BuildCustomKeyboard(languageKeyboardLabels);

            string responseMessage = message;
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: responseMessage,
                replyMarkup: languageKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
    }
}

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
    public class ViewHabitCommand
    {
        public static async Task ViewHabitCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, ReplyKeyboardMarkup inlineKeyboard, HabitTracker.Functions.Models.UserData currentUserData)
        {
            string text;
            long chatId;
            if (update.Message != null)
            {
                text = update.Message.Text;
                chatId = update.Message.Chat.Id;
            }
            else if (update.CallbackQuery != null)
            {
                text = update.CallbackQuery.Data;
                chatId = update.CallbackQuery.Message.Chat.Id;
            }
            else
            {
                text = "nothing";
                chatId = 0;
            }


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
            var message = new StringBuilder();


            for (int i = 1; i < currentUserData.Habits.Count(); i++)
            {
                string habitStatusUz = currentUserData.Habits[i].isCompleted ? "Ushbu odatga ko'nikilgan" : "Ushbu odatga konikilinmagan";
                string habitStatusRu = currentUserData.Habits[i].isCompleted ? "Этот навык освоен" : "Этот навык не освоен";

                if (currentUserData.Language == "Uz")
                {
                    message.AppendLine($"*{currentUserData.Habits[i].HabitId}.* *Nomi:* {currentUserData.Habits[i].Name}\n" +
                                       $"*Bajarilish oralig'i:* {currentUserData.Habits[i].FrequencyUz}\n" +
                                       $"*Eslatma vaqti:* Har *{currentUserData.Habits[i].FrequencyWithNumbers}* kunda soat *{currentUserData.Habits[i].AlertTime}* da\n" +
                                       $"*Holat:* {habitStatusUz}");
                }
                else
                {
                    message.AppendLine($"*{currentUserData.Habits[i].HabitId}. Названия:* {currentUserData.Habits[i].Name}\n" +
                                       $"*Регулярность:* {currentUserData.Habits[i].FrequencyRu}\n" +
                                       $"*Время оповещения:* на *{currentUserData.Habits[i].AlertTime}* каждый *{currentUserData.Habits[i].FrequencyWithNumbers}* день\n" +
                                       $"*Статус:* {habitStatusRu}");
                }
                message.AppendLine("-----------------------------");
            }

            await BotClient.SendTextMessageAsync(
            chatId: chatId,
            text: message.ToString(),
            replyMarkup: inlineKeyboard,
            parseMode: ParseMode.Markdown);

        }
    }
}

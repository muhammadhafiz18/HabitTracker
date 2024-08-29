using HabitTracker.Functions.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HabitTracker.Functions
{
    public class AddHabitCommand
    {
        public static async Task AddHabitCommandAsync(TelegramBotClient BotClient, Update update, ILogger _logger, UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            string backToMainMenuLabel = currentUserData.Language == "Uz" ? "🏠 Asosiy menyuga qaytish" : "🏠 Вернуться в Главное Меню";

            List<string> keyboardOfAddHabitLabels = new List<string>() { backToMainMenuLabel };

            var keyboardOfAddHabit = KeyboardBuilder.BuildCustomKeyboard(keyboardOfAddHabitLabels);

            string responseMessage = currentUserData.Language == "Uz" ? "Iltimos shakillantirmoqchi bo'lgan odatingizning nomini kiriting:" : "Пожалуйста, введите название новой привычки: ";

            var userState = new UserState();
            var userStateData = new UserStatee()
            {
                UserId = update.Message.Chat.Id,
                State = "waiting_for_new_habit_name"
            };

            await userState.UserDetailAdder(userStateData);

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: responseMessage,
                replyMarkup: keyboardOfAddHabit,
                parseMode: ParseMode.Markdown
            );
        }
        public static async Task AddHabitCommandSecondStepAsync(TelegramBotClient BotClient, Update update, ILogger _logger, UserData currentUserData)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            string backToMainMenuLabel = currentUserData.Language == "Uz" ? "🏠 Asosiy menyuga qaytish" : "🏠 Вернуться в Главное Меню";

            List<string> AddHabitFrequenciesUzbek = new List<string>() { "Har kuni", "Ikki kunda bir", "Uch kunda bir", "To'rt kunda bir", "Besh kunda bir", "Olti kunda bir", "Bir haftada bir", "Ikki haftada bir", "Bir oyda bir" };
            List<string> AddHabitFrequenciesRussian = new List<string>() { "Один раз в день", "Один раз в два дня", "Один раз в три дня", "Один раз в четыре дня", "Один раз в пять дня", "Один раз в шесть дня", "Один раз в неделю", "Один раз в две недели", "Один раз в месяц" };

            var keyboardOfAddHabit = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? AddHabitFrequenciesUzbek : AddHabitFrequenciesRussian);

            string responseMessage = currentUserData.Language == "Uz" ? "Iltimos shakillantirmoqchi bo'lgan odatingizning bajarilishi kerak bo'lgan muddatini tanlang)" : "Пожалуйста, выберите регулярность привычки";

            var userState = new UserState();
            var userStateData = new UserStatee()
            {
                UserId = update.Message.Chat.Id,
                State = "waiting_for_new_habit_frequency",
                NewHabitName = update.Message.Text
            };

            await userState.UserDetailAdder(userStateData);

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: responseMessage,
                replyMarkup: keyboardOfAddHabit,
                parseMode: ParseMode.Markdown
            );
        }
        public static async Task AddHabitCommandThirdStepAsync(TelegramBotClient BotClient, Update update, ILogger _logger, UserData currentUserData, UserStatee currentUserState)
        {
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            _logger.LogInformation($"{text} is received from {chatId}");

            string backToMainMenuLabel = currentUserData.Language == "Uz" ? "🏠 Asosiy menyuga qaytish" : "🏠 Вернуться в Главное Меню";

            List<string> keyboardOfAddHabitLabels = new List<string>() { "00:00 🕛", "00:30 🕧", "01:00 🕐", "01:30 🕜", "02:00 🕑", "02:30 🕝",
                                                                        "03:00 🕒", "03:30 🕞", "04:00 🕓", "04:30 🕟", "05:00 🕔", "05:30 🕠",
                                                                        "06:00 🕕", "06:30 🕡", "07:00 🕖", "07:30 🕢", "08:00 🕗", "08:30 🕣",
                                                                        "09:00 🕘", "09:30 🕤", "10:00 🕙", "10:30 🕥", "11:00 🕚", "11:30 🕦",
                                                                        "12:00 🕛", "12:30 🕧", "13:00 🕐", "13:30 🕜", "14:00 🕑", "14:30 🕝",
                                                                        "15:00 🕒", "15:30 🕞", "16:00 🕓", "16:30 🕟", "17:00 🕔", "17:30 🕠",
                                                                        "18:00 🕕", "18:30 🕡", "19:00 🕖", "19:30 🕢", "20:00 🕗", "20:30 🕣",
                                                                        "21:00 🕘", "21:30 🕤", "22:00 🕙", "22:30 🕥", "23:00 🕚", "23:30 🕦", backToMainMenuLabel };
            
            var keyboardOfAddHabit = KeyboardBuilder.BuildCustomKeyboard(keyboardOfAddHabitLabels);

            string responseMessage = currentUserData.Language == "Uz" ? "Ushbu vaqtlarda sizga eslatma yuboriladi.\n\nTanlang:" : "Вы получите уведомление в это время.\n\n Пожалуйста, выберите один";

            var userState = new UserState();
            var userStateData = new UserStatee()
            {
                UserId = update.Message.Chat.Id,
                State = "waiting_for_new_habit_frequency_time",
                NewHabitName = currentUserState.NewHabitName,
                NewHabitFrequency = update.Message.Text
            };

            await userState.UserDetailAdder(userStateData);

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: responseMessage,
                replyMarkup: keyboardOfAddHabit,
                parseMode: ParseMode.Markdown
            );
        }

    }
}

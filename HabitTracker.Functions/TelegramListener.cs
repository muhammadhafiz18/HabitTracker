using HabitTracker.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HabitTracker.Functions
{
    public class TelegramListener
    {
        private static readonly string BotToken = "YourBotTokenHere";
        private static readonly TelegramBotClient BotClient = new TelegramBotClient(BotToken);
        private readonly ILogger<TelegramListener> _logger;

        public TelegramListener(ILogger<TelegramListener> logger)
        {
            _logger = logger;
        }

        [Function("TelegramListener")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();

            var update = JsonConvert.DeserializeObject<Update>(json);

            await ProcessUpdate(update);

            return new OkResult();
        }

        private async Task ProcessUpdate(Update update)
        {
            var userDetails = new UserDetails();
            List<UserData> userDataFromJson = await userDetails.UserDetailGetter();

            var userState = new UserState();
            long chatId;
            if (update.Message != null)
            {
                chatId = update.Message.Chat.Id;
            } 
            else if (update.CallbackQuery != null)
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
            } 
            else
            {
                chatId = 0;
            }

            List<UserStatee> userStatesFromJson = await userState.UserDetailGetter();
            var currentUserState = userStatesFromJson.FirstOrDefault(c => c.UserId == chatId);
            var currentUserData = userDataFromJson.FirstOrDefault(c => long.Parse(c.UserId) == chatId);

            var languageChooser = new LanguageChooser();
            List<string> mainMenuKeyboardLabelsInRussian = new List<string>() { "➕ Добавить Новую Привычку", "✔️ Привык к Привычке", "🔄 Начать привыкать к привычке заново", "❌ Удалить Привычку", "📄 Посмотреть Привычки", "📊 Посмотреть Статистику Привычек", "📃 Руководство", "📊 Посмотреть Статистику Приложения", "🇷🇺 Изменить язык", "✍️ Для отзывов и жалоб" };
            List<string> mainMenuKeyboardLabelsInUzbek = new List<string>() { "➕ Yangi odat qo'shish", "✔️ Odatga ko'nikildi", "🔄 Odatga ko'nikishni boshqattan boshlash", "❌ Odatni o'chirib tashlash", "📄 Odatlarni ko'rish", "📊 Statistikani ko'rish", "📃 Qo'llanma", "📊 Umumiy foydalanuvchilar soni", "🇺🇿 Til o'zgartirish", "✍️ Taklif va shikoyatlar uchun" };

            ReplyKeyboardMarkup mainMenuKeyboard;

            if (currentUserData != null)
            {
                mainMenuKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? mainMenuKeyboardLabelsInUzbek : mainMenuKeyboardLabelsInRussian);
            }
            else if (currentUserState?.Langauge != null)
            {
                mainMenuKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserState.Langauge == "Uz" ? mainMenuKeyboardLabelsInUzbek : mainMenuKeyboardLabelsInRussian);
            } 
            else
            {
                mainMenuKeyboard = KeyboardBuilder.BuildCustomKeyboard(mainMenuKeyboardLabelsInUzbek);
            }


            if (update.Message != null)
            {
                if (currentUserState != null && currentUserState.State == "waiting_for_language")
                {
                    string language;

                    if (update.Message.Text == "🇷🇺 Русский")
                    {
                        language = "Ru";
                        await AfterGettingLanguage(BotClient, update, currentUserState, language);
                        await AskForNicknameAsync(BotClient, update.Message.Chat.Id, language);

                    }
                    else if (update.Message.Text == "🇺🇿 O'zbekcha")
                    {
                        language = "Uz";
                        await AfterGettingLanguage(BotClient, update, currentUserState, language);
                        await AskForNicknameAsync(BotClient, update.Message.Chat.Id, language);
                    }
                    else
                    {
                        await userState.UserDetailRemover(currentUserState);

                        await languageChooser.LanguageChooserAsync(update, BotClient, "waiting_for_language", "Iltimos til tanlang! | Пожалуйста выберите язый!");
                    }
                }
                else if (update.Message.Text == "🏠 Asosiy menyuga qaytish" || update.Message.Text == "🏠 Вернуться в Главное Меню")
                {
                    await userState.UserDetailRemover(currentUserState);

                    await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: currentUserData.Language == "Uz" ? "Asosiy menyu" : "Главное меню",
                            replyMarkup: mainMenuKeyboard,
                            parseMode: ParseMode.Markdown
                        );
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_nickname")
                {
                    if (update.Message.Text != null)
                    {
                        string responseMessage = currentUserState.Langauge == "Uz" ? $"Assalomu alaykum {update.Message.Text}!\nUshbu Telegram Botning qo'llanmasi:\n\nhttps://telegra.ph/Habit-Tracker-qollanmasi-08-09" : $"Привет {update.Message.Text}. Это руководство для этого бота Telegram:\n\nhttps://telegra.ph/Rukovodstvo-dlya-Habit-Tracker-08-09";
                        await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: responseMessage,
                            replyMarkup: mainMenuKeyboard,
                            parseMode: ParseMode.Markdown
                        );
                        userState.UserDetailRemover(currentUserState);
                        var userDetailBuilder = new UserDetailBuilder();

                        var newUser = userDetailBuilder.UserDataBuilder(update.Message.Chat.Id, update.Message.Text, currentUserState.Langauge);
                        _logger.LogInformation($"{newUser.UserName} {newUser.UserId} New joined!!!");
                        await userDetails.UserDetailAdder(newUser);
                    }
                    else
                    {
                        string language = currentUserState.Langauge;
                        await AfterGettingLanguage(BotClient, update, currentUserState, language);
                        await AskForNicknameAsync(BotClient, update.Message.Chat.Id, language);
                    }
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_new_language")
                {
                    string language;

                    if (update.Message.Text == "🇷🇺 Русский")
                    {
                        language = "Ru";

                        await userDetails.UserDetailRemover(currentUserData);

                        currentUserData.Language = language;

                        await userDetails.UserDetailAdder(currentUserData);

                        await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Ваш новый язык: Русский",
                            replyMarkup: KeyboardBuilder.BuildCustomKeyboard(mainMenuKeyboardLabelsInRussian),
                            parseMode: ParseMode.Markdown
                        );
                        await userState.UserDetailRemover(currentUserState);

                    }
                    else if (update.Message.Text == "🇺🇿 O'zbekcha")
                    {
                        language = "Uz";

                        currentUserData.Language = language;

                        await userDetails.UserDetailRemover(currentUserData);

                        currentUserData.Language = language;

                        await userDetails.UserDetailAdder(currentUserData);

                        await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: "Yangi til: O'zbekcha",
                            replyMarkup: KeyboardBuilder.BuildCustomKeyboard(mainMenuKeyboardLabelsInUzbek),
                            parseMode: ParseMode.Markdown
                        );
                        await userState.UserDetailRemover(currentUserState);

                    }
                    else
                    {
                        await userState.UserDetailRemover(currentUserState);

                        await languageChooser.LanguageChooserAsync(update, BotClient, "waiting_for_new_language", currentUserData.Language == "Uz" ? "Iltimos yangi til tanlang" : "Пожалуйста выберите новый язык");
                    }
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_new_habit_name")
                {
                    await userState.UserDetailRemover(currentUserState);
                    await AddHabitCommand.AddHabitCommandSecondStepAsync(BotClient, update, _logger, currentUserData);
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_new_habit_frequency")
                {
                    List<string> AddHabitFrequenciesUzbek = new List<string>() { "Har kuni", "Ikki kunda bir", "Uch kunda bir", "To'rt kunda bir", "Besh kunda bir", "Olti kunda bir", "Bir haftada bir", "Ikki haftada bir", "Bir oyda bir" };
                    List<string> AddHabitFrequenciesRussian = new List<string>() { "Один раз в день", "Один раз в два дня", "Один раз в три дня", "Один раз в четыре дня", "Один раз в пять дня", "Один раз в шесть дня", "Один раз в неделю", "Один раз в две недели", "Один раз в месяц" };

                    if (AddHabitFrequenciesUzbek.Contains(update.Message.Text) || AddHabitFrequenciesRussian.Contains(update.Message.Text))
                    {
                        await userState.UserDetailRemover(currentUserState);

                        await AddHabitCommand.AddHabitCommandThirdStepAsync(BotClient, update, _logger, currentUserData, currentUserState);
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: currentUserData.Language == "Uz" ? "Iltimos birini tanlang: " : "пожалуйста выберите один: ",
                            parseMode: ParseMode.Markdown
                        );
                        await userState.UserDetailRemover(currentUserState);
                        await AddHabitCommand.AddHabitCommandSecondStepAsync(BotClient, update, _logger, currentUserData);
                    }

                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_new_habit_frequency_time")
                {
                    List<string> halfHourTimes = new List<string>() { "05:10", "00:00 🕛", "00:30 🕧", "01:00 🕐", "01:30 🕜", "02:00 🕑", "02:30 🕝",
                                                                        "03:00 🕒", "03:30 🕞", "04:00 🕓", "04:30 🕟", "05:00 🕔", "05:30 🕠",
                                                                        "06:00 🕕", "06:30 🕡", "07:00 🕖", "07:30 🕢", "08:00 🕗", "08:30 🕣",
                                                                        "09:00 🕘", "09:30 🕤", "10:00 🕙", "10:30 🕥", "11:00 🕚", "11:30 🕦",
                                                                        "12:00 🕛", "12:30 🕧", "13:00 🕐", "13:30 🕜", "14:00 🕑", "14:30 🕝",
                                                                        "15:00 🕒", "15:30 🕞", "16:00 🕓", "16:30 🕟", "17:00 🕔", "17:30 🕠",
                                                                        "18:00 🕕", "18:30 🕡", "19:00 🕖", "19:30 🕢", "20:00 🕗", "20:30 🕣",
                                                                        "21:00 🕘", "21:30 🕤", "22:00 🕙", "22:30 🕥", "23:00 🕚", "23:30 🕦" };

                    List<string> AddHabitFrequenciesUzbek = new List<string>() { "Har kuni", "Ikki kunda bir", "Uch kunda bir", "To'rt kunda bir", "Besh kunda bir", "Olti kunda bir", "Bir haftada bir", "Ikki haftada bir", "Bir oyda bir" };
                    List<string> AddHabitFrequenciesRussian = new List<string>() { "Один раз в день", "Один раз в два дня", "Один раз в три дня", "Один раз в четыре дня", "Один раз в пять дня", "Один раз в шесть дня", "Один раз в неделю", "Один раз в две недели", "Один раз в месяц" };

                    int indexOfHabitFrequency = currentUserData.Language == "Uz" ? AddHabitFrequenciesUzbek.IndexOf(currentUserState.NewHabitFrequency) : AddHabitFrequenciesRussian.IndexOf(currentUserState.NewHabitFrequency);
                    int tillAlertDayLeft;

                    switch (indexOfHabitFrequency)
                    {
                        case 0:
                            tillAlertDayLeft = 1;
                            break;

                        case 1:
                            tillAlertDayLeft = 2;
                            break;

                        case 2:
                            tillAlertDayLeft = 3;
                            break;

                        case 3:
                            tillAlertDayLeft = 4;
                            break;

                        case 4:
                            tillAlertDayLeft = 5;
                            break;

                        case 5:
                            tillAlertDayLeft = 6;
                            break;

                        case 6:
                            tillAlertDayLeft = 7;
                            break;

                        case 7:
                            tillAlertDayLeft = 14;
                            break;

                        case 8:
                            tillAlertDayLeft = 30;
                            break;
                        default:
                            tillAlertDayLeft = 0;
                            break;
                    }

                    if (halfHourTimes.Contains(update.Message.Text))
                    {
                        string timeForAlert = update.Message.Text.Substring(0, 5);

                        await userState.UserDetailRemover(currentUserState);

                        await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: currentUserData.Language == "Uz" ? $"*Yangi odat nomi:* {currentUserState.NewHabitName}\n*Yangi odat qilinishining vaqt oralig'i:* {currentUserState.NewHabitFrequency}\n\n*Eslatma vaqti:* Har *{tillAlertDayLeft}* kunda soat *{timeForAlert}* da" : $"*Название новой привычки:* {currentUserState.NewHabitName}\n\n*Регулярность нового привички:* {currentUserState.NewHabitFrequency}\n\n*Уводемления* на *{timeForAlert}* каждый *{tillAlertDayLeft}* дней",
                            replyMarkup: mainMenuKeyboard,
                            parseMode: ParseMode.Markdown
                        );

                        string timeZoneId = "Central Asia Standard Time";

                        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                        // Get the current UTC time
                        DateTime utcNow = DateTime.UtcNow;

                        // Convert UTC time to local time in Uzbekistan
                        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
                        DateTime oneHourBefore = localTime.AddHours(-1);
                        string timeFormat = "HH:mm:ss";
                        string formattedTime = oneHourBefore.ToString(timeFormat);

                        int forChangingTillAlertDayLeft = 0;

                        TimeSpan habitTime = TimeSpan.Parse(timeForAlert+":00");
                        TimeSpan currentTime = TimeSpan.Parse(formattedTime);

                        if (habitTime > currentTime)
                        {
                            // Habit time is greater than current time
                            forChangingTillAlertDayLeft = 1;
                        }

                        Habit habit = new Habit
                        {
                            HabitId = Convert.ToString(int.Parse(currentUserData.Habits[^1].HabitId) + 1),
                            Name = currentUserState.NewHabitName,
                            FrequencyRu = AddHabitFrequenciesRussian[indexOfHabitFrequency],
                            FrequencyUz = AddHabitFrequenciesUzbek[indexOfHabitFrequency],
                            FrequencyWithNumbers = Convert.ToString(tillAlertDayLeft),
                            AlertTime = timeForAlert+":00",
                            TillAlertDayLeft = Convert.ToString(tillAlertDayLeft-forChangingTillAlertDayLeft)
                        };

                        await userDetails.UserDetailRemover(currentUserData);

                        currentUserData.Habits.Add(habit);
                        _logger.LogInformation($"{currentUserData.UserName} {currentUserData.UserId} added new habit {habit.Name} with frequency {habit.FrequencyRu}");
                        await userDetails.UserDetailAdder(currentUserData);
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(
                            chatId: update.Message.Chat.Id,
                            text: currentUserData.Language == "Uz" ? "Iltimos biror bir vaqtni tanlang: " : "пожалуйста выберите один: ",
                            parseMode: ParseMode.Markdown
                        );
                        await userState.UserDetailRemover(currentUserState);
                        await AddHabitCommand.AddHabitCommandThirdStepAsync(BotClient, update, _logger, currentUserData, currentUserState);
                    }
                }
                else if (currentUserState != null && currentUserState.State == "Waiting_for_habit_name_for_removing")
                {
                    await userState.UserDetailRemover(currentUserState);

                    List<string> allHabits = new List<string>();

                    for (int i = 1; i < currentUserData.Habits.Count(); i++)
                    {
                        allHabits.Add($"{currentUserData.Habits[i].HabitId}. {currentUserData.Habits[i].Name}");
                    }

                    if (allHabits.Contains(update.Message.Text))
                    {

                        int habitIdForRemoving = int.Parse(update.Message.Text.Substring(0, 1));
                        var message = new StringBuilder();

                        var currentUserHabitForRemoving = currentUserData.Habits.FirstOrDefault(c => long.Parse(c.HabitId) == habitIdForRemoving);

                        string habitStatusUz = currentUserHabitForRemoving.isCompleted ? "Ushbu odatga ko'nikilgan" : "Ushbu odatga konikilinmagan";
                        string habitStatusRu = currentUserHabitForRemoving.isCompleted ? "Этот навык освоен" : "Этот навык не освоен";

                        if (currentUserData.Language == "Uz")
                        {
                            message.AppendLine($"*{currentUserHabitForRemoving.HabitId}.* *Nomi:* {currentUserHabitForRemoving.Name}\n" +
                                               $"*Bajarilish oralig'i:* {currentUserHabitForRemoving.FrequencyUz}\n" +
                                               $"*Eslatma vaqti:* Har *{currentUserHabitForRemoving.FrequencyWithNumbers}* kunda soat *{currentUserHabitForRemoving.AlertTime}* da\n" +
                                               $"*Holat:* {habitStatusUz}");
                        }
                        else
                        {
                            message.AppendLine($"*{currentUserHabitForRemoving.HabitId}. Названия:* {currentUserHabitForRemoving.Name}\n" +
                                               $"*Регулярность:* {currentUserHabitForRemoving.FrequencyRu}\n" +
                                               $"*Время оповещения:* на *{currentUserHabitForRemoving.AlertTime}* каждый *{currentUserHabitForRemoving.FrequencyWithNumbers}* день\n" +
                                               $"*Статус:* {habitStatusRu}");
                        }
                        message.AppendLine("-----------------------------");
                        message.AppendLine(currentUserData.Language == "Uz" ? "\n\nRostanham o'chirmoqchimisiz?" : "\n\nХотите ли вы удалить?");

                        List<string> yesAndNoUz = new List<string> { "Ha", "Yo'q" };
                        List<string> yesAndNoRu = new List<string> { "Да", "Нет" };

                        ReplyKeyboardMarkup yesAndNoKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? yesAndNoUz : yesAndNoRu);

                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: message.ToString(),
                                replyMarkup: yesAndNoKeyboard,
                                parseMode: ParseMode.Markdown
                        );

                        currentUserState.State = "waiting_for_confirmation_for_removing_habit";
                        currentUserState.HabitIdForRemoving = $"{habitIdForRemoving}";

                        await userState.UserDetailAdder(currentUserState);
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? $"Iltimos o'chirish uchun odatlardan birini tanlang!" : $"Пожалуйста выберите одного",
                                parseMode: ParseMode.Markdown
                            );
                        await RemoveHabitCommand.RemoveHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                    }
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_confirmation_for_removing_habit")
                {
                    List<string> yesAndNoUz = new List<string> { "Ha", "Yo'q" };
                    List<string> yesAndNoRu = new List<string> { "Да", "Нет" };
                    await userState.UserDetailRemover(currentUserState);

                    if (yesAndNoUz.Contains(update.Message.Text) || yesAndNoRu.Contains(update.Message.Text))
                    {
                        if (update.Message.Text == "Да" || update.Message.Text == "Ha")
                        {
                            await userDetails.UserDetailRemover(currentUserData);

                            currentUserData.Habits.RemoveAll(habit => habit.HabitId == currentUserState.HabitIdForRemoving);

                            await userDetails.UserDetailAdder(currentUserData);

                            await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? "Muvaffaqiyatli o'chirildi" : "Успешно удалено",
                                replyMarkup: mainMenuKeyboard,
                                parseMode: ParseMode.Markdown
                            );
                        }
                        else
                        {
                            await RemoveHabitCommand.RemoveHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                        }
                    }
                    else
                    {
                        ReplyKeyboardMarkup yesAndNoKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? yesAndNoUz : yesAndNoRu);

                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? "*Iltimos \"Ha\" yoki \"Yo'q\" ni tanlang*" : "*Пожалуйста выберите \"Да\" или \"Нет\"*",
                                replyMarkup: yesAndNoKeyboard,
                                parseMode: ParseMode.Markdown
                            );
                    }
                }
                else if (currentUserState != null && currentUserState.State == "Waiting_for_habit_name_that_used_to")
                {
                    await userState.UserDetailRemover(currentUserState);

                    List<string> allHabits = new List<string>();

                    for (int i = 1; i < currentUserData.Habits.Count(); i++)
                    {
                        allHabits.Add($"{currentUserData.Habits[i].HabitId}. {currentUserData.Habits[i].Name}");
                    }

                    if (allHabits.Contains(update.Message.Text))
                    {

                        int habitIdForRemoving = int.Parse(update.Message.Text.Substring(0, 1));
                        var message = new StringBuilder();

                        var currentUserHabitForRemoving = currentUserData.Habits.FirstOrDefault(c => long.Parse(c.HabitId) == habitIdForRemoving);

                        string habitStatusUz = currentUserHabitForRemoving.isCompleted ? "Ushbu odatga ko'nikilgan" : "Ushbu odatga konikilinmagan";
                        string habitStatusRu = currentUserHabitForRemoving.isCompleted ? "Этот навык освоен" : "Этот навык не освоен";

                        if (currentUserData.Language == "Uz")
                        {
                            message.AppendLine($"*{currentUserHabitForRemoving.HabitId}.* *Nomi:* {currentUserHabitForRemoving.Name}\n" +
                                               $"*Bajarilish oralig'i:* {currentUserHabitForRemoving.FrequencyUz}\n" +
                                               $"*Eslatma vaqti:* Har *{currentUserHabitForRemoving.FrequencyWithNumbers}* kunda soat *{currentUserHabitForRemoving.AlertTime}* da\n" +
                                               $"*Holat:* {habitStatusUz}");
                        }
                        else
                        {
                            message.AppendLine($"*{currentUserHabitForRemoving.HabitId}. Названия:* {currentUserHabitForRemoving.Name}\n" +
                                               $"*Регулярность:* {currentUserHabitForRemoving.FrequencyRu}\n" +
                                               $"*Время оповещения:* на *{currentUserHabitForRemoving.AlertTime}* каждый *{currentUserHabitForRemoving.FrequencyWithNumbers}* день\n" +
                                               $"*Статус:* {habitStatusRu}");
                        }
                        message.AppendLine("-----------------------------");
                        message.AppendLine(currentUserData.Language == "Uz" ? "\n\nShu odatga ko'nikib bo'ldingizmi?" : "\n\nВы привыкли к этой привычке?");

                        List<string> yesAndNoUz = new List<string> { "Ha", "Yo'q" };
                        List<string> yesAndNoRu = new List<string> { "Да", "Нет" };

                        ReplyKeyboardMarkup yesAndNoKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? yesAndNoUz : yesAndNoRu);

                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: message.ToString(),
                                replyMarkup: yesAndNoKeyboard,
                                parseMode: ParseMode.Markdown
                        );

                        currentUserState.State = "waiting_for_confirmation_for_marking_habit_as_used_to";
                        currentUserState.HabitIdForRemoving = $"{habitIdForRemoving}";

                        await userState.UserDetailAdder(currentUserState);
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? $"Iltimos odatlardan birini tanlang" : $"Пожалуйста выберите одного",
                                parseMode: ParseMode.Markdown
                            );
                        await LogHabitCommand.LogHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                    }
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_confirmation_for_marking_habit_as_used_to")
                {
                    List<string> yesAndNoUz = new List<string> { "Ha", "Yo'q" };
                    List<string> yesAndNoRu = new List<string> { "Да", "Нет" };
                    await userState.UserDetailRemover(currentUserState);

                    if (yesAndNoUz.Contains(update.Message.Text) || yesAndNoRu.Contains(update.Message.Text))
                    {
                        if (update.Message.Text == "Да" || update.Message.Text == "Ha")
                        {
                            await userDetails.UserDetailRemover(currentUserData);

                            foreach (var habit in currentUserData.Habits)
                            {
                                if (habit.HabitId == currentUserState.HabitIdForRemoving)
                                {
                                    habit.isCompleted = true;
                                    habit.TillAlertDayLeft = "-1";
                                    break;
                                }
                            }

                            await userDetails.UserDetailAdder(currentUserData);

                            await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? $"Ushbu odatga ko'nikkaningiz bilan tabriklaymiz {currentUserData.UserName}!" : $"Поздравляем с привыканием к этой привычке {currentUserData.UserName}!",
                                replyMarkup: mainMenuKeyboard,
                                parseMode: ParseMode.Markdown
                            );
                        }
                        else
                        {
                            await LogHabitCommand.LogHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                        }
                    }
                    else
                    {
                        ReplyKeyboardMarkup yesAndNoKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? yesAndNoUz : yesAndNoRu);

                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? "*Iltimos \"Ha\" yoki \"Yo'q\" ni tanlang*" : "*Пожалуйста выберите \"Да\" или \"Нет\"*",
                                replyMarkup: yesAndNoKeyboard,
                                parseMode: ParseMode.Markdown
                            );
                    }
                }
                else if (currentUserState != null && currentUserState.State == "Waiting_for_habit_name_that_want_to_do_again")
                {
                    await userState.UserDetailRemover(currentUserState);

                    List<string> allHabits = new List<string>();

                    for (int i = 1; i < currentUserData.Habits.Count(); i++)
                    {
                        allHabits.Add($"{currentUserData.Habits[i].HabitId}. {currentUserData.Habits[i].Name}");
                    }

                    if (allHabits.Contains(update.Message.Text))
                    {
                        int habitIdForRemoving = int.Parse(update.Message.Text.Substring(0, 1));
                        var message = new StringBuilder();

                        var currentUserHabitForRemoving = currentUserData.Habits.FirstOrDefault(c => long.Parse(c.HabitId) == habitIdForRemoving);
                        string habitStatusUz = currentUserHabitForRemoving.isCompleted ? "Ushbu odatga ko'nikilgan" : "Ushbu odatga konikilinmagan";
                        string habitStatusRu = currentUserHabitForRemoving.isCompleted ? "Этот навык освоен" : "Этот навык не освоен";

                        if (currentUserData.Language == "Uz")
                        {
                            message.AppendLine($"*{currentUserHabitForRemoving.HabitId}.* *Nomi:* {currentUserHabitForRemoving.Name}\n" +
                                               $"*Bajarilish oralig'i:* {currentUserHabitForRemoving.FrequencyUz}\n" +
                                               $"*Eslatma vaqti:* Har *{currentUserHabitForRemoving.FrequencyWithNumbers}* kunda soat *{currentUserHabitForRemoving.AlertTime}* da\n" +
                                               $"*Holat:* {habitStatusUz}");
                        }
                        else
                        {
                            message.AppendLine($"*{currentUserHabitForRemoving.HabitId}. Названия:* {currentUserHabitForRemoving.Name}\n" +
                                               $"*Регулярность:* {currentUserHabitForRemoving.FrequencyRu}\n" +
                                               $"*Время оповещения:* на *{currentUserHabitForRemoving.AlertTime}* каждый *{currentUserHabitForRemoving.FrequencyWithNumbers}* день\n" +
                                               $"*Статус:* {habitStatusRu}");
                        }
                        message.AppendLine("-----------------------------");
                        message.AppendLine(currentUserData.Language == "Uz" ? "\n\nShu ishni qilishni boshqattan boshlamoqchimisiz?" : "\n\nВы действительно хотите возобновить эту привычку?");

                        List<string> yesAndNoUz = new List<string> { "Ha", "Yo'q" };
                        List<string> yesAndNoRu = new List<string> { "Да", "Нет" };

                        ReplyKeyboardMarkup yesAndNoKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? yesAndNoUz : yesAndNoRu);

                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: message.ToString(),
                                replyMarkup: yesAndNoKeyboard,
                                parseMode: ParseMode.Markdown
                        );

                        currentUserState.State = "waiting_for_confirmation_for_restarting_habit";
                        currentUserState.HabitIdForRemoving = $"{habitIdForRemoving}";

                        await userState.UserDetailAdder(currentUserState);
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? $"Iltimos odatlardan birini tanlang!" : $"Пожалуйста выберите одного",
                                parseMode: ParseMode.Markdown
                            );
                        await RestartHabit.RestartHabitAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                    }
                }
                else if (currentUserState != null && currentUserState.State == "waiting_for_confirmation_for_restarting_habit")
                {
                    List<string> yesAndNoUz = new List<string> { "Ha", "Yo'q" };
                    List<string> yesAndNoRu = new List<string> { "Да", "Нет" };
                    await userState.UserDetailRemover(currentUserState);

                    if (yesAndNoUz.Contains(update.Message.Text) || yesAndNoRu.Contains(update.Message.Text))
                    {
                        if (update.Message.Text == "Да" || update.Message.Text == "Ha")
                        {
                            await userDetails.UserDetailRemover(currentUserData);

                            foreach (var habit in currentUserData.Habits)
                            {
                                if (habit.HabitId == currentUserState.HabitIdForRemoving)
                                {
                                    habit.isCompleted = false;
                                    habit.TillAlertDayLeft = habit.FrequencyWithNumbers;
                                    break;
                                }
                            }

                            await userDetails.UserDetailAdder(currentUserData);

                            await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? $"Ushbu ishni qayta boshlashga kuch-quvvat tilab qolamiz {currentUserData.UserName}!" : $"Удачи в повторении этой привычки {currentUserData.UserName}!",
                                replyMarkup: mainMenuKeyboard,
                                parseMode: ParseMode.Markdown
                            );
                        }
                        else
                        {
                            await RestartHabit.RestartHabitAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                        }
                    }
                    else
                    {
                        ReplyKeyboardMarkup yesAndNoKeyboard = KeyboardBuilder.BuildCustomKeyboard(currentUserData.Language == "Uz" ? yesAndNoUz : yesAndNoRu);

                        await BotClient.SendTextMessageAsync(
                                chatId: update.Message.Chat.Id,
                                text: currentUserData.Language == "Uz" ? "*Iltimos \"Ha\" yoki \"Yo'q\" ni tanlang*" : "*Пожалуйста выберите \"Да\" или \"Нет\"*",
                                replyMarkup: yesAndNoKeyboard,
                                parseMode: ParseMode.Markdown
                            );
                    }

                }
                else if (update.Message.Text == "/start")
                {
                    await StartCommand.StartCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData!);
                } // done
                else if (update.Message.Text == "/addhabit" || update.Message.Text == "➕ Yangi odat qo'shish" || update.Message.Text == "➕ Добавить Новую Привычку")
                {
                    await AddHabitCommand.AddHabitCommandAsync(BotClient, update, _logger, currentUserData);
                } // done
                else if (update.Message.Text == "/removehabit" || update.Message.Text == "❌ Odatni o'chirib tashlash" || update.Message.Text == "❌ Удалить Привычку")
                {
                    await RemoveHabitCommand.RemoveHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                } // done
                else if (update.Message.Text == "/viewhabits" || update.Message.Text == "📄 Odatlarni ko'rish" || update.Message.Text == "📄 Посмотреть Привычки")
                {
                    await ViewHabitCommand.ViewHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                } // done
                else if (update.Message.Text == "/loghabit" || update.Message.Text == "✔️ Odatga ko'nikildi" || update.Message.Text == "✔️ Привык к Привычке")
                {
                    await LogHabitCommand.LogHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                }
                else if (update.Message.Text == "/restartHabit" || update.Message.Text == "🔄 Odatga ko'nikishni boshqattan boshlash" || update.Message.Text == "🔄 Начать привыкать к привычке заново")
                {
                    await RestartHabit.RestartHabitAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                }
                else if (update.Message.Text == "/language" || update.Message.Text == "🇺🇿 Til o'zgartirish" || update.Message.Text == "🇷🇺 Изменить язык")
                {
                    await LanguageCommand.LanguageCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData.Language);
                } // done
                else if (update.Message.Text == "/manual" || update.Message.Text == "📃 Qo'llanma" || update.Message.Text == "📃 Руководство")
                {
                    await ManualCommand.ManualCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                } // done
                else if (update.Message.Text == "/statistics" || update.Message.Text == "📊 Statistikani ko'rish" || update.Message.Text == "📊 Посмотреть Статистику Привычек")
                {
                    await StatisticCommand.StatisticCommandAsync(BotClient, update, _logger, currentUserData, mainMenuKeyboard);
                }
                else if (update.Message.Text == "/statisticsofapp" || update.Message.Text == "📊 Umumiy foydalanuvchilar soni" || update.Message.Text == "📊 Посмотреть Статистику Приложения")
                {
                    await StatisticOfApp.StatisticOfAppAsync(BotClient, update, _logger, mainMenuKeyboard, userDataFromJson, currentUserData);
                }
                else if (update.Message.Text == "/feedback" || update.Message.Text == "✍️ Taklif va shikoyatlar uchun" || update.Message.Text == "✍️ Для отзывов и жалоб")
                {
                    await FeedbackCommand.FeedbackCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                }

                else
                {
                    _logger.LogInformation($"{update.Message.Text} is received from {update.Message.Chat.Id}");
                }
            } else if (update.CallbackQuery != null) 
            {
                if (update.CallbackQuery.Data == "📄 Odatlarni ko'rish" || update.CallbackQuery.Data == "📄 Посмотреть Привычки")
                {
                    await ViewHabitCommand.ViewHabitCommandAsync(BotClient, update, _logger, mainMenuKeyboard, currentUserData);
                } 
                else if (update.CallbackQuery.Data == "🚀 Ha" || update.CallbackQuery.Data == "🚀 Да")
                {
                    await BotClient.EditMessageReplyMarkupAsync(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        messageId: update.CallbackQuery.Message.MessageId,
                        replyMarkup: null
                        );

                    await BotClient.SendTextMessageAsync(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: currentUserData.Language == "Uz" ? "Tabriklaymiz. Shunday davom eting! 💪" : "Так держать! 💪", 
                        replyMarkup: mainMenuKeyboard
                        );
                }
                else if (update.CallbackQuery.Data == "❌ Yo'q" || update.CallbackQuery.Data == "❌ Нет")
                {
                    await BotClient.EditMessageReplyMarkupAsync(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        messageId: update.CallbackQuery.Message.MessageId,
                        replyMarkup: null
                        );

                    await BotClient.SendTextMessageAsync(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: currentUserData.Language == "Uz" ? "😕 Yaxshi emas.\n\nKeyingi muddat kelguncha ushbu ishni qilishga harakat qiling!" : "😕 Не хорошо. Делайте все возможное до следующего срока",
                        replyMarkup: mainMenuKeyboard
                        );
                }
            }
        }

        public static async Task AskForNicknameAsync(TelegramBotClient botClient, long chatId, string language)
        {
            var removeKeyboard = new ReplyKeyboardRemove();
            string askNicknameMessage = language == "Uz" ? "Iltimos nik kiriting: " : "Напишите ник";
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: askNicknameMessage,
                replyMarkup: removeKeyboard
            );
        }

        public static async Task AfterGettingLanguage(TelegramBotClient botClient, Update update, UserStatee currentUserState, string language)
        {
            var userState = new UserState();

            string responseMessage = language == "Uz" ? "O'zbek tilini tanladingiz" : "Вы выбрали русский язык";
            await BotClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: responseMessage,
                parseMode: ParseMode.Markdown
            );
            var userDetailBuilder = new UserDetailBuilder();


            await userState.UserDetailRemover(currentUserState);
            currentUserState.State = "waiting_for_nickname";
            currentUserState.Langauge = language;
            await userState.UserDetailAdder(currentUserState);
        }
    }
}

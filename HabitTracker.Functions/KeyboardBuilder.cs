using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

public class KeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildCustomKeyboard(List<string> buttonLabels)
    {
        var keyboardButtons = new List<KeyboardButton[]>();

        for (int i = 0; i < buttonLabels.Count; i += 2)
        {
            var row = new List<KeyboardButton>();

            row.Add(new KeyboardButton(buttonLabels[i]));

            if (i + 1 < buttonLabels.Count)
            {
                row.Add(new KeyboardButton(buttonLabels[i + 1]));
            }

            keyboardButtons.Add(row.ToArray());
        }

        var keyboard = new ReplyKeyboardMarkup(keyboardButtons)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        return keyboard;
    }
}

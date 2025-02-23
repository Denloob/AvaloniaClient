using System;
using System.Linq;
using System.Collections.Generic;

using Client;

public class Utils
{
    public static bool IsInteger(string value)
    {
        return value.All(char.IsNumber) && value != "";
    }

    public static void TryOrElseOpenPopup(Action func, Predicate<Exception> shouldDisplayExceptionMessage, ErrorPopup popup)
    {
        try
        {
            func();
        }
        catch (Exception e) when (shouldDisplayExceptionMessage(e))
        {
            popup.Open(e.Message);
        }
        catch (Exception)
        {
            popup.Open("Something went wrong");
        }
    }

    /**
     * @brief Extracts the second element of type T from a list of lists.
     * For example given a list
     * {
     *   {"cat", 123},
     *   {"dog", 321},
     * }
     * The returned list will be {123, 321}
     *
     * @tparam T The type of the second element in the sublist.
     * @param nestedList The nested list to extract from.
     * @return List of the second elements.
     */
    public static List<T> ExtractSecond<T>(List<List<object>> nestedList)
    {
        List<T> result = new List<T>();

        foreach (var item in nestedList)
        {
            if (item.Count > 1 && item[1] is T secondElement)
            {
                result.Add(secondElement);
            }
        }

        return result;
    }
}

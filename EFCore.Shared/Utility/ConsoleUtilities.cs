using System.Text.RegularExpressions;

namespace EFCore.Shared.Utility;
public static class ConsoleUtilities
{
    public static decimal ReadDecimalNumber(string? message = null)
    {
        do
        {
            Console.Write(message);
            string? numberString = Console.ReadLine();
            if (decimal.TryParse(numberString, out var date))
                return date;
            Console.Write("Unable to comprehend input as Decimal value. Try again");
        } while (true);
    }
    public static string ReadPhoneNumber(string? message = null)
    {
        do
        {
            Console.Write(message);
            string? phoneString = Console.ReadLine();
            if (Regex.IsMatch(phoneString ?? string.Empty, @"[0-9]{10}"))
                return phoneString;
            Console.Write("Unable to comprehend input as number value. Try again");
        } while (true);
    }
    public static DateTime ReadDateTime(string? message = null)
    {
        do
        {
            Console.Write(message);
            string? dateString = Console.ReadLine();
            if (DateTime.TryParse(dateString, out var date))
                return date;
            Console.Write("Unable to comprehend input as DateTime value. Try again");
        } while (true);
    }
    public static int ReadNumber(string? message = null)
    {
        do
        {
            Console.Write(message);
            string? numberString = Console.ReadLine();
            if (int.TryParse(numberString, out var date))
                return date;
            Console.Write("Unable to comprehend input as Integer value. Try again");
        } while (true);
    }
    public static string? ReadNames(string? message = null)
    {
        do
        {
            Console.Write(message);
            string? nameString = Console.ReadLine();
            if (Regex.IsMatch(nameString ?? string.Empty, @"[A-ZА-Я][ a-z\-\.а-яA-ZА-Я]+"))
                return nameString;
            Console.Write("Unable to comprehend input as name value. Try again");
        } while (true);
    }

    public static string ReadEmail(string? message = null)
    {
        do
        {
            Console.Write(message);
            string? emailString = Console.ReadLine();
            if (Regex.IsMatch(emailString ?? string.Empty, @"[A-Za-z0-9]+@[A-Za-z]\.[a-z]+"))
                return emailString;
            Console.Write("Unable to comprehend input as name value. Try again");
        } while (true);
    }
}

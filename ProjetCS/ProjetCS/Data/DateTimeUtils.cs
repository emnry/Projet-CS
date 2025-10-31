using System.Globalization;

namespace ProjetCS.Data.InterfaceRepository
{
    public class DateTimeUtils
    {
        public static DateTime ConvertToDateTime(string date, string format)
        {
            if (string.IsNullOrWhiteSpace(date))
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            if (DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime parsed))
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            Console.WriteLine($"La date fournie est mal renseignée ({date})");
            return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        }
    }
}
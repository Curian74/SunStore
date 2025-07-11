namespace SunStore.Helpers
{
    public static class DateFormatHelper
    {
        public static string GetDateFormat(DateTime date)
        {
            var timeDifference = DateTime.Now.Subtract(date);
            string timeAgo;

            if (timeDifference.TotalDays >= 1 && timeDifference.TotalDays < 3)
            {
                timeAgo = $"{(int)timeDifference.TotalDays} ngày trước";
            }
            else if (timeDifference.TotalHours >= 1 && timeDifference.TotalDays < 3)
            {
                timeAgo = $"{(int)timeDifference.TotalHours} giờ trước";
            }
            else if (timeDifference.TotalMinutes >= 1 && timeDifference.TotalDays < 3)
            {
                timeAgo = $"{(int)timeDifference.TotalMinutes} phút trước";
            }

            else if (timeDifference.TotalDays >= 3)
            {
                timeAgo = $"{date.Month}/{date.Day}/{date.Year}";
            }

            else
            {
                timeAgo = "Vừa xong";
            }
            return timeAgo;
        }
    }
}

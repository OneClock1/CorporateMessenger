using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Extensions
{
    public class UtcToUnixConverter : IValueConverter<DateTime, long>
    {
        public static long Convert(DateTime sourceMember)
        {
            TimeSpan span = sourceMember - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long timestamp = (long)span.TotalSeconds;
            return timestamp;
        }

        public long Convert(DateTime sourceMember, ResolutionContext context) =>
            Convert(sourceMember);
    }
}

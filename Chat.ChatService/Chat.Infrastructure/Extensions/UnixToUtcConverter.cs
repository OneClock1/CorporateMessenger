using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Infrastructure.Extensions
{
    public class UnixToUtcConverter : IValueConverter<long, DateTime>, ITypeConverter<long, DateTime>
    {
        public static DateTime Convert(long source)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(source);
            return dateTime;
        }

        public DateTime Convert(long source, ResolutionContext context) =>
            Convert(source);

        public DateTime Convert(long source, DateTime destination, ResolutionContext context) =>
            Convert(source);
    }
}

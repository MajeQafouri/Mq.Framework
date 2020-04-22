using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Core.Mq.Core.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription<T>(this T enumObject) where T : IConvertible
        {
            string description = null;

            if (enumObject is Enum)
            {
                Type type = enumObject.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == enumObject.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }

        public static string GetDisplayName<T>(this T enumObject) where T : IConvertible
        {
            string description = null;

            if (enumObject is Enum)
            {
                Type type = enumObject.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == enumObject.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DisplayNameAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            description = ((DisplayNameAttribute)descriptionAttributes[0]).DisplayName;
                        }

                        break;
                    }
                }
            }

            return description;
        }
    }
}

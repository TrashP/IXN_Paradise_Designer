using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Profiling;

namespace Polyperfect.Common
{
    public static class StringExtensions
    {
        public static string NewlineSpaces(this string that)
        {
            if (string.IsNullOrEmpty(that))
                that = "";
            return that.Replace(' ', '\n');
        }

        //https://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token
        public static string SpacifyCamelCaps(this string that)
        {
            if (string.IsNullOrEmpty(that))
                return that;
            Profiler.BeginSample("Spacify Camelcaps Regex");
            var ret = Regex.Replace(that, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
            Profiler.EndSample();
            return ret;
        }

        public static string Truncate(this string value, int maxChars)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxChars ? value : $"{value.Substring(0, maxChars-1)}…";
        }

        public static string WordwrapOrTruncate(this string that, int maxLength, int maxLines)
        {
            if (string.IsNullOrEmpty(that))
                return that;
            var builder = new StringBuilder(that.Length*2+5);
            var split = that.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
            var curLength = 0;
            var lines = 1;
            foreach ( var item in split)
            {
                if (curLength > 0)
                {
                    var intendedNew = curLength + 1 + item.Length;
                    if (intendedNew <= maxLength)
                    {
                        builder.Append($" {item}");
                        curLength = intendedNew;
                        continue;
                    }

                    if (lines + 1 > maxLines)
                    {
                        if (builder[builder.Length-1]!='…')
                            builder.Append("…");
                        break;
                    }

                    builder.AppendLine();
                    lines++;
                    curLength = 0;
                }

                builder.Append(item.Truncate(maxLength));
                curLength += item.Length;
            }

            return builder.ToString();

        }
    }
    
}
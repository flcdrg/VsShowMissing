using System;
using System.Text.RegularExpressions;

namespace DavidGardiner.Gardiner_VsShowMissing
{
    // http://stackoverflow.com/a/4375058/25702
    internal static class FindFilesPatternToRegex
    {
        private static readonly Regex HasQuestionMarkRegEx   = new Regex(@"\?", RegexOptions.Compiled);
        private static readonly Regex IllegalCharactersRegex  = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
        private static readonly Regex CatchExtentionRegex    = new Regex(@"^\s*.*\.([^\.]+)\s*$", RegexOptions.Compiled);
        private const string NonDotCharacters = @"[^.]*";

        public static Regex Convert(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            pattern = pattern.Trim();
            if (pattern.Length == 0)
            {
                throw new ArgumentException("Pattern is empty.");
            }
            if(IllegalCharactersRegex.IsMatch(pattern))
            {
                throw new ArgumentException("Pattern contains illegal characters.");
            }
            bool hasExtension = CatchExtentionRegex.IsMatch(pattern);
            bool matchExact = false;
            if (HasQuestionMarkRegEx.IsMatch(pattern))
            {
                matchExact = true;
            }
            else if(hasExtension)
            {
                matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
            }
            string regexString = Regex.Escape(pattern);
            regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
            regexString = Regex.Replace(regexString, @"\\\?", ".");
            if(!matchExact && hasExtension)
            {
                regexString += NonDotCharacters;
            }
            regexString += "$";

            if (regexString.StartsWith(@"^\", StringComparison.CurrentCultureIgnoreCase))
                regexString = regexString.Replace(@"^\", @"^.*\");

            Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return regex;
        }
    }
}
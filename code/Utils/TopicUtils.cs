// <copyright file="TopicUtils.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace OBAService.Utils
{
    /// <summary>
    /// Utilities to support publishing topics in Embedded Social
    /// </summary>
    public static class TopicUtils
    {
        /// <summary>
        /// Remove "hashtags" from the input string by adding a space after #
        /// </summary>
        /// <remarks>
        /// If a topic title or text has a string such as #1234, then Embedded Social
        /// interprets that #1234 as a hashtag and indexes it in search. In the case of
        /// OBA app published topics for routes and stops, these strings are not intended
        /// to be interpreted as hashtags. This method provides a hack to address the problem
        /// by introducing a space after the #.
        /// </remarks>
        /// <param name="text">topic title or text</param>
        /// <returns>input string with spaces after the #</returns>
        public static string RemoveHashtags(string text)
        {
            // skip empty strings
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // walk through the string
            int i = text.IndexOf("#", 0);
            while (i > -1 && i < (text.Length - 1))
            {
                // hashtags start with # (i.e. start of string or whitespace followed by #)
                if (i == 0 || char.IsWhiteSpace(text[i - 1]))
                {
                    // hashtags must have 1 non-whitespace character after the #
                    if (!char.IsWhiteSpace(text[i + 1]))
                    {
                        // insert a space
                        text = text.Insert(i + 1, " ");
                    }
                }

                // continue searching from right after the #
                i = text.IndexOf("#", i + 1);
            }

            return text;
        }
    }
}
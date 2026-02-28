using Makale_Analizi;
using System;
using System.Collections.Generic;

namespace Makale_Analizi
{
    public static class JsonDocumentParser
    {
        public static List<ArticleDocument> Parse(string jsonContent)
        {
            var articles = new List<ArticleDocument>();

            jsonContent = jsonContent.Trim();
            if (jsonContent.StartsWith("[")) jsonContent = jsonContent.Substring(1);
            if (jsonContent.EndsWith("]")) jsonContent = jsonContent.Substring(0, jsonContent.Length - 1);

            int braceCount = 0;
            int startIndex = 0;
            bool insideQuote = false;

            for (int i = 0; i < jsonContent.Length; i++)
            {
                if (jsonContent[i] == '"' && (i == 0 || jsonContent[i - 1] != '\\'))
                {
                    insideQuote = !insideQuote;
                }

                if (!insideQuote)
                {
                    if (jsonContent[i] == '{')
                    {
                        if (braceCount == 0) startIndex = i;
                        braceCount++;
                    }
                    else if (jsonContent[i] == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            string objectBlock = jsonContent.Substring(startIndex, i - startIndex + 1);
                            ArticleDocument doc = ParseSingleObject(objectBlock);
                            if (doc != null) articles.Add(doc);
                        }
                    }
                }
            }

            return articles;
        }

        private static ArticleDocument ParseSingleObject(string jsonObject)
        {
            ArticleDocument doc = new ArticleDocument();
            doc.referenced_works = new List<string>();
            doc.authors = new List<string>();

            doc.id = ExtractStringValue(jsonObject, "\"id\":");

            doc.title = ExtractStringValue(jsonObject, "\"title\":");

            string yearStr = ExtractNumberValue(jsonObject, "\"year\":");
            if (int.TryParse(yearStr, out int y)) doc.year = y;

            doc.referenced_works = ExtractStringArray(jsonObject, "\"referenced_works\":");

            doc.authors = ExtractStringArray(jsonObject, "\"authors\":");

            return doc;
        }

        private static string ExtractStringValue(string source, string key)
        {
            int keyIndex = source.IndexOf(key);
            if (keyIndex == -1) return null;

            int startQuote = source.IndexOf("\"", keyIndex + key.Length);
            if (startQuote == -1) return null;

            int endQuote = source.IndexOf("\"", startQuote + 1);
            while (endQuote != -1 && source[endQuote - 1] == '\\')
            {
                endQuote = source.IndexOf("\"", endQuote + 1);
            }

            if (endQuote == -1) return null;

            return source.Substring(startQuote + 1, endQuote - startQuote - 1);
        }

        private static string ExtractNumberValue(string source, string key)
        {
            int keyIndex = source.IndexOf(key);
            if (keyIndex == -1) return "0";

            int startVal = keyIndex + key.Length;
            while (startVal < source.Length && !char.IsDigit(source[startVal]) && source[startVal] != '-')
            {
                startVal++;
            }

            int endVal = startVal;
            while (endVal < source.Length && char.IsDigit(source[endVal]))
            {
                endVal++;
            }

            return source.Substring(startVal, endVal - startVal);
        }

        private static List<string> ExtractStringArray(string source, string key)
        {
            List<string> result = new List<string>();
            int keyIndex = source.IndexOf(key);
            if (keyIndex == -1) return result;

            int openBracket = source.IndexOf("[", keyIndex);
            int closeBracket = source.IndexOf("]", openBracket);

            if (openBracket == -1 || closeBracket == -1) return result;

            string arrayContent = source.Substring(openBracket + 1, closeBracket - openBracket - 1);

            string[] parts = arrayContent.Split(',');
            foreach (var part in parts)
            {
                string clean = part.Trim();
                int firstQuote = clean.IndexOf('"');
                int lastQuote = clean.LastIndexOf('"');

                if (firstQuote != -1 && lastQuote > firstQuote)
                {
                    string value = clean.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result.Add(value);
                    }
                }
            }
            return result;
        }
    }
}
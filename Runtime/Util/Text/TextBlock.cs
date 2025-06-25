using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MAVLinkAPI.Util.Text
{
    public class TextBlock
    {
        public readonly List<string> Lines;

        public int Width => Lines.Select(it => it.Length).DefaultIfEmpty(0).Max();

        private TextBlock(List<string> lines)
        {
            Lines = lines;
        }

        public TextBlock(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Lines = new List<string>();
                return;
            }

            Lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        }

        public TextBlock Indent(
            int indentationLevel = 1,
            int spacesPerIndent = 4,
            bool indentFirstLine = true
        )
        {
            var indentation = new string(' ', indentationLevel * spacesPerIndent);

            var lines = Lines
                .Select((line, index) => index == 0 && !indentFirstLine ? line : indentation + line)
                .ToList();

            return new TextBlock(string.Join(Environment.NewLine, lines));
        }

        public TextBlock PadLeft(
            string firstRow = "|",
            string otherRows = "|"
        )
        {
            var newLines = Lines.Select((line, index) =>
            {
                var prefix = index == 0 ? firstRow : otherRows;
                return prefix + line;
            }).ToList();

            return new TextBlock(newLines);
        }

        public TextBlock ZipRight(TextBlock other)
        {
            var aWidth = Width;
            var maxHeight = Math.Max(Lines.Count, other.Lines.Count);

            var newLines = new List<string>(maxHeight);
            for (var i = 0; i < maxHeight; i++)
            {
                var lineA = i < Lines.Count ? Lines[i] : "";
                var lineB = i < other.Lines.Count ? other.Lines[i] : "";

                newLines.Add(lineA.PadRight(aWidth) + lineB);
            }

            return new TextBlock(newLines);
        }

        public TextBlock ZipLeft(TextBlock other)
        {
            return other.ZipRight(this);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Lines);
        }
    }

    public static class StringExtensions
    {
        public static TextBlock Block(this string text)
        {
            return new TextBlock(text);
        }

        // public static string BlockSelect(this string text, Func<TextBlock, TextBlock> fn)
        // {
        //     return fn(Block(text)).ToString();
        // }

        public static string normaliseLineBreak(this string text)
        {
            if (text == null) return null;
            return Regex.Replace(text, "\r\n?|\n", Environment.NewLine);
        }
    }
}
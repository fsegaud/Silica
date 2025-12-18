namespace Silica.Parser;

using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Markdown
{
    public static string ToHtml(string markdown)
    {
        string content = markdown;

        content = content.Replace("\r\n", "\n");

        IList<string> codeChunks = Markdown.IsolateAndParseCode(ref content);
        IList<string> paths = Markdown.ParseImagesAndIsolatePaths(ref content);
        Dictionary<string, string?> linkReferences = Markdown.ParseLinkReferences(ref content);

        Markdown.LineBreaksFirstPass(ref content);

        Markdown.ParseHeaders(ref content);
        Markdown.ParseHorizontalRules(ref content);
        Markdown.ParseEmphasis(ref content);
        Markdown.ParseLinks(ref content, linkReferences);
        Markdown.ParseCode(ref content);
        Markdown.ParseQuotes(ref content);
        Markdown.ParseTables(ref content);
        Markdown.ParseLists(ref content);

        Markdown.LineBreaksSecondPass(ref content);
        Markdown.RestoreCode(ref content, codeChunks);
        Markdown.RestoreImagePaths(ref content, paths);

        return content;
    }

    private static IList<string> IsolateAndParseCode(ref string content)
    {
        List<string> codeChunks = new List<string>();

        content = Regex.Replace(
            content,
            @"^```(?<lang>[\w\d#+-]+)?(?<text>.*?)^```",
            m =>
            {
                codeChunks.Add($"%%LANG{m.Groups["lang"]}%<br/>" + m.Groups["text"].Value.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\t", "    ").Trim('\n') /*.Replace("\n", "<br/>")*/);
                return $"```\n%CODE{codeChunks.Count - 1}%\n```";
            },
            RegexOptions.Multiline | RegexOptions.Singleline);

        content = Regex.Replace(
            content,
            @"`(?<text>[^`].*?)`",
            m =>
            {
                codeChunks.Add(m.Groups["text"].Value.Replace("<", "&lt;").Replace(">", "&gt;"));
                return $"`%CODE{codeChunks.Count - 1}%`";
            },
            RegexOptions.Multiline);

        for (int i = 0; i < codeChunks.Count; i++)
        {
            Match match = Regex.Match(codeChunks[i], @"(?<=^%%LANG).*?(?=%)");
            codeChunks[i] = Regex.Replace(codeChunks[i], @"^%%LANG.*?%<br/>", _ => string.Empty);
            codeChunks[i] = Helper.FormatCode(codeChunks[i], match.Value);
        }

        return codeChunks;
    }

    private static void RestoreCode(ref string content, IList<string> codeChunks)
    {
        content = Regex.Replace(content, @"%CODE(?<index>\d*)%", m => codeChunks[int.Parse(m.Groups["index"].Value)], RegexOptions.Multiline);
    }

    private static void ParseCode(ref string content)
    {
        content = Regex.Replace(content, @"^```(?<text>.*?)^```", m => $"<pre class=\"code\">{m.Groups["text"]}</pre>", RegexOptions.Multiline | RegexOptions.Singleline);
        content = Regex.Replace(content, @"`(?<text>.*?)`", m => $"<code>{m.Groups["text"]}</code>", RegexOptions.Multiline | RegexOptions.Singleline);
    }

    private static void ParseQuotes(ref string content)
    {
        content = Regex.Replace(content, @"^>\s(?<text>.*)$", m => $"<blockquote>{m.Groups["text"]}</blockquote>", RegexOptions.Multiline);
        content = content.Replace("</blockquote>\n<blockquote>", string.Empty);
    }

    private static Dictionary<string, string?> ParseLinkReferences(ref string content)
    {
        Dictionary<string, string?> linkReferences = new Dictionary<string, string?>();
        MatchCollection matchCollection = Regex.Matches(content, @"^\[(?<ref>.+?)\]:\s(?<url>[^\s<]+)", RegexOptions.Multiline);
        foreach (Match match in matchCollection)
        {
            linkReferences.Add(match.Groups["ref"].Value.ToLowerInvariant(), match.Groups["url"].Value);
            content = content.Replace(match.Value, string.Empty);
        }

        return linkReferences;
    }

    private static void LineBreaksFirstPass(ref string content)
    {
        content = Regex.Replace(content, @"^(?![#`>\s*|_-]).+", m => $"{m}<br/>", RegexOptions.Multiline); // no br
        content = content.Replace("<br/>\n\n", "<br/><br/>\n\n"); // doubles lines for paragraphs
        content = Regex.Replace(content, @"(<br/>)+(?=[^\w\d]*?```)", _ => string.Empty, RegexOptions.Multiline | RegexOptions.Singleline); // lines before code
        content = Regex.Replace(content, @"(<br/>)+(?=[^\w\d]*?>\s)", _ => string.Empty, RegexOptions.Multiline | RegexOptions.Singleline); // lines before quotes
    }

    private static void LineBreaksSecondPass(ref string content)
    {
        content = content.Replace("\r\n", "\n");
        content = Regex.Replace(content, @"(<br/>)+(?=[^\w\d]*?<table>)", _ => string.Empty, RegexOptions.Multiline | RegexOptions.Singleline); // lines before tables
        content = Regex.Replace(content, @"(<br/>)+(?=[^\w\d]*?<ul>)", _ => string.Empty, RegexOptions.Multiline | RegexOptions.Singleline); // lines before ul
        content = Regex.Replace(content, @"(<br/>)+(?=[^\w\d]*?<ol>)", _ => string.Empty, RegexOptions.Multiline | RegexOptions.Singleline); // lines before ol
        content = Regex.Replace(content, @"(?<=<li>.*)(<br/>)+(?=.*</li>)", _ => string.Empty, RegexOptions.Multiline); // lines in li
        content = Regex.Replace(content, @"(<br/>)+(?=</blockquote>)", _ => string.Empty, RegexOptions.Multiline); // lines in quotes
        content = Regex.Replace(content, @"(<br/>)+(?=</h[123456]>)", _ => string.Empty, RegexOptions.Multiline); // lines in h
        content = Regex.Replace(content, @"(<br/>)+(?=\s*?<h[123456]>)", _ => string.Empty, RegexOptions.Multiline); // lines before h
        content = Regex.Replace(content, @"(?<=<img.*?>\s*)<br/>(?=.<img)", _ => string.Empty, RegexOptions.Multiline | RegexOptions.Singleline); // lines between img
    }

    private static IList<string> ParseImagesAndIsolatePaths(ref string content)
    {
        List<string> paths = new List<string>();

        content = Regex.Replace(
            content, 
            "!\\[(?<text>.*?)\\]\\((?<url>.*?)(\\s\"(?<title>.*?)\")?\\)", 
            m =>
            {
                paths.Add(m.Groups["url"].Value);
                return $"<img src=\"%PATH{paths.Count - 1}%\" alt=\"{m.Groups["text"]}\" title=\"{m.Groups["title"]}\">";
            }, 
            RegexOptions.Multiline);

        return paths;
    }

    private static void RestoreImagePaths(ref string content, IList<string> paths)
    {
        content = Regex.Replace(content, @"%PATH(?<index>\d*)%", m => paths[int.Parse(m.Groups["index"].Value)], RegexOptions.Multiline);
    }

    private static void ParseHeaders(ref string content)
    {
        for (int i = 1; i <= 6; ++i)
        {
            int iLocal = i;
            content = Regex.Replace(content, $@"^#{{{i}}}[^#](?<text>.+?)$", m => $"<h{iLocal} id=\"{m.Groups["text"].Value.TrimEnd('\r').TrimEnd('\n').Replace(' ', '-').ToLowerInvariant()}\">{m.Groups["text"].Value.TrimEnd('\r').TrimEnd('\n')}</h{iLocal}>", RegexOptions.Multiline);
        }
    }

    private static void ParseHorizontalRules(ref string content)
    {
        content = Regex.Replace(content, @"^[\*-_]{3,}$", _ => "<hr>", RegexOptions.Multiline);
    }

    private static void ParseEmphasis(ref string content)
    {
        content = Regex.Replace(content, @"\*\*(?<text>.+?)\*\*", m => $"<b>{m.Groups["text"]}</b>", RegexOptions.Multiline);
        content = Regex.Replace(content, @"__(?<text>.+?)__", m => $"<b>{m.Groups["text"]}</b>", RegexOptions.Multiline);
        content = Regex.Replace(content, @"\*(?<text>.+?)\*", m => $"<i>{m.Groups["text"]}</i>", RegexOptions.Multiline);
        content = Regex.Replace(content, @"_(?<text>.+?)_", m => $"<i>{m.Groups["text"]}</i>", RegexOptions.Multiline);
        content = Regex.Replace(content, @"~~(?<text>.+?)~~", m => $"<strike>{m.Groups["text"]}</strike>", RegexOptions.Multiline);
    }

    private static void ParseLinks(ref string content, Dictionary<string, string?> linkReferences)
    {
        content = Regex.Replace(content, "(?<=(^|[^!]))\\[(?<label>.+?)\\]\\((?<url>[^\\s)]*)(\\s\")?(?<title>.*?)\"?\\)", m => $"<a href=\"{m.Groups["url"]}\" title=\"{m.Groups["title"]}\">{m.Groups["label"]}</a>", RegexOptions.Multiline);

        content = Regex.Replace(
            content,
            @"(?<=(^|[^!]))\[(?<label>.+?)\]\[(?<ref>.+?)\]", 
            m =>
            {
                string key = m.Groups["ref"].Value.ToLowerInvariant();
                if (linkReferences.TryGetValue(key, out string? value))
                {
                    return $"<a href=\"{value}\">{m.Groups["label"]}</a>";
                }

                return m.Value;
            },
            RegexOptions.Multiline);

        content = Regex.Replace(
            content,
            @"(?<=(^|[^!]))\[(?<ref>.+?)\][^\[(]", 
            m =>
            {
                string key = m.Groups["ref"].Value.ToLowerInvariant();
                if (linkReferences.TryGetValue(key, out string? value))
                {
                    return $"<a href=\"{value}\">{m.Groups["ref"]}</a>";
                }

                return m.Value;
            },
            RegexOptions.Multiline);

        content = Regex.Replace(content, "(?<=^|[^\\\"(>])https?://[^\\s\\\"><)]+", m => $"<a href=\"{m}\">{m}</a>", RegexOptions.Multiline);
    }

    private static void ParseTables(ref string content)
    {
        bool tableMode = false;
        bool endTable = false;
        string tableReplace = string.Empty;
        string[] cellClass = null!;
        List<string> tableHeader = new List<string>();
        List<string[]> tableLines = new List<string[]>();

        string[] lines = content.Replace("\r\n", "\n").Split('\n');
        for (int li = 0; li < lines.Length; li++)
        {
            string line = lines[li];
            if (!tableMode)
            {
                if (Regex.IsMatch(line, @"^\|.*\|"))
                {
                    tableMode = true;
                    endTable = false;
                    tableReplace = string.Empty;
                    cellClass = null!;
                    tableHeader.Clear();
                    tableLines.Clear();

                    if (li + 1 <= lines.Length && Regex.IsMatch(lines[li + 1], @"\|.*-{3,}.*\|"))
                    {
                        MatchCollection matches = Regex.Matches(line, @"(?<=\|).+?(?=\|)");
                        for (int mi = 0; mi < matches.Count; mi++)
                        {
                            tableHeader.Add(matches[mi].Value.Trim(' '));
                        }
                    }
                    // Useless branch?
                    // else
                    // {
                    //     MatchCollection matches = Regex.Matches(line, @"(?<=\|).+?(?=\|)");
                    //     string[] cells = new string[matches.Count];
                    //     for (int mi = 0; mi < matches.Count; mi++)
                    //     {
                    //         cells[mi] = matches[mi].Value.Trim(' ');
                    //     }
                    // }

                    tableReplace += line + "\n";
                }
            }
            else
            {
                if (Regex.IsMatch(line, @"\|.*-{3,}.*\|"))
                {
                    MatchCollection matches = Regex.Matches(content, @"(?<=\|)[\s:]?-{3,}[\s:]?(?=\|)");
                    cellClass = new string[matches.Count];
                    for (int mi = 0; mi < matches.Count; mi++)
                    {
                        string value = matches[mi].Value;
                        if (value.StartsWith(":") && value.EndsWith(":"))
                        {
                            cellClass[mi] = "center";
                        }
                        else if (value.EndsWith(":"))
                        {
                            cellClass[mi] = "right";
                        }
                        else
                        {
                            cellClass[mi] = null!;
                        }
                    }

                    tableReplace += line + "\n";
                    continue;
                }
                else if (Regex.IsMatch(line, @"^\|.*\|"))
                {
                    MatchCollection matches = Regex.Matches(line, @"(?<=\|).+?(?=\|)");
                    string[] cells = new string[matches.Count];
                    for (int mi = 0; mi < matches.Count; mi++)
                    {
                        cells[mi] = matches[mi].Value.Trim(' ');
                    }

                    tableLines.Add(cells);
                    tableReplace += line + "\n";
                }
                else
                {
                    endTable = true;
                }
            }

            if (li == lines.Length - 1 && tableMode)
            {
                endTable = true;
            }

            if (endTable)
            {
                tableMode = false;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<table>");

                if (tableHeader.Count > 0)
                {
                    sb.AppendLine("<thead><tr>");

                    for (int i = 0; i < tableHeader.Count; i++)
                    {
                        if (cellClass != null && cellClass[i] != null)
                        {
                            sb.AppendLine($"<td class=\"{cellClass[i]}\">{tableHeader[i]}</td>");
                        }
                        else
                        {
                            sb.AppendLine($"<td>{tableHeader[i]}</td>");
                        }
                    }

                    sb.AppendLine("</tr></thead>");
                }

                foreach (string[] values in tableLines)
                {
                    sb.AppendLine($"<tr>");

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (cellClass != null && cellClass[i] != null)
                        {
                            sb.AppendLine($"<td class=\"{cellClass[i]}\">{values[i]}</td>");
                        }
                        else
                        {
                            sb.AppendLine($"<td>{values[i]}</td>");
                        }
                    }

                    sb.AppendLine($"</tr>");
                }

                sb.AppendLine("</table>");

                content = content.Replace(tableReplace.TrimEnd('\n'), sb.ToString());
            }
        }
    }

    private static void ParseLists(ref string content)
    {
        string[] lines = content.Replace("\r\n", "\n").Split('\n');
        for (int li = 0; li < lines.Length; li++)
        {
            if (Regex.IsMatch(lines[li], @"^[\*+-]\s") || Regex.IsMatch(lines[li], @"^\d+.\s"))
            {
                string replace = string.Empty;
                System.Text.StringBuilder sblist = new System.Text.StringBuilder();
                li = ParseListsRecursively(lines, li, sblist, ref replace);
                content = content.Replace(replace.TrimEnd('\n'), sblist.ToString().TrimEnd('\n'));
            }
        }
    }

    private static int ParseListsRecursively(string[] lines, int li, System.Text.StringBuilder sb, ref string replace, int depth = 0)
    {
        string line = lines[li];
        if (Regex.IsMatch(line, $@"^\s{{{depth * 2}}}[\*+-]\s"))
        {
            sb.AppendLine("<ul>");
            replace += line + "\n";
            sb.AppendLine(Regex.Replace(line, @"^\s*[\*+-]\s(?<text>.*)$", m => $"<li>{m.Groups["text"]}</li>", RegexOptions.Multiline));

            while (true)
            {
                if (++li >= lines.Length)
                {
                    break;
                }

                if (Regex.IsMatch(lines[li], $@"^\s{{{depth * 2}}}[\*+-]\s"))
                {
                    replace += lines[li] + "\n";
                    sb.AppendLine(Regex.Replace(lines[li], @"^\s*[\*+-]\s(?<text>.*)$", m => $"<li>{m.Groups["text"]}</li>", RegexOptions.Multiline));
                }
                else if (Regex.IsMatch(lines[li], $@"^\s{{{(depth + 1) * 2}}}[\*+-]\s"))
                {
                    li = ParseListsRecursively(lines, li, sb, ref replace, depth + 1);
                }
                else
                {
                    break;
                }
            }

            sb.AppendLine("</ul>");
            return li - 1;
        }

        if (Regex.IsMatch(line, $@"^\s{{{depth * 2}}}\d+."))
        {
            sb.AppendLine("<ol>");
            replace += line + "\n";
            sb.AppendLine(Regex.Replace(line, @"^\s*\d+.\s(?<text>.*)$", m => $"<li>{m.Groups["text"]}</li>", RegexOptions.Multiline));

            while (true)
            {
                if (++li >= lines.Length)
                {
                    break;
                }

                if (Regex.IsMatch(lines[li], $@"^\s{{{depth * 2}}}\d+.\s"))
                {
                    replace += lines[li] + "\n";
                    sb.AppendLine(Regex.Replace(lines[li], @"^\s*\d+.\s(?<text>.*)$", m => $"<li>{m.Groups["text"]}</li>", RegexOptions.Multiline));
                }
                else if (Regex.IsMatch(lines[li], $@"^\s{{{(depth + 1) * 2}}}\d+.\s"))
                {
                    li = ParseListsRecursively(lines, li, sb, ref replace, depth + 1);
                }
                else
                {
                    break;
                }
            }

            sb.AppendLine("</ol>");
            return li - 1;
        }

        return li + 1;
    }
}

using System.Text.RegularExpressions;

namespace Silica;

public class ObNote
{
    public const int InvalidId = 0;

    public static int NextId = InvalidId + 1;
    
    public int Id { get; private set; } = InvalidId;
    public string Name { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string[] Tags { get; private set; } = [];
    public string[] Aliases { get; private set; } = [];
    public ObNote[] Links { get; private set; } = [];       // TODO
    public ObNote[] BackLinks { get; private set; } = [];   // TODO
    
    // public string UrlName => $"{Id:0000}-{HttpUtility.UrlEncode(Name)}";

    public static ObNote Parse(string nodePath)
    {
        ObNote note = new ObNote
        {
            Id = NextId++,
            Name = System.IO.Path.GetFileNameWithoutExtension(nodePath),
            Path = System.IO.Path.GetRelativePath(Params.ObsidianProjectPath, nodePath).Replace(".md", string.Empty),
            Content = File.ReadAllText(nodePath)
        };

        note.ParseMetadata();
        
        return note;
    }

    public void WriteToDisk(string deployPath)
    {
        // Open html skeleton.
#if DEBUG
        string resPath = "../../../../Res";
#else
        string resPath = "./Res";
#endif
        string finalPath;
        if (Config.Export.ShortUrl)
        {
            finalPath = System.IO.Path.Combine(deployPath, $"{Id:0000}" + ".html");
        }
        else
        {
            finalPath = System.IO.Path.Combine(deployPath, Path + ".html");
        }
        
        string htmlSkeletonPath = System.IO.Path.Combine(resPath, "Html/note.html");
        string cssPath = System.IO.Path.Combine(resPath, $"Css/{Config.Export.CssStyle}");
        
        // Html.
        string htmlContent = File.ReadAllText(htmlSkeletonPath);
        htmlContent = htmlContent.Replace("@NoteName", Name);
        
        // Css.
        if (Config.Export.CssEmbedded)
        {
            string cssContent = File.ReadAllText(cssPath);
            htmlContent = htmlContent.Replace("@Css", $"<style> {cssContent} </style>");
        }
        else
        {
            File.Copy(cssPath, System.IO.Path.Combine(deployPath, "style.css"), true);
            
            string cssRelativePath = System.IO.Path.GetRelativePath(
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(finalPath) ?? string.Empty), 
                deployPath);
            
            cssRelativePath = System.IO.Path.Combine(cssRelativePath, "style.css");
            htmlContent = htmlContent.Replace("@Css", $"<link rel=\"stylesheet\" href=\"{cssRelativePath}\">");
        }
        
        // Markdown.
        string mdToHtml = Parser.Markdown.ToHtml(Content);
        htmlContent = htmlContent.Replace("@NoteContent", mdToHtml);

        // Metadata.
        if (Tags.Length > 0)
        {
            string htmlTags = string.Empty;
            foreach (string tag in Tags)
            {
                htmlTags += $"<span class=\"tag\">{tag}</span> ";
            }
            
            htmlContent = htmlContent.Replace("@NoteTags", htmlTags.TrimEnd(' '));
        }
        else
        {
            // No tag => delete html-related code.
            htmlContent = htmlContent.Replace("<div id=\"tags\">Tags: @NoteTags</div>", string.Empty);
        }
        
        if (Aliases.Length > 0)
        {
            string htmlAliases = string.Empty;
            foreach (string alias in Aliases)
            {
                htmlAliases += $"<span class=\"alias\">{alias}</span> ";
            }
            
            htmlContent = htmlContent.Replace("@NoteAliases", htmlAliases.TrimEnd(' '));
        }
        else
        {
            // No alias => delete html-related code.
            htmlContent = htmlContent.Replace("<div id=\"aliases\">Aliases: @NoteAliases</div>", string.Empty);
        }

        // Write file.
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(finalPath) ?? string.Empty);
        File.WriteAllText(finalPath, htmlContent);
    }

    private void ParseMetadata()
    {
        Match match = Regex.Match(Content, @"---(?:.|\n)+---");
        if (match == null || !match.Success)
        {
            return;
        }

        // Remove metadata from content.
        string matchValue = match.Groups[0].Value;
        Content = Content.Substring(matchValue.Length, Content.Length - matchValue.Length);
        Content = Content.TrimStart('\n');
        
        string[] metadataLines = matchValue.Split('\n');
        for (int index = 0; index < metadataLines.Length; index++)
        {
            // Tags.
            if (metadataLines[index].StartsWith("tags:"))
            {
                index++;
                List<string> tags = [];
                
                while (metadataLines[index].StartsWith("  - ") && index < metadataLines.Length)
                {
                    string data = metadataLines[index].Substring(4, metadataLines[index].Length - 4);
                    tags.Add(data);
                    index++;
                }
                
                Tags = tags.ToArray();
            }
            
            // Aliases.
            if (metadataLines[index].StartsWith("aliases:"))
            {
                index++;
                List<string> aliases = [];
                
                while (metadataLines[index].StartsWith("  - ") && index < metadataLines.Length)
                {
                    string data = metadataLines[index].Substring(4, metadataLines[index].Length - 4);
                    aliases.Add(data);
                    index++;
                }
                
                Aliases = aliases.ToArray();
            }
        }
    }

    public override string ToString()
    {
        return $"[{Id:0000}]{Path}";
    }
}

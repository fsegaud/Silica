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
    public ObNote[] Links { get; private set; } = [];
    public ObNote[] BackLinks { get; private set; } = [];
    
    // public string UrlName => $"{Id:0000}-{HttpUtility.UrlEncode(Name)}";

    public static ObNote Parse(string nodePath)
    {
        ObNote note = new ObNote();
        note.Id = NextId++;
        note.Name = System.IO.Path.GetFileNameWithoutExtension(nodePath);
        note.Path = System.IO.Path.GetRelativePath(Params.ObsidianProjectPath, nodePath).Replace(".md", string.Empty);
        note.Content = File.ReadAllText(nodePath);
        
        

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
        
        // Write file.
        // TODO: Short links.
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(finalPath) ?? string.Empty);
        File.WriteAllText(finalPath, htmlContent);
    }

    public override string ToString()
    {
        return $"[{Id:0000}]{Path}";
    }
}

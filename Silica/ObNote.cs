using System.Web;

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
        string finalPath = System.IO.Path.Combine(deployPath, Path + ".html");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(finalPath) ?? string.Empty);
        File.WriteAllText(finalPath, Content);
    }

    public override string ToString()
    {
        return $"{Id:0000}:{Path}";
    }
}

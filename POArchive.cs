using System.IO.Compression;


//
// Archive Processing
//
internal class POArchive
{
    private ZipArchive archive;

    public POArchive(string fn)
    {
        archive = ZipFile.Open(fn, ZipArchiveMode.Read);
    }

    public List<string> GetFilenames()
    {
        List<string> ret = new List<string>();
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            ret.Add(entry.Name);
        }
        return ret;
    }
}

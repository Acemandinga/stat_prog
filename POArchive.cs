using System.IO.Compression;


//
// Archive Processing
//
internal class POArchive
{
    private ZipArchive archive;
    private string archiveFilename;

    public POArchive(string fn)
    {
        archiveFilename = fn;
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

    // dt: datetime of archive file
    public void Process(History hist)
    {
        // loop thru objects looking for work to do
        var contents = GetFilenames();
        foreach (var filename in contents)
        {
            // Is this content file a PDF? 
            if (Path.GetExtension(filename).ToLower() == ".pdf")
            {
                if (!hist.HasBeenProcessed(archiveFilename, filename))
                {
                    // process pdf file


                    // store in history
                    hist.AddPDFFile(archiveFilename, filename);
                }
            }
        }

        // 
    }
}

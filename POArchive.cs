using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
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

    public void Process(History hist)
    {
        // extract archive to work dir
        ZipFile.ExtractToDirectory(archiveFilename, @"work", true);
        
        // load csv within
        var files = GetFilenames();
        var csvFile = files.Find(s => Path.GetExtension(s).ToLower() == ".csv");
        if (csvFile != null)
        {
            // open csv
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "~",
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", "")
            };
            
            using (var reader = new StreamReader($"work\\{csvFile}"))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<CSVA>();
                var a = 1;
            }

            // process each entry from csv

        }

            // loop thru objects looking for work to do
            //var contents = GetFilenames();
            //foreach (var filename in contents)
            //{
            //    // Is this content file a PDF? 
            //    if (Path.GetExtension(filename).ToLower() == ".pdf")
            //    {
            //        if (!hist.HasBeenProcessed(archiveFilename, filename))
            //        {
            //            // 

            //            // store in history
            //            hist.AddPDFFile(archiveFilename, filename);
            //        }
            //    }
            //}

            // 
        }
    }

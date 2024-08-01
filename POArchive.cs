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

    public async Task<bool> Process(History hist, AWS_S3 awsS3)
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
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", ""),
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            // use csvhelper to parse csv file using config
            using (var reader = new StreamReader($"work\\{csvFile}"))
            using (var csv = new CsvReader(reader, config))
            {
                // here is where I would switch record templates based on something (filename? location? bucket?)
                var records = csv.GetRecords<CSVA>();

                foreach (var result in records)
                {
                    Console.WriteLine($"  PO: {result.GetPO()}");
                    var pdfs = result.GetPDFFiles();
                    foreach (var pdf in pdfs)
                    {
                        if (string.IsNullOrEmpty(pdf))
                            continue;

                        // get filename only
                        var pdfFilename = Path.GetFileName(pdf);

                        Console.Write($"    {pdfFilename} Uploading...");

                        // upload pdf to s3
                        var s3path = $"by-po/{result.GetPO()}/{pdfFilename}";
                        var filePath = $".\\work\\{pdfFilename}";
                        var ulSuccess = await awsS3.PutFileAsync(s3path, filePath);

                        if (ulSuccess)
                        {
                            // add pdf to history
                            var archFnOnly = Path.GetFileName(archiveFilename);
                            hist.AddPDFFile(archFnOnly, pdfFilename);
                        }
                        else
                        {
                            Console.Error.WriteLine($"ERROR uploading to S3. {pdfFilename},{s3path},{filePath}");
                        }
                    }
                }
            }
        }
        else
        {
            Console.Error.WriteLine($"Error processing {archiveFilename} due to no CSV found.");
        }
        return true;
    }
}

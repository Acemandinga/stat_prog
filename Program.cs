using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.S3.Model;
using Helpers;


//
// Main
//
internal class Program
{
    private static History? history;
    private static string? historyFilename;
    private static string historyPath = @"work";
    private static AWS_S3? awsS3;

    private static async Task<int> Main(string[] args)
    {
        // initialize statics
        history = new History();
        awsS3 = new AWS_S3();


        //
        // load .env file for keys
        //
        Console.WriteLine("Loading environment...");
        var root = @"c:\Users\dennis\source\repos\stat_prog\";
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);

        var ret = Environment.GetEnvironmentVariable("HISTORY_FILENAME");
        historyFilename = (ret == null) ? "" : ret;
        string? key = Environment.GetEnvironmentVariable("KEY");
        string? secret = Environment.GetEnvironmentVariable("SECRET");
        string? bucket = Environment.GetEnvironmentVariable("BUCKET");

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(bucket) || string.IsNullOrEmpty(historyFilename))
        {
            Console.Error.WriteLine($"One or more keys missing in {dotenv}!");
            return -1;
        }

        // utility to save history
        async Task<bool> SaveHistory()
        {
            var histPath = $".\\{historyFilename}";
            //
            // save history to disk
            //
            Console.WriteLine($"Saving history...");
            history.Save(histPath);

            //
            // save history to bucket
            //
            Console.WriteLine($"Writing history to S3...");
            var ret = await awsS3.PutFileAsync(historyFilename, histPath);
            return false;
        }



        //
        // Open S3 bucket
        //
        Console.WriteLine("Opening S3 Bucket...");
        ListObjectsV2Response? s3Response = await awsS3.InitializeAsync(key, secret, bucket, Amazon.RegionEndpoint.USEast2);

        if (s3Response == null)
        {
            Console.Error.WriteLine($"Failed to initialize AWS S3 bucket: {bucket}");
            return -1;
        }

        //
        // Load history
        //
        Console.WriteLine($"Loading session history from {historyFilename}...");
        var s3oHistory = s3Response.S3Objects.Find(obj => obj.Key == historyFilename);
        if (s3oHistory != null)
        {
            // history within s3 bucket, load it
            var outputPath = await awsS3.GetObjectAsync(s3oHistory.Key, historyPath);
            if (outputPath != null)
            {
                history.Load(outputPath);
                Console.WriteLine("  session history loaded.");
            }
            else
            {
                Console.WriteLine("  session history NOT LOADED, see errors.");
                Console.Error.WriteLine($"Error downloading history file {historyFilename}");
            }
        }
        else
        {
            Console.WriteLine("  session history not found, starting new.");
        }

        //
        // Sort objects by date
        //
        var compare = new S3ObjectDTCompare();
        s3Response.S3Objects.Sort(compare);

        //
        // loop through archives, acting upon unprocessed.  store in history
        //
        const string workDir = @"work";
        Console.WriteLine($"Processing Bucket:");
        foreach (var s3o in s3Response.S3Objects)
        {
            var processThisFile = false;
            var ext = Path.GetExtension(s3o.Key).ToLower();
            var archiveName = Path.GetFileName(s3o.Key);

            if (ext == ".zip")
            {
                Console.Write($"Archive: {s3o.Key}...");
                processThisFile = true;

                var item = history.GetItemByArchive(archiveName);
                if (item != null)
                {
                    // archive has been processed before, reprocess only if zip is newer
                    if (item.dateTime >= s3o.LastModified)
                    {
                        processThisFile = false;
                        Console.WriteLine("Already processed, skipping.");
                    }
                }
            }

            if (processThisFile)
            {
                Console.Write($"Downloading...");

                // download archive from s3
                var path = await awsS3.GetObjectAsync(s3o.Key, workDir);

                // open archive
                Console.WriteLine($"Processing {path}...");
                POArchive archive = new POArchive(path);
                await archive.Process(history, awsS3);
                Console.WriteLine($"Done with archive.");

                await SaveHistory();
            }
        }

        return 0;
    }
}


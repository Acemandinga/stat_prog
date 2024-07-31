using Amazon.S3.Model;
using Helpers;


//
// Main
//
internal class Program
{
    const string HISTORY_FILENAME = "_history.json";

    private static async Task<int> Main(string[] args)
    {
        //
        // load .env file for keys
        //
        var root = @"c:\Users\dennis\source\repos\stat_prog\";
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);

        string? key = Environment.GetEnvironmentVariable("KEY");
        string? secret = Environment.GetEnvironmentVariable("SECRET"); 
        string? bucket = Environment.GetEnvironmentVariable("BUCKET");

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(bucket))
        {
            Console.Error.WriteLine($"One or more keys missing in {dotenv}!");
            return -1;
        }

        //
        // Open S3 bucket
        //
        AWS_S3 awsS3 = new AWS_S3();
        ListObjectsV2Response? s3Response = await awsS3.InitializeAsync(key, secret, bucket, Amazon.RegionEndpoint.USEast2);

        if (s3Response == null)
        {
            Console.Error.WriteLine($"Failed to initialize AWS S3 bucket: {bucket}");
            return -1;
        }

        //
        // Load history
        //
        const string historyPath = @"work";
        History history = new();
        var s3oHistory = s3Response.S3Objects.Find(obj => obj.Key == HISTORY_FILENAME);
        if (s3oHistory != null)
        {
            // history within s3 bucket, load it
            var outputPath = await awsS3.GetObjectAsync(s3oHistory.Key, historyPath);
            if (outputPath != null)
            {
                history.Load(outputPath);
            }
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
        foreach (var s3o in s3Response.S3Objects)
        {
            // download archive from s3
            var path = await awsS3.GetObjectAsync(s3o.Key, workDir);

            // open archive
            POArchive archive = new POArchive(path);

            // process any work to do in the file
            archive.Process(history);
        }

        //
        // save history to disk
        //
        history.Save(HISTORY_FILENAME);

        //
        // save history to bucket
        //
        var ret = await awsS3.PutFileAsync(HISTORY_FILENAME, historyPath);


        //POArchive file = new POArchive("../../../../test.zip");
        //List<string> fns = file.GetFilenames();
        //foreach (string fn in fns)
        //{
        //    Console.WriteLine(fn);
        //}


        return 0;
    }
}


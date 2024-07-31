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
        var root = @"c:\Users\dennis\source\repos\ConsoleApp1\";
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
        ListObjectsV2Response? objects = await awsS3.InitializeAsync(key, secret, bucket, Amazon.RegionEndpoint.USEast2);

        if (objects == null)
        {
            Console.Error.WriteLine($"Failed to initialize AWS S3 bucket: {bucket}");
            return -1;
        }

        //
        // Load history
        //
        const string tmpFile = @".\work.tmp";
        History history = new();
        var s3oHistory = objects.S3Objects.Find(obj => obj.Key == HISTORY_FILENAME);
        if (s3oHistory != null)
        {
            // history within s3 bucket, load it
            if (await awsS3.GetObjectAsync(s3oHistory.Key, tmpFile))
            {
                history.Load(tmpFile);

            }
        }

        //
        // Sort objects by date
        //


        //
        // loop through archives, acting upon unprocessed.  store in history
        //

        //
        // save history to disk
        //
        history.Save(HISTORY_FILENAME);

        //
        // save history to bucket
        //
        var ret = await awsS3.PutFileAsync(HISTORY_FILENAME, tmpFile);


        //POArchive file = new POArchive("../../../../test.zip");
        //List<string> fns = file.GetFilenames();
        //foreach (string fn in fns)
        //{
        //    Console.WriteLine(fn);
        //}


        return 0;
    }
}


using Amazon.S3.Model;
using System;
using System.IO;

namespace Helpers;

public static class DotEnv
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}

public class S3ObjectDTCompare : IComparer<S3Object>
{
    public int Compare(S3Object? a, S3Object? b)
    {
        if (a == null && b == null) return 0;
        if (a != null && b == null) return -1;
        if (a == null && b != null) return 1;
        return a.LastModified.CompareTo(b.LastModified);
    }
}

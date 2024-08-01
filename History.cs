using System.Text.Json;

//
// History (Storage)
//
internal class HistoryItem
{
    public required string archiveFilename;

    // pdf files extracted from the archive
    public required string[] pdfFiles;

    // dt processed
    public required DateTime dateTime;

    // "success" if archive was processed without errors
    public required string status;
}
internal class HistoryStore
{
    public required HistoryItem[] items;
}

internal class History
{
    public HistoryStore store;
    private JsonSerializerOptions serializeOptions;

    public History()
    {
        store = new HistoryStore() { items = [] };
        serializeOptions = new JsonSerializerOptions() { IncludeFields = true };
    }

    public void Load(string fn)
    {
        string strHistory = File.ReadAllText(fn);
        HistoryStore? _store = null;
        if (!string.IsNullOrEmpty(strHistory))
        {
            try
            {

                _store = JsonSerializer.Deserialize<HistoryStore>(strHistory, serializeOptions);
                if (_store != null)
                {
                    store = _store;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing history file - " + ex.Message);
                Console.Error.WriteLine("Error parsing history file: " + ex);
            }
        }
    }

    public void Save(string fn)
    {
        string strHistory = JsonSerializer.Serialize<HistoryStore>(store, serializeOptions);
        File.WriteAllText(fn, strHistory);
    }

    public HistoryItem? GetItemByArchive(string archiveName)
    {
        foreach (var item in store.items)
        {
            if (item.archiveFilename == archiveName)
            {
                return item;
            }
        }
        return null;
    }

    public void AddPDFFile(string archiveName, string pdfName)
    {
        var isNewArchive = true;

        foreach (var item in store.items)
        {
            if (item.archiveFilename == archiveName)
            {
                isNewArchive = false;
                var isProcessed = item.pdfFiles.Contains(pdfName);

                // this archive has been processed, but not this pdf file
                if (!isProcessed)
                {
                    Array.Resize<string>(ref item.pdfFiles, item.pdfFiles.Length + 1);
                    item.pdfFiles[item.pdfFiles.Length - 1] = pdfName;
                    item.status = "success";
                }
            }
        }

        // this archive has not yet been processed at all
        if (isNewArchive)
        {
            Array.Resize<HistoryItem>(ref store.items, store.items.Length + 1);
            store.items[store.items.Length - 1] = new HistoryItem()
            {
                archiveFilename = archiveName,
                pdfFiles = [pdfName],
                dateTime = DateTime.Now,
                status = "success"
            };
        }
    }
}


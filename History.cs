using System.Text.Json;

//
// History (Storage)
//
internal class HistoryItem
{
    public required string archiveFilename;

    // pdf files extracted from the archive
    public required string[] pdfFiles;

    // when archive was processed
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

    public History() {
        store = new HistoryStore() { items = [] };
    }

    public void Load(string fn)
    {
        string strHistory = File.ReadAllText(fn);
        HistoryStore? _store = null;
        if (!string.IsNullOrEmpty(strHistory))
        {
            _store = JsonSerializer.Deserialize<HistoryStore>(strHistory);
            if (_store != null)
            {
                store = _store;
            }
        }
    }

    public void Save(string fn)
    {
        string strHistory = JsonSerializer.Serialize<HistoryStore>(store);
        File.WriteAllText(fn, strHistory);
    }
}


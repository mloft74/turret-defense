using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text.Json;
using TurretDefense.Models;

namespace TurretDefense;

public static class Persistence
{
    private const string HIGH_SCORES = "highScores.json";
    private const string KEY_MAP = "keyMap.json";

    public static List<int> LoadScores()
    {
        using var storage = IsolatedStorageFile.GetUserStoreForApplication();
        if (!storage.FileExists(HIGH_SCORES)) return new();

        using var fs = storage.OpenFile(HIGH_SCORES, FileMode.Open);
        using var streamReader = new StreamReader(fs);

        var contents = streamReader.ReadToEnd();
        return JsonSerializer.Deserialize<List<int>>(contents) ?? new();
    }

    public static Dictionary<string, KeyInfo> LoadKeyMap()
    {
        using var storage = IsolatedStorageFile.GetUserStoreForApplication();
        if (!storage.FileExists(KEY_MAP)) return new();

        using var fs = storage.OpenFile(KEY_MAP, FileMode.Open);
        using var streamReader = new StreamReader(fs);

        var contents = streamReader.ReadToEnd();
        var loaded = JsonSerializer.Deserialize<Dictionary<string, KeyInfoDto>>(contents) ?? new();
        var transformed = loaded.Select(pair => KeyValuePair.Create(pair.Key, pair.Value.ToKeyInfo()));
        return new(transformed);
    }

    public static void SaveScores(List<int> scores)
    {
        using var storage = IsolatedStorageFile.GetUserStoreForApplication();
        using var fs = storage.CreateFile(HIGH_SCORES);
        using var writeStream = new StreamWriter(fs);
        var contents = JsonSerializer.Serialize(scores);
        writeStream.Write(contents);
    }

    public static void SaveKeyMap(Dictionary<string, KeyInfo> keyMap)
    {
        var transformed = keyMap.Select(pair => KeyValuePair.Create(pair.Key, pair.Value.ToDto()));
        var toSave = new Dictionary<string, KeyInfoDto>(transformed);
        using var storage = IsolatedStorageFile.GetUserStoreForApplication();
        using var fs = storage.CreateFile(KEY_MAP);
        using var writeStream = new StreamWriter(fs);
        var contents = JsonSerializer.Serialize(toSave);
        writeStream.Write(contents);
    }
}

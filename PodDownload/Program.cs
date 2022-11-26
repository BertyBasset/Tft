// See https://aka.ms/new-console-template for more information

using PodDownload;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System;


if (args.Length < 2) {
    Console.WriteLine("Usage: PodDownload.exe <json file list path> <download folder>\r\n   Absolute or relative patch can be used for both arguments\r\n   Use ./ for current path, ../ to go up one level");
    return;
}



string exePath = (new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)).Directory.FullName;


//string jsonFileListPath = Path.GetFullPath(args[0], exePath);

string jsonDownloadListFileSpec = Path.GetFullPath(Path.Combine(exePath, args[0]));
string downloadFolder = Path.GetFullPath(args[1], exePath);

if (!downloadFolder.EndsWith("/") && !downloadFolder.EndsWith("\\"))
    downloadFolder += "\\";



// Check these two exist
if (!System.IO.File.Exists(jsonDownloadListFileSpec)) { 
    Console.WriteLine($"Json Download List File '{jsonDownloadListFileSpec}' does not exist.");
    return;
}
if (!System.IO.Directory.Exists(downloadFolder)) {
    Console.WriteLine($"Download folder '{downloadFolder}' does not exist.");
    return;
}


List<Pod> podCasts = null;
try {
    podCasts = JsonSerializer.Deserialize<List<Pod>>(System.IO.File.ReadAllText(jsonDownloadListFileSpec));
} catch (Exception) {
    Console.WriteLine($"Json Download List File '{jsonDownloadListFileSpec}' does not appear to contain valid Json.");
    return;
}



int count = 0;
int success = 0;
int skipped = 0;

var httpClient = new HttpClient();


foreach (var podCast in podCasts) {
    try {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{podCast.url}...");

        if (System.IO.File.Exists(downloadFolder + "\\" + podCast.fileName)) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{podCast.fileName} exists, Skipping.. ");
            Console.ForegroundColor = ConsoleColor.White;
            skipped++;
            continue;
        }



        var response = await httpClient.GetAsync(podCast.url);
        using (var fs = new FileStream(downloadFolder + podCast.fileName, FileMode.Create)) {
            await response.Content.CopyToAsync(fs);
        }


        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($" OK");
        success++;


    } catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Failed {ex.Message}");
        Console.ForegroundColor = ConsoleColor.White;
        System.IO.File.AppendAllText("Error.log", podCast.fileName + "\r\n");
    }

    count++;
    Console.ForegroundColor = ConsoleColor.White;
}

Console.WriteLine($"");
Console.Write($"Success: ");
Console.ForegroundColor = ConsoleColor.Green;
Console.Write($"{success}");
Console.ForegroundColor = ConsoleColor.White;
Console.Write($"     Skipped: ");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write($"{skipped}");
Console.ForegroundColor = ConsoleColor.White;
Console.ForegroundColor = ConsoleColor.White;
Console.Write($"     Failed: ");
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"{podCasts.Count - (success + skipped)}");
Console.ForegroundColor = ConsoleColor.White;











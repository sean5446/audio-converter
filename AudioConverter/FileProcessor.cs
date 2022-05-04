
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AudioConverter;


public class FileProcessor
{
    private string vlcPath;

    public FileProcessor(string vlcPath)
    {
        this.vlcPath = vlcPath;
    }

    private class TaskParams
    {
        public string Cmd { get; set; }
        public string Args { get; set; }
        public override string ToString()
        {
            return Cmd + " " + Args;
        }
    }

    public string[] ProcessFiles(string sourcePath, string sourceExt, string destPath, string destExt)
    {
        string[] files = Directory.GetFiles(sourcePath);
        int bitrate = 192;
        var tasks = new List<Task>();

        Directory.CreateDirectory(destPath);

        foreach (string file in files)
        {
            if (file.Contains("." + sourceExt))
            {
                string destFile = GetDestFileName(file, destPath, destExt);

                TaskParams taskParams = new()
                {
                    Cmd = vlcPath,
                    Args = $" --sout=#transcode{{acodec={destExt},ab={bitrate},channels=2,samplerate=44100}}:file{{dst=\"{destFile}\"}} \"{file}\" vlc://quit",
                };
                // https://wiki.videolan.org/transcode/

                tasks.Add(Task<string>.Factory.StartNew(convertFile, taskParams));
                Console.WriteLine(taskParams.ToString());
            }
            else
            {
                string dest = GetDestFileName(file, destPath, Path.GetExtension(file).Replace(".", ""));
                if (!File.Exists(dest))
                {
                    File.Copy(file, dest);
                }
            }
        }

        Task.WaitAll(tasks.ToArray());

        foreach (string file in files)
        {
            if (file.Contains("." + sourceExt))
            {
                CopyTags(file, destPath, destExt);
            }
        }

        return files;
    }

    private readonly Func<object, string> convertFile = (object obj) =>
    {
        TaskParams tp = (TaskParams)obj;
        RunCommand(tp.Cmd, tp.Args);
        return tp.ToString();
    };

    private static void CopyTags(string filePath, string destPath, string destExt)
    {
        // get tags
        TagLib.File sourceFile = TagLib.File.Create(filePath);
        string title = sourceFile.Tag.Title;
        string album = sourceFile.Tag.Album;
        string artist = sourceFile.Tag.FirstArtist;
        uint track = sourceFile.Tag.Track;
        uint year = sourceFile.Tag.Year;

        // apply tags
        string destFileName = GetDestFileName(filePath, destPath, destExt);
        TagLib.File destFile = TagLib.File.Create(destFileName);
        destFile.Tag.Title = title;
        destFile.Tag.Album = album;
        destFile.Tag.Artists = new string[] { artist };
        destFile.Tag.Track = track;
        destFile.Tag.Year = year;
        destFile.Save();
    }

    private static void RunCommand(string cmd, string args)
    {
        Process process = new();
        ProcessStartInfo startInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            //startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = " /S /C \"" + command + "\"";
            FileName = cmd,
            Arguments = args,
        };
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    public static string GetDestFileName(string filePath, string destPath, string extension)
    {
        string fileName = destPath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filePath) + "." + extension;
        // these characters seem to cause problems with vlc command
        return fileName.Replace(",", "").Replace("'", "").Replace("\"", "");
    }

}

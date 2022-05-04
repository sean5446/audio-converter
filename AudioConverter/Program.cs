
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AudioConverter
{
    class Program
    {
        public static void Main(string[] args)
        {
            // start stopwatch
            Stopwatch stopWatch = new();
            stopWatch.Start();

            // get params from console
            string vlcPath = args[1];
            // string vlcPath = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            string sourceDir = args[2];
            //string sourceDir = @"C:\Users\queso\Downloads\Andrew Bird - Discography [FLAC]\";

            // open parent directory with album folders
            DirectoryInfo parentDir = new(sourceDir);
            List<string> processedFiles = new();

            // do the work
            FileProcessor fileProcessor = new(vlcPath);
            foreach (DirectoryInfo sourceFolder in parentDir.GetDirectories())
            {
                string destFolder = sourceFolder + " [mp3]";
                string[] files = fileProcessor.ProcessFiles(sourceFolder.FullName, "flac", destFolder, "mp3");
                processedFiles.AddRange(files);
            }

            // print timings
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            Console.WriteLine(String.Format("\n\nProcessed {0} files in {1}\n\n", processedFiles.Count, elapsedTime));
        }
    }
}
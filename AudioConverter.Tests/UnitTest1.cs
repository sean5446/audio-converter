using Xunit;
using System.IO;

namespace AudioConverter.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        string actual = FileProcessor.GetDestFileName("path/to/fi,'le.flac", "new/dest", "mp3");
        Assert.Equal($"new/dest{Path.DirectorySeparatorChar}file.mp3", actual);
    }
}
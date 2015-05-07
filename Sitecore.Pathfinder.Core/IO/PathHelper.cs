﻿namespace Sitecore.Pathfinder.IO
{
  using System;
  using System.IO;
  using System.Text.RegularExpressions;
  using Sitecore.Pathfinder.Diagnostics;
  using Sitecore.Pathfinder.Extensions.StringExtensions;

  public static class PathHelper
  {
    [NotNull]
    public static string Combine([NotNull] string path1, [NotNull] string path2)
    {
      var f1 = NormalizeFilePath(path1);
      var f2 = NormalizeFilePath(path2);

      var path = Path.Combine(f1, f2);

      path = Path.GetFullPath(path);

      if (f1.IndexOf(Path.VolumeSeparatorChar) < 0 && f2.IndexOf(Path.VolumeSeparatorChar) < 0)
      {
        var n = path.IndexOf(Path.VolumeSeparatorChar);
        if (n >= 0)
        {
          path = path.Mid(n + 1);
        }
      }

      return path;
    }

    public static bool CompareFiles([NotNull] string sourceFileName, [NotNull] string destinationFileName)
    {
      var fileInfo1 = new FileInfo(sourceFileName);
      var fileInfo2 = new FileInfo(destinationFileName);

      if (!fileInfo1.Exists && !fileInfo2.Exists)
      {
        return true;
      }

      if (!fileInfo2.Exists)
      {
        return false;
      }

      if (!fileInfo1.Exists)
      {
        return false;
      }

      // var lastWrite1 = new DateTime(fileInfo1.LastWriteTimeUtc.Year, fileInfo1.LastWriteTimeUtc.Month, fileInfo1.LastWriteTimeUtc.Day, fileInfo1.LastWriteTimeUtc.Hour, fileInfo1.LastWriteTimeUtc.Minute, fileInfo1.LastWriteTimeUtc.Second);
      // var lastWrite2 = new DateTime(fileInfo2.LastWriteTimeUtc.Year, fileInfo2.LastWriteTimeUtc.Month, fileInfo2.LastWriteTimeUtc.Day, fileInfo2.LastWriteTimeUtc.Hour, fileInfo2.LastWriteTimeUtc.Minute, fileInfo2.LastWriteTimeUtc.Second);
      if (fileInfo1.LastWriteTimeUtc != fileInfo2.LastWriteTimeUtc)
      {
        return false;
      }

      if (fileInfo1.Length != fileInfo2.Length)
      {
        return false;
      }

      return true;
    }

    [NotNull]
    public static string GetDirectoryAndFileNameWithoutExtensions([NotNull] string fileName)
    {
      var n = NormalizeFilePath(fileName).LastIndexOf('\\');
      if (n < 0)
      {
        n = 0;
      }

      n = fileName.IndexOf('.', n);
      if (n < 0)
      {
        return fileName;
      }

      return fileName.Left(n);
    }

    [NotNull]
    public static string GetFileNameWithoutExtensions([NotNull] string fileName)
    {
      var s = NormalizeFilePath(fileName).LastIndexOf('\\');
      var e = fileName.IndexOf('.', s);
      return fileName.Mid(s + 1, e - s - 1);
    }

    public static bool MatchesPattern([NotNull] string fileName, [NotNull] string pattern)
    {
      var s = Path.GetFileName(fileName) ?? string.Empty;

      var regex = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

      return Regex.IsMatch(s, regex, RegexOptions.IgnoreCase);
    }

    [NotNull]
    public static string NormalizeFilePath([NotNull] string filePath)
    {
      return filePath.Replace("/", "\\");
    }

    [NotNull]
    public static string NormalizeWebPath([NotNull] string filePath)
    {
      return filePath.Replace("\\", "/");
    }

    [NotNull]
    public static string UnmapPath([NotNull] string rootPath, [NotNull] string fileName)
    {
      rootPath = NormalizeFilePath(rootPath).TrimEnd('\\');
      fileName = NormalizeFilePath(fileName);

      if (fileName.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
      {
        return fileName.Mid(rootPath.Length);
      }

      return fileName;
    }
  }
}
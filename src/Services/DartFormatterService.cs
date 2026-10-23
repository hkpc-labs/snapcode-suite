using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Snapcode.Services
{
    /// <summary>
    /// Service for automatically formatting Dart code to idiomatic Flutter standards,
    /// eliminating lengthy lines, optimizing widget tree indentation, and running dart format.
    /// </summary>
    public static class DartFormatterService
    {
        private static string? _cachedDartExe;
        private static bool _hasSearchedDartExe = false;
        private static readonly object _lock = new();

        /// <summary>
        /// Locates the fastest available Dart executable (prefers native dart.exe inside dart-sdk cache).
        /// </summary>
        public static string? GetDartExecutablePath()
        {
            lock (_lock)
            {
                if (_hasSearchedDartExe && !string.IsNullOrEmpty(_cachedDartExe) && File.Exists(_cachedDartExe))
                {
                    return _cachedDartExe;
                }

                _hasSearchedDartExe = true;
                _cachedDartExe = FindDartExecutableInternal();
                return _cachedDartExe;
            }
        }

        private static string? FindDartExecutableInternal()
        {
            var candidatePaths = new List<string>();

            // 1. From Config file
            try
            {
                string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapCode", "config.json");
                if (File.Exists(configFilePath))
                {
                    string json = File.ReadAllText(configFilePath);
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("flutterSdkPath", out var p) && !string.IsNullOrWhiteSpace(p.GetString()))
                    {
                        string configuredSdk = p.GetString()!;
                        if (Directory.Exists(configuredSdk))
                        {
                            candidatePaths.Add(Path.Combine(configuredSdk, "bin", "cache", "dart-sdk", "bin", "dart.exe"));
                            candidatePaths.Add(Path.Combine(configuredSdk, "bin", "dart.bat"));
                            candidatePaths.Add(Path.Combine(configuredSdk, "bin", "dart.exe"));
                            candidatePaths.Add(Path.Combine(configuredSdk, "bin", "flutter.bat"));
                        }
                    }
                }
            }
            catch { }

            // 2. Common Flutter installation locations on Windows
            string[] commonRoots = new[]
            {
                @"C:\src\flutter",
                @"C:\flutter",
                @"D:\src\flutter",
                @"D:\flutter",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "flutter"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "flutter")
            };

            foreach (var root in commonRoots)
            {
                if (Directory.Exists(root))
                {
                    candidatePaths.Add(Path.Combine(root, "bin", "cache", "dart-sdk", "bin", "dart.exe"));
                    candidatePaths.Add(Path.Combine(root, "bin", "dart.bat"));
                }
            }

            foreach (var path in candidatePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // 3. Check PATH for dart.exe or dart.bat
            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                string[] dirs = pathEnv.Split(Path.PathSeparator);
                foreach (var dir in dirs)
                {
                    if (string.IsNullOrWhiteSpace(dir)) continue;
                    try
                    {
                        string dartExe = Path.Combine(dir.Trim(), "dart.exe");
                        if (File.Exists(dartExe)) return dartExe;

                        string dartBat = Path.Combine(dir.Trim(), "dart.bat");
                        if (File.Exists(dartBat)) return dartBat;
                    }
                    catch { }
                }
            }

            return null;
        }

        /// <summary>
        /// Formats a Dart file on disk in place using dart format or pure C# indenter fallback.
        /// </summary>
        public static async Task<bool> FormatFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            string? dartExe = GetDartExecutablePath();
            if (!string.IsNullOrEmpty(dartExe) && File.Exists(dartExe))
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = dartExe,
                        Arguments = $"format \"{filePath}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc != null)
                    {
                        using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await proc.WaitForExitAsync(cts.Token).ConfigureAwait(false);
                        if (proc.ExitCode == 0)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DartFormatter] CLI format failed: {ex.Message}");
                }
            }

            // Fallback: Format using pure C# in-memory formatter
            try
            {
                string source = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                string formatted = FormatDartCodeInMemory(source);
                if (formatted != source)
                {
                    await File.WriteAllTextAsync(filePath, formatted).ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DartFormatter] In-memory format fallback failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Synchronous wrapper for FormatFileAsync.
        /// </summary>
        public static bool FormatFile(string filePath)
        {
            try
            {
                return FormatFileAsync(filePath).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats Dart code string in memory, leveraging dart format CLI if available or pure C# fallback.
        /// </summary>
        public static string FormatCode(string dartCode)
        {
            if (string.IsNullOrWhiteSpace(dartCode)) return dartCode;

            string? dartExe = GetDartExecutablePath();
            if (!string.IsNullOrEmpty(dartExe) && File.Exists(dartExe))
            {
                try
                {
                    string tempFile = Path.Combine(Path.GetTempPath(), $"snapcode_fmt_{Guid.NewGuid():N}.dart");
                    File.WriteAllText(tempFile, dartCode);
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = dartExe,
                            Arguments = $"format \"{tempFile}\"",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        using var proc = Process.Start(psi);
                        if (proc != null)
                        {
                            bool exited = proc.WaitForExit(3000);
                            if (exited && proc.ExitCode == 0 && File.Exists(tempFile))
                            {
                                return File.ReadAllText(tempFile);
                            }
                        }
                    }
                    finally
                    {
                        if (File.Exists(tempFile))
                        {
                            try { File.Delete(tempFile); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DartFormatter] Code format CLI failed: {ex.Message}");
                }
            }

            return FormatDartCodeInMemory(dartCode);
        }

        /// <summary>
        /// Pure C# formatter and indenter for Dart code. Breaks long widget lines and enforces 2-space Flutter indentation.
        /// </summary>
        public static string FormatDartCodeInMemory(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;

            // Step 1: Pre-process long lines containing nested widgets to insert line breaks after commas and parens
            var expanded = new StringBuilder();
            using var reader = new StringReader(source);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 85 && (trimmed.Contains("Container(") || trimmed.Contains("Column(") || trimmed.Contains("Row(") || trimmed.Contains("children: [") || trimmed.Contains("Text(") || trimmed.Contains("ElevatedButton(")))
                {
                    // Break this line carefully without splitting inside quotes
                    expanded.AppendLine(SplitLongWidgetLine(line));
                }
                else
                {
                    expanded.AppendLine(line);
                }
            }

            // Step 2: Indent token-by-token based on braces, brackets, and parentheses
            var result = new StringBuilder();
            using var expandedReader = new StringReader(expanded.ToString());
            int indentLevel = 0;
            const string indentStep = "  ";

            while ((line = expandedReader.ReadLine()) != null)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    result.AppendLine();
                    continue;
                }

                // Preserve comment markers exactly
                if (trimmed.StartsWith("// <!-- USER_CODE_"))
                {
                    result.Append(new string(' ', Math.Max(0, indentLevel) * 2));
                    result.AppendLine(trimmed);
                    continue;
                }

                // If line starts with closing tokens, reduce indent before writing
                int leadingClosingCount = 0;
                for (int i = 0; i < trimmed.Length; i++)
                {
                    char c = trimmed[i];
                    if (c == '}' || c == ')' || c == ']')
                    {
                        leadingClosingCount++;
                    }
                    else if (!char.IsWhiteSpace(c) && c != ';' && c != ',')
                    {
                        break;
                    }
                }

                int effectiveIndent = Math.Max(0, indentLevel - leadingClosingCount);
                result.Append(new string(' ', effectiveIndent * 2));
                result.AppendLine(trimmed);

                // Update indent level for subsequent lines
                bool inSingleQuote = false;
                bool inDoubleQuote = false;
                for (int i = 0; i < trimmed.Length; i++)
                {
                    char c = trimmed[i];
                    char prev = i > 0 ? trimmed[i - 1] : '\0';

                    if (c == '\'' && prev != '\\' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
                    else if (c == '\"' && prev != '\\' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
                    else if (!inSingleQuote && !inDoubleQuote)
                    {
                        if (c == '{' || c == '(' || c == '[')
                        {
                            indentLevel++;
                        }
                        else if (c == '}' || c == ')' || c == ']')
                        {
                            indentLevel = Math.Max(0, indentLevel - 1);
                        }
                    }
                }
            }

            return result.ToString().TrimEnd() + Environment.NewLine;
        }

        private static string SplitLongWidgetLine(string line)
        {
            var sb = new StringBuilder();
            bool inSingleQuote = false;
            bool inDoubleQuote = false;
            int parenDepth = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                char prev = i > 0 ? line[i - 1] : '\0';

                if (c == '\'' && prev != '\\' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
                else if (c == '\"' && prev != '\\' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;

                sb.Append(c);

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (c == '(' || c == '[')
                    {
                        parenDepth++;
                    }
                    else if (c == ')' || c == ']')
                    {
                        parenDepth = Math.Max(0, parenDepth - 1);
                    }
                    else if (c == ',' && parenDepth > 0)
                    {
                        // Check if next char is not already newline
                        if (i + 1 < line.Length && line[i + 1] != '\n' && line[i + 1] != '\r')
                        {
                            sb.AppendLine();
                            if (i + 1 < line.Length && line[i + 1] == ' ') i++; // skip following space
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}

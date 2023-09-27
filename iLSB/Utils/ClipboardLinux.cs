using System.Diagnostics;
using System.Text;

namespace iLSB.Utils;

public class ClipboardLinux : IClipboard
{
    private bool isWsl;

    public ClipboardLinux()
    {
        isWsl = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME") != null;
    }

    public void SetText(string text)
    {
        var tempFileName = Path.GetTempFileName();
        File.WriteAllText(tempFileName, text);
        InnerSetText(tempFileName);
    }

    private void InnerSetText(string tempFileName)
    {
        try
        {
            if (isWsl)
            {
                BashRunner.Run($"cat {tempFileName} | clip.exe ");
            }
            else
            {
                BashRunner.Run($"cat {tempFileName} | xsel -i --clipboard ");
            }
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }

    public string? GetText()
    {
        var tempFileName = Path.GetTempFileName();
        try
        {
            InnerGetText(tempFileName);
            return File.ReadAllText(tempFileName);
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }

    private void InnerGetText(string tempFileName)
    {
        if (isWsl)
        {
            BashRunner.Run($"powershell.exe -NoProfile Get-Clipboard  > {tempFileName}");
        }
        else
        {
            BashRunner.Run($"xsel -o --clipboard  > {tempFileName}");
        }
    }
}

static class BashRunner
{
    public static string Run(string commandLine)
    {
        var errorBuilder = new StringBuilder();
        var outputBuilder = new StringBuilder();
        var arguments = $"-c \"{commandLine}\"";
        using (var process = new Process
               {
                   StartInfo = new ProcessStartInfo
                   {
                       FileName = "bash",
                       Arguments = arguments,
                       RedirectStandardOutput = true,
                       RedirectStandardError = true,
                       UseShellExecute = false,
                       CreateNoWindow = false,
                   }
               })
        {
            process.Start();
            process.OutputDataReceived += (sender, args) => { outputBuilder.AppendLine(args.Data); };
            process.BeginOutputReadLine();
            process.ErrorDataReceived += (sender, args) => { errorBuilder.AppendLine(args.Data); };
            process.BeginErrorReadLine();
            if (!process.WaitForExit(500))
            {
                var timeoutError = $@"Process timed out. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
                throw new Exception(timeoutError);
            }

            if (process.ExitCode == 0)
            {
                return outputBuilder.ToString();
            }

            var error = $@"Could not execute process. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
            throw new Exception(error);
        }
    }
}

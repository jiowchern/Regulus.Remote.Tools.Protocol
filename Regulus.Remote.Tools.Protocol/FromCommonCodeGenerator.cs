using Microsoft.Build.Framework;

namespace Regulus.Remote.Tools.Protocol
{
    public class FromCommonCodeGenerator : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem RegulusProtocol { get; set;  }

        void _LogError(string msg)
        {
            Log.LogError($"[regulus-remote-protocol]{msg}");
        }
        void _LogMessage(string msg)
        {
            Log.LogMessage(MessageImportance.High, $"[regulus-remote-protocol]{msg}");
        }

        public override bool Execute()
        {
            _LogMessage("Start.");
            var outDir = System.IO.Path.GetFullPath(RegulusProtocol.ItemSpec);
            var sourceFile = System.IO.Path.GetFullPath(RegulusProtocol.GetMetadata("InputFile"));
            var toolDir = System.IO.Path.GetFullPath(RegulusProtocol.GetMetadata("ToolDir"));
            _LogMessage("OutputDir is " + outDir);
            _LogMessage("SourceFile is " + sourceFile);

            if (!System.IO.Directory.Exists(outDir))
            {
                _LogError($"OutputDir does not exist.");
                return false;
            }

            if (!System.IO.Directory.Exists(toolDir))
            {
                _LogMessage("ToolDir is " + toolDir);
                _LogError($"ToolDir does not exist.");
                return false;
            }
            if (!System.IO.File.Exists(sourceFile))
            {
                Log.LogError($"SourceFile does not exist.");
                return false;
            }

             var removeFiles = System.IO.Directory.GetFiles(outDir, "*.cs");
             foreach (var removeFile in removeFiles)
             {                
                 System.IO.File.Delete(removeFile);
                 _LogMessage($"remove file {removeFile}.");
             }
            _LogMessage("Generating code.");

            var toolFile = System.IO.Path.Combine(toolDir, "Regulus.Application.Protocol.CodeWriter.dll");
            _LogMessage($"dotnet {toolFile} --common {sourceFile} --output {outDir}");

            var process = new System.Diagnostics.Process();// System.Diagnostics.Process.Start("dotnet", $"run -p {toolFile} -- --common={sourceFile} --output={outDir}");
            var info = new System.Diagnostics.ProcessStartInfo("dotnet", $"{toolFile} --common {sourceFile} --output {outDir}");
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo = info;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return false;
            }
            _LogMessage("Done.");
            return true;
        }
    }
}

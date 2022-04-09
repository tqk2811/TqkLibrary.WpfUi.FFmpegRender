using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace TqkLibrary.WpfUi.FFmpegRender
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderService
    {
        static readonly FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
        readonly string _RenderPath = Path.Combine(Directory.GetCurrentDirectory(), fileInfo.Name);
        /// <summary>
        /// 
        /// </summary>
        public event DataReceivedEventHandler OutputDataReceived;
        /// <summary>
        /// 
        /// </summary>
        public event DataReceivedEventHandler ErrorDataReceived;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderPath"></param>
        public RenderService(string renderPath = null)
        {
            if (!string.IsNullOrEmpty(renderPath)) _RenderPath = renderPath;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderData"></param>
        /// <returns></returns>
        public async Task<int> StartRun(RenderData renderData)
        {
#if NET462
            PipeSecurity _pipeSecurity = new PipeSecurity();
            PipeAccessRule psEveryone = new PipeAccessRule("Everyone", PipeAccessRights.FullControl,
                System.Security.AccessControl.AccessControlType.Allow);
            _pipeSecurity.AddAccessRule(psEveryone);
#endif
            string json = JsonConvert.SerializeObject(renderData);
            string PipeName = "Render_" + Guid.NewGuid().ToString();
            using NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(
                  PipeName,
                  PipeDirection.InOut,
                  1,
                  PipeTransmissionMode.Byte,
                  PipeOptions.None,
                  1024 * 1024,
                  1024 * 1024
                  #if NET462
                  ,_pipeSecurity
                  #endif
                  );

            ProcessStartInfo processStartInfo = new ProcessStartInfo(_RenderPath);
            processStartInfo.Arguments = PipeName;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            processStartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            using Process process = Process.Start(processStartInfo);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(10000);
            try
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                process.EnableRaisingEvents = true;
                process.Exited += (sender, args) => tcs.TrySetResult(null);
                
                await namedPipeServerStream.WaitForConnectionAsync(cancellationTokenSource.Token);
                using StreamWriter sw = new StreamWriter(namedPipeServerStream);
                await sw.WriteLineAsync(json);
                namedPipeServerStream.WaitForPipeDrain();
                
                await tcs.Task.ConfigureAwait(false);
            }
            catch
            {

            }
            finally
            {
                try { if (!process.HasExited) process.Kill(); } catch { }
            }

            return process.ExitCode;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            ErrorDataReceived?.Invoke(sender, e);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputDataReceived?.Invoke(sender, e);
        }
    }
}

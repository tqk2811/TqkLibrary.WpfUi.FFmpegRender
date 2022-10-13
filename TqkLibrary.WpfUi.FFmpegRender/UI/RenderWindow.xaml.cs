using FFmpegArgs.Executes;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TqkLibrary.WpfUi.FFmpegRender.UI.ViewModels;
using System.IO.Pipes;
namespace TqkLibrary.WpfUi.FFmpegRender.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RenderWindow : Window
    {
        readonly RenderWVM renderWVM;

        readonly string PipeName;

        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        RenderData renderData;
        bool AllowClose = false;
        int arg_index = 0;


        internal RenderWindow(string[] Args)
        {
            this.DataContext = this;
            if (Args == null || Args.Length != 1)
            {
                Application.Current.Shutdown();
            }
            else
            {
                PipeName = Args[0];
                InitializeComponent();
                this.renderWVM = this.DataContext as RenderWVM;
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            MessageBox.Show("Wait attach");
#endif
            try
            {
                using (NamedPipeClientStream NamedPipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut))
                {
                    await NamedPipeClientStream.ConnectAsync(5000);
                    if (NamedPipeClientStream.IsConnected)
                    {
                        NamedPipeClientStream.ReadMode = PipeTransmissionMode.Byte;
                        if (NamedPipeClientStream.IsConnected)
                        {
                            using (StreamReader sr = new StreamReader(NamedPipeClientStream))
                            {
                                string json = await sr.ReadToEndAsync();
                                this.renderData = JsonConvert.DeserializeObject<RenderData>(json);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Connection timeout", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        AllowClose = true;
                        this.Close();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, ex.GetType().FullName);
                AllowClose = true;
                Application.Current.Shutdown(-1);
                return;
            }
            if (renderData?.RenderItems == null && renderData.RenderItems.Count == 0)
            {
                AllowClose = true;
                this.Close();
                return;
            }
            ProcessArg();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckBeforeExit())
            {
                e.Cancel = true;
            }
        }

        private void BT_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBeforeExit())
            {
                Environment.ExitCode = -1;
            }
        }

        bool CheckBeforeExit()
        {
            if (AllowClose) return true;
            if (MessageBox.Show("Bạn có muốn hủy", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                cancellationTokenSource.Cancel();
                AllowClose = true;
                return true;
            }
            return false;
        }


        async void ProcessArg()
        {
            int exitCode = 0;
            try
            {
                for (arg_index = 0; arg_index < renderData.RenderItems.Count; arg_index++)
                {
                    renderWVM.StepInfo = $"Step: {arg_index + 1}/{renderData.RenderItems.Count}";
                    Console.Out.WriteLine($"{arg_index + 1}/{renderData.RenderItems.Count}|0");

                    StreamWriter sw = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(renderData.RenderItems[arg_index].LogPath))
                        {
                            sw = new StreamWriter(renderData.RenderItems[arg_index].LogPath, true);
                            sw.WriteLine("Arg: " + renderData.RenderItems[arg_index].Arguments);
                            sw.WriteLine();
                            sw.WriteLine();
                        }

                        TaskbarManager.Instance.SetProgressValue(0, 1);
                        renderWVM.ProgressValue = 0;
                        renderWVM.ProgressMax = renderData.RenderItems[arg_index].Time.TotalMilliseconds;

                        FFmpegRenderConfig config = new FFmpegRenderConfig();
                        if (!string.IsNullOrEmpty(renderData.FFmpegPath))
                            config.WithFFmpegBinaryPath(renderData.FFmpegPath);
                        if (!string.IsNullOrEmpty(renderData.RenderItems[arg_index].WorkingDirectory))
                            config.WithWorkingDirectory(renderData.RenderItems[arg_index].WorkingDirectory);

                        FFmpegArgs.Executes.FFmpegRender render = FFmpegArgs.Executes.FFmpegRender.FromArguments(renderData.RenderItems[arg_index].Arguments, config);
                        render.OnEncodingProgress += RenderProgressDelegate;

                        FFmpegRenderResult result = await render.ExecuteAsync(cancellationTokenSource.Token);

                        foreach (var line in result.ErrorDatas) sw.WriteLine(line);
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        exitCode = result.ExitCode;
                        result.EnsureSuccess();
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (ex is AggregateException ae) ex = ae.InnerException;
                        if (ex is OperationCanceledException) return;
                        string error = ex.GetType().FullName + "\r\nMessage: " + ex.Message;
#if DEBUG
                        error += "\r\n" + ex.StackTrace;
#endif
                        sw?.WriteLine();
                        sw?.WriteLine(error + ",  Args: " + string.Join(";", renderData.RenderItems) + ", StackTrace: " + ex.StackTrace);
                        sw?.WriteLine();
                        Console.Error.WriteLine(error);
                        return;
                    }
                    finally
                    {
                        sw?.Dispose();
                    }
                }
            }
            finally
            {
                AllowClose = true;
                if (exitCode == 0)
                    this.Close();
                else
                    Application.Current.Shutdown(exitCode);
            }
        }

        void RenderProgressDelegate(RenderProgress renderProgress)
        {
            renderWVM.ProgressValue = renderProgress.Time.TotalMilliseconds;
            renderWVM.Percent = renderWVM.ProgressValue * 100 / renderWVM.ProgressMax;
            TaskbarManager.Instance.SetProgressValue((int)renderWVM.ProgressValue, (int)renderWVM.ProgressMax);
            Console.Out.WriteLine($"{arg_index + 1}/{renderData.RenderItems.Count}|{renderWVM.Percent}");
        }
    }
}

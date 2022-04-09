using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.WpfUi.FFmpegRender
{
    public class RenderItem
    {
        public string WorkingDirectory { get; set; }
        public TimeSpan Time { get; set; }
        public string Arguments { get; set; }
        public string LogPath { get; set; }
        public override string ToString()
        {
            return Arguments;
        }
    }
}

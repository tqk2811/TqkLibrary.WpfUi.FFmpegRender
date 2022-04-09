using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.WpfUi.FFmpegRender
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string WorkingDirectory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Arguments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LogPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Arguments;
        }
    }
}

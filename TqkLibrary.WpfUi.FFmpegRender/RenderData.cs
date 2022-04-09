using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.WpfUi.FFmpegRender
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderData
    {
        /// <summary>
        /// 
        /// </summary>
        public string FFmpegPath { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public List<RenderItem> RenderItems { get; set; }
    }
}

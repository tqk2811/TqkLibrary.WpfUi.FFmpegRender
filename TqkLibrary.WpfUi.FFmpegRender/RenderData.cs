using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.WpfUi.FFmpegRender
{
    public class RenderData
    {
        public string FFmpegPath { get; set; }
        public List<RenderItem> RenderItems { get; set; }
    }
}

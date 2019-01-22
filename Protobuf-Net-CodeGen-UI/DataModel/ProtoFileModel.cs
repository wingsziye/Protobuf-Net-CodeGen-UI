using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuf_Net_CodeGen_UI.DataModel
{
    public class ProtoFileModel: NotifyBase
    {
        private bool _IsSelected = false;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                this.OnPropertyChanged(nameof(IsSelected));
            }
        }

        private ProtoFile _ProtoFile;
        public ProtoFile ProtoFile
        {
            get
            {
                return _ProtoFile;
            }
            set
            {
                _ProtoFile = value;
                this.OnPropertyChanged(nameof(ProtoFile));
            }
        }
    }
}

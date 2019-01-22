using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Protobuf_Net_CodeGen_UI.Annotations;

namespace Protobuf_Net_CodeGen_UI.DataModel
{
    public class ProtoFile:INotifyPropertyChanged
    {
        private int iD;
        private string filePath;
        private string outputMessage;
        private string inputMessage;
        private string errMsg;

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; OnPropertyChanged(); }
        }

        public int ID
        {
            get { return iD; }
            set { iD = value; OnPropertyChanged(); }
        }

        public string OutputMessage
        {
            get { return outputMessage; }
            set { outputMessage = value;
                OnPropertyChanged();
            }
        }

        public string InputMessage
        {
            get { return inputMessage; }
            set { inputMessage = value; OnPropertyChanged(); }
        }

        public string ErrMsg
        {
            get { return errMsg; }
            set { errMsg = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

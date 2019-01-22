using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Protobuf_Net_CodeGen_UI.Annotations;
using Protobuf_Net_CodeGen_UI.CodeGen;
using Protobuf_Net_CodeGen_UI.Command;
using Protobuf_Net_CodeGen_UI.DataModel;


namespace Protobuf_Net_CodeGen_UI.ViewModels
{
    public class MainWindowViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel()
        {
            init();
        }
        private void init()
        {
            ProtoFiles = new ObservableCollection<ProtoFileModel>();
            LoadProFileCmd = new CommandBase(() =>
            {
                var files = OpenSelectFileDialog();
                AddFilePath(files);
            });
            LoadProDirCmd = new CommandBase(() =>
            {
                var files = OpenSelectDirectoryDialog();
                AddFilePath(files);
            });
            ExcuteTransferCmd = new DelegateCommand<IEnumerable>(ExcuteTransfer);
        }

        private async void ExcuteTransfer(IEnumerable items)
        {
            Console.WriteLine("Excute");
            var sels = ProtoFiles.ToList().FindAll(item => item.IsSelected);
            List<Task> tlist = new List<Task>();
            foreach (var s in sels)
            {
                var orginPath = s.ProtoFile.FilePath;
                var dirPath = ".\\SrcProto";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                Task t = new Task(() =>
                {
                    var src = Path.Combine(dirPath, Path.GetFileName(orginPath));
                    File.Copy(orginPath, src, true);
                    var state = ProtoGenerator.DoGen((output) => { s.ProtoFile.OutputMessage = output; },$"{src}", $"--csharp_out={OutputDir}");
                    if (!state)
                    {
                        s.ProtoFile.ErrMsg = s.ProtoFile.OutputMessage;
                    }
                    else
                    {
                        s.ProtoFile.ErrMsg = "Generated Succeed";
                        s.ProtoFile.InputMessage = File.ReadAllText(src);
                    }
                    
                });
                tlist.Add(t);
            }
            tlist.ForEach(t=>t.Start());
            Task.WaitAll(tlist.ToArray());
        }

        private void AddFilePath(params string[] files)
        {
            ProtoFiles.Clear();
            if (files==null|| files.Length<=0)
            {
                return;
            }
            for (int i = 0; i < files.Length; i++)
            {
                var f = files[i];
                ProtoFiles.Add(new ProtoFileModel() { ProtoFile = new ProtoFile() { FilePath = f, ID = i } });
            }
        }

        /// <summary>
        /// 选取文件，可多选
        /// </summary>
        private string[] OpenSelectFileDialog()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.CheckFileExists = true;//检查文件是否存在
            openFile.CheckPathExists = true;//检查路径是否存在
            openFile.Multiselect = true;//是否允许多选，false表示单选
            openFile.Filter = "(*.proto)|*.proto|All files(*.*)|*.*";
            openFile.DefaultExt = "(*.proto)|*.proto";

            if (openFile.ShowDialog() == true)
            {
                return openFile.FileNames;
            }
            return null;
        }

        /// <summary>
        /// 遍历文件夹中所有Proto文件，不遍历子文件夹
        /// </summary>
        private string[] OpenSelectDirectoryDialog()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择.Proto文件所在文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    //TODO 查找该文件夹下所有
                    DirectoryInfo root = new DirectoryInfo(dialog.SelectedPath);
                    FileInfo[] files = root.GetFiles();
                    var farray = files.ToList().FindAll((info)=>info.Extension.ToLower()==".proto").ConvertAll<string>(info=>info.FullName).ToArray();
                    return farray;
                }
            }
            return null;
        }

        private string outputDir=@".\output\";

        public CommandBase LoadProFileCmd { get; set; }
        public DelegateCommand<IEnumerable> ExcuteTransferCmd { get; set; }
        public CommandBase LoadProDirCmd { get; set; }

        private ObservableCollection<ProtoFileModel> protoFiles;
        public ObservableCollection<ProtoFileModel> ProtoFiles
        {
            get { return protoFiles; }
            set { protoFiles = value; OnPropertyChanged(nameof(ProtoFiles));}
        }

        private ProtoFileModel selProtoModel;

        public ProtoFileModel SelProtoModel
        {
            get { return selProtoModel; }
            set { selProtoModel = value; OnPropertyChanged();}
        }

        private bool _IsSelectAll = false;
        public bool IsSelectAll
        {
            get { return _IsSelectAll; }
            set
            {
                _IsSelectAll = value;
                OnPropertyChanged("IsSelectAll");
            }
        }


        private ICommand _SelectAllCommand;
        public ICommand SelectAllCommand
        {
            get
            {
                return _SelectAllCommand ?? (_SelectAllCommand = new DelegateCommand<object>(SelectAll));
            }
        }

        public void SelectAll(object id)
        {
            foreach (var item in ProtoFiles)
            {
                item.IsSelected = IsSelectAll;
            }
        }

        private ICommand _SelectCommand;
        public ICommand SelectCommand
        {
            get
            {
                return _SelectCommand ?? (_SelectCommand = new DelegateCommand<int>(Select));
            }
        }

        public void Select(int id)
        {
            ProtoFileModel md = ProtoFiles.Where(p => p.ProtoFile.ID == id).FirstOrDefault();
            if (md != null)
            {
                if (!md.IsSelected && IsSelectAll)
                {
                    IsSelectAll = false;
                }
                else if (md.IsSelected && !IsSelectAll)
                {
                    foreach (var item in ProtoFiles)
                    {
                        if (!item.IsSelected) return;
                    }
                    IsSelectAll = true;
                }
            }
        }
        public bool SelectValidate(bool onlyOne = false)
        {
            if (this.ProtoFiles.Count(p => p.IsSelected) < 1)
            {
                //MessageBox.Show("未勾选数据！");
                return false;
            }

            if (onlyOne)
            {
                if (this.ProtoFiles.Count(p => p.IsSelected) > 1)
                {
                    //MessageBox.Show("只能勾选一条数据！");
                    return false;
                }
            }

            return true;
        }

        private ICommand _DelCommand;
        public ICommand DelCommand
        {
            get
            {
                return _DelCommand ?? (_DelCommand = new DelegateCommand<object>(Del));
            }
        }

        public void Del(object obj = null)
        {
            if (!SelectValidate()) return;

            MessageBoxResult result = MessageBox.Show("是否删除选中项?", "提示", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();

            bool hasSelect = false;
            //获取选中的数据
            foreach (var v in ProtoFiles)
            {
                if (v.IsSelected) { sb.Append(v.ProtoFile.FilePath + ";"); hasSelect = true; }
            }
            if (!hasSelect)
            {
                //MessageBox.Show("请选择修改项", "警告");
                return;
            }

            //MessageBox.Show("选中 " + sb.ToString());
        }

        private ICommand _EditCommand;
        public ICommand EditCommand
        {
            get
            {
                return _EditCommand ?? (_EditCommand = new DelegateCommand<object>(Edit));
            }
        }

        public string OutputDir
        {
            get { return outputDir; }
            set { outputDir = value;OnPropertyChanged(nameof(OutputDir)); }
        }


        public void Edit(object obj = null)
        {
            if (!SelectValidate(true)) return;

            ProtoFileModel model = null;


            bool hasSelect = false;
            //获取选中的数据
            foreach (var v in ProtoFiles)
            {
                if (v.IsSelected) { model = v; hasSelect = true; break; }
            }
            if (!hasSelect)
            {
                //MessageBox.Show("请选择修改项", "警告");
                return;
            }

            //MessageBox.Show("选中 " + model.ProtoFile.FilePath);

        }
    }
}

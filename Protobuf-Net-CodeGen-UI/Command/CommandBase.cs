using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Protobuf_Net_CodeGen_UI.Command
{
    public class CommandBase : ICommand
    {
        #region Private Fields  
        private readonly Action _command;
        private readonly Func<object, bool> _canExecute;
        #endregion
        #region Constructor  
        /// <summary>  
        /// 实例化一个CommandBase对象  
        /// </summary>  
        /// <param name="command">委托一个有object类型参数的命令执行函数</param>  
        /// <param name="canExecute">委托一个有object类型参数的命令是否能被执行的函数（可选）</param>  
        /// <exception cref="ArgumentNullException">参数command不可以为null引用</exception>  
        public CommandBase(Action command, Func<object, bool> canExecute = null)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            _canExecute = canExecute;
            _command = command;
        }
        #endregion
        #region ICommand Members  
        public void Execute(object parameter)
        {
            _command();
        }
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion
    }
}

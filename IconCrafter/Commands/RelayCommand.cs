using System;
using System.Windows.Input;

namespace IconCrafter.Commands
{
    /// <summary>
    /// 通用的命令实现类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;
        
        /// <summary>
        /// 初始化RelayCommand实例
        /// </summary>
        /// <param name="execute">执行的操作</param>
        /// <param name="canExecute">是否可以执行的判断</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        /// <summary>
        /// 当命令的可执行状态发生变化时触发
        /// </summary>
        public event EventHandler? CanExecuteChanged;
        
        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>是否可以执行</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }
        
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            _execute();
        }
        
        /// <summary>
        /// 手动触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// 带参数的通用命令实现类
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;
        
        /// <summary>
        /// 初始化RelayCommand实例
        /// </summary>
        /// <param name="execute">执行的操作</param>
        /// <param name="canExecute">是否可以执行的判断</param>
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        /// <summary>
        /// 当命令的可执行状态发生变化时触发
        /// </summary>
        public event EventHandler? CanExecuteChanged;
        
        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>是否可以执行</returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;
            
            return _canExecute?.Invoke(default(T)!) ?? true;
        }
        
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            if (parameter is T typedParameter)
                _execute(typedParameter);
            else
                _execute(default(T)!);
        }
        
        /// <summary>
        /// 手动触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
using System;
using System.Windows.Input;

namespace SRO.ViewModels
{
    /// <summary>
    /// A Command that runs an Action.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        #region Private Members
        /// <summary>
        /// The action to run.
        /// </summary>
        private readonly Action<T> mAction;
        /// <summary>
        /// The execute validation.
        /// </summary>
        private readonly Predicate<T> mCanExecute;
        #endregion

        #region Public Events
        /// <summary>
        /// The event thats fired when the <see cref="CanExecute(object)"/> value has changed.
        /// </summary>
        public event EventHandler CanExecuteChanged = (sender, e) => { };
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelayCommand(Action<T> action, Predicate<T> canExecute = null)
        {
            mAction = action ?? throw new ArgumentNullException(nameof(action));
            mCanExecute = canExecute;
        }
        #endregion

        #region Command Methods
        /// <summary>
        /// Explicitly triggers the <see cref="CanExecuteChanged"/> event to re-evaluate execution status.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Check if command can be executed.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (mCanExecute == null)
                return true;

            if (parameter == null && typeof(T).IsValueType)
                return mCanExecute(default);

            return mCanExecute((T)parameter);
        }
        /// <summary>
        /// Executes the commands action.
        /// </summary>
        public void Execute(object parameter)
        {
            mAction((T)parameter);
        }
        #endregion
    }
}
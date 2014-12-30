using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesys.WebServicesClient.Components
{
    public class NotifyPropertyChangedSupport : INotifyPropertyChanged
    {
        protected void ChangeAndNotifyProperty(string propertyName, object value)
        {
            // TODO: check if value has really changed
            GetType().GetProperty(propertyName).SetValue(this, value);
            RaisePropertyChanged(propertyName);
        }

        // An alternative for notifying from within the setter method. No need to use property names strings this way.
        protected void RaiseThisPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            RaisePropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // Saved for future reference if having to implement more complex data binding objects.
        #region INotifyDataErrorInfo

        //Dictionary<string, IList<string>> errors = new Dictionary<string, IList<string>>();
        //public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        //public IEnumerable GetErrors(string propertyName)
        //{
        //    return errors.ContainsKey(propertyName) ?
        //        errors[propertyName] :
        //        Enumerable.Empty<string>();
        //}

        //public bool HasErrors
        //{
        //    get { return errors.Count > 0; }
        //}

        //void RaiseError(string propertyName, string error)
        //{
        //    errors[propertyName] = new string[] { error };

        //    if (ErrorsChanged != null)
        //        ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));

        //    RaisePropertyChanged(propertyName);
        //}

        //void RemoveError(string propertyName)
        //{
        //    errors.Remove(propertyName);
        //}

        //#endregion INotifyDataErrorInfo

        //#region IDataErrorInfo

        //string IDataErrorInfo.Error
        //{
        //    get
        //    {
        //        return HasErrors ? "Error" : "";
        //    }
        //}

        //string IDataErrorInfo.this[string columnName]
        //{
        //    get
        //    {
        //        return GetErrors(columnName).Cast<string>().FirstOrDefault() ?? "";                    
        //    }
        //}

        #endregion IDataErrorInfo
    }
}

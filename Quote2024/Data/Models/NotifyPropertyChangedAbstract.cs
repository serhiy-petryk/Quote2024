using System.ComponentModel;

namespace Data.Models
{
    public abstract class NotifyPropertyChangedAbstract : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertiesChanged(params string[] propertyNames) // [CallerMemberName]
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public abstract void UpdateUI();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Расписание : INotifyPropertyChanged
    {
        private int _id;
        private int _врач_id;
        private string _день_недели;
        private TimeSpan _время_начала;
        private TimeSpan _время_окончания;
        private TimeSpan? _перерыв_начала;
        private TimeSpan? _перерыв_окончания;
        private bool _активен;

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(id)); }
        }

        public int врач_id
        {
            get => _врач_id;
            set { _врач_id = value; OnPropertyChanged(nameof(врач_id)); }
        }

        public string день_недели
        {
            get => _день_недели;
            set { _день_недели = value; OnPropertyChanged(nameof(день_недели)); }
        }

        public TimeSpan время_начала
        {
            get => _время_начала;
            set { _время_начала = value; OnPropertyChanged(nameof(время_начала)); }
        }

        public TimeSpan время_окончания
        {
            get => _время_окончания;
            set { _время_окончания = value; OnPropertyChanged(nameof(время_окончания)); }
        }

        public TimeSpan? перерыв_начала
        {
            get => _перерыв_начала;
            set { _перерыв_начала = value; OnPropertyChanged(nameof(перерыв_начала)); }
        }

        public TimeSpan? перерыв_окончания
        {
            get => _перерыв_окончания;
            set { _перерыв_окончания = value; OnPropertyChanged(nameof(перерыв_окончания)); }
        }

        public bool активен
        {
            get => _активен;
            set { _активен = value; OnPropertyChanged(nameof(активен)); }
        }

        public virtual Врач Врач { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

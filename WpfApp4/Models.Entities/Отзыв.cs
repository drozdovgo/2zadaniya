using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Отзыв : INotifyPropertyChanged
    {
        private int _id;
        private int _запись_id;
        private int _оценка;
        private string _комментарий;
        private DateTime _дата_создания;
        private bool _одобрен;

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(id)); }
        }

        public int запись_id
        {
            get => _запись_id;
            set { _запись_id = value; OnPropertyChanged(nameof(запись_id)); }
        }

        public int оценка
        {
            get => _оценка;
            set { _оценка = value; OnPropertyChanged(nameof(оценка)); }
        }

        public string комментарий
        {
            get => _комментарий;
            set { _комментарий = value; OnPropertyChanged(nameof(комментарий)); }
        }

        public DateTime дата_создания
        {
            get => _дата_создания;
            set { _дата_создания = value; OnPropertyChanged(nameof(дата_создания)); }
        }

        public bool одобрен
        {
            get => _одобрен;
            set { _одобрен = value; OnPropertyChanged(nameof(одобрен)); }
        }

        public virtual Запись Запись { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


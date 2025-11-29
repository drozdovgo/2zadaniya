using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Врач : INotifyPropertyChanged
    {
        private int _id;
        private int _пользователь_id;
        private int _специализация_id;
        private string _лицензия;
        private string _страхование;
        private string _программа;
        private decimal _рейтинг;
        private string _статус;

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(id)); }
        }

        public int пользователь_id
        {
            get => _пользователь_id;
            set { _пользователь_id = value; OnPropertyChanged(nameof(пользователь_id)); }
        }

        public int специализация_id
        {
            get => _специализация_id;
            set { _специализация_id = value; OnPropertyChanged(nameof(специализация_id)); }
        }

        public string лицензия
        {
            get => _лицензия;
            set { _лицензия = value; OnPropertyChanged(nameof(лицензия)); }
        }

        public string страхование
        {
            get => _страхование;
            set { _страхование = value; OnPropertyChanged(nameof(страхование)); }
        }

        public string программа
        {
            get => _программа;
            set { _программа = value; OnPropertyChanged(nameof(программа)); }
        }

        public decimal рейтинг
        {
            get => _рейтинг;
            set { _рейтинг = value; OnPropertyChanged(nameof(рейтинг)); }
        }

        public string статус
        {
            get => _статус;
            set { _статус = value; OnPropertyChanged(nameof(статус)); }
        }


        public virtual Пользователь Пользователь { get; set; }
        public virtual Специализация Специализация { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


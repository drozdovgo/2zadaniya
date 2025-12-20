using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Запись : INotifyPropertyChanged
    {
        private int _id;
        private int _пациент_id;
        private int _врач_id;
        private DateTime _дата_записи;
        private TimeSpan _время_записи;
        private string _статус;
        private string _симптомы;
        private string _диагноз;
        private string _рекомендации;
        private DateTime _дата_создания;

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(id)); }
        }

        public int пациент_id
        {
            get => _пациент_id;
            set { _пациент_id = value; OnPropertyChanged(nameof(пациент_id)); }
        }

        public int врач_id
        {
            get => _врач_id;
            set { _врач_id = value; OnPropertyChanged(nameof(врач_id)); }
        }

        public DateTime дата_записи
        {
            get => _дата_записи;
            set { _дата_записи = value; OnPropertyChanged(nameof(дата_записи)); }
        }

        public TimeSpan время_записи
        {
            get => _время_записи;
            set { _время_записи = value; OnPropertyChanged(nameof(время_записи)); }
        }

        public string статус
        {
            get => _статус;
            set { _статус = value; OnPropertyChanged(nameof(статус)); }
        }

        public string симптомы
        {
            get => _симптомы;
            set { _симптомы = value; OnPropertyChanged(nameof(симптомы)); }
        }

        public string диагноз
        {
            get => _диагноз;
            set { _диагноз = value ?? string.Empty; OnPropertyChanged(nameof(диагноз)); }
        }

        public string рекомендации
        {
            get => _рекомендации;
            set { _рекомендации = value ?? string.Empty; OnPropertyChanged(nameof(рекомендации)); }
        }

        public DateTime дата_создания
        {
            get => _дата_создания;
            set { _дата_создания = value; OnPropertyChanged(nameof(дата_создания)); }
        }


        public virtual Пользователь Пациент { get; set; }
        public virtual Врач Врач { get; set; }
        public virtual Пользователь Пользователь { get; set; }
        public virtual Специализация Специализация { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Запись()
        {
            // Устанавливаем значения по умолчанию
            симптомы = string.Empty;
            диагноз = string.Empty;
            рекомендации = string.Empty;
            статус = "запланирована";
            дата_создания = DateTime.Now;

            // Устанавливаем дату записи по умолчанию (завтра)
            дата_записи = DateTime.Now.AddDays(1);
            время_записи = new TimeSpan(9, 0, 0);
        }
    }
}
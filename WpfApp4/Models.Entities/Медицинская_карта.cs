using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Медицинская_карта : INotifyPropertyChanged
    {
        private int _id;
        private int _пациент_id;
        private string _группа_крови;
        private string _аллергии;
        private string _хронические_заболевания;
        private DateTime _дата_создания;
        private DateTime _дата_обновления;

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

        public string группа_крови
        {
            get => _группа_крови;
            set { _группа_крови = value; OnPropertyChanged(nameof(группа_крови)); }
        }

        public string аллергии
        {
            get => _аллергии;
            set { _аллергии = value; OnPropertyChanged(nameof(аллергии)); }
        }

        public string хронические_заболевания
        {
            get => _хронические_заболевания;
            set { _хронические_заболевания = value; OnPropertyChanged(nameof(хронические_заболевания)); }
        }

        public DateTime дата_создания
        {
            get => _дата_создания;
            set { _дата_создания = value; OnPropertyChanged(nameof(дата_создания)); }
        }

        public DateTime дата_обновления
        {
            get => _дата_обновления;
            set { _дата_обновления = value; OnPropertyChanged(nameof(дата_обновления)); }
        }


        public virtual Пользователь Пользователь { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


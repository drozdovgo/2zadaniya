using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Специализация : INotifyPropertyChanged
    {
        private int _id;
        private string _название;
        private string _описание;
        private string _категория;

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(id)); }
        }

        public string название
        {
            get => _название;
            set { _название = value; OnPropertyChanged(nameof(название)); }
        }

        public string описание
        {
            get => _описание;
            set { _описание = value; OnPropertyChanged(nameof(описание)); }
        }

        public string категория
        {
            get => _категория;
            set { _категория = value; OnPropertyChanged(nameof(категория)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

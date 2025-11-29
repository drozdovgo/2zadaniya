using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models.Entities
{
    public class Пользователь : PropertyChangedBase
    {
        private int _id;
        private string _email;
        private string _пароль;
        private string _роль;
        private string _имя;
        private string _фамилия;
        private string _телефон;
        private DateTime? _дата_рождения;
        private DateTime _дата_регистрации;
        private bool _активен;

        public int id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(id)); }
        }

        public string email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(email)); }
        }

        public string пароль
        {
            get => _пароль;
            set { _пароль = value; OnPropertyChanged(nameof(пароль)); }
        }

        public string роль
        {
            get => _роль;
            set { _роль = value; OnPropertyChanged(nameof(роль)); }
        }

        public string имя
        {
            get => _имя;
            set { _имя = value; OnPropertyChanged(nameof(имя)); }
        }

        public string фамилия
        {
            get => _фамилия;
            set { _фамилия = value; OnPropertyChanged(nameof(фамилия)); }
        }

        public string телефон
        {
            get => _телефон;
            set { _телефон = value; OnPropertyChanged(nameof(телефон)); }
        }

        public DateTime? дата_рождения
        {
            get => _дата_рождения;
            set { _дата_рождения = value; OnPropertyChanged(nameof(дата_рождения)); }
        }

        public DateTime дата_регистрации
        {
            get => _дата_регистрации;
            set { _дата_регистрации = value; OnPropertyChanged(nameof(дата_регистрации)); }
        }

        public bool активен
        {
            get => _активен;
            set { _активен = value; OnPropertyChanged(nameof(активен)); }
        }

        public string ПолноеИмя => $"{фамилия} {имя}";


        public Пользователь()
        {
            дата_регистрации = DateTime.Now;
            активен = true;
        }
    }
}



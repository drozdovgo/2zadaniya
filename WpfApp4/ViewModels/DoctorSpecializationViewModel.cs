using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;  // Добавьте эту строку
using WpfApp4.Domain;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class DoctorSpecializationViewModel : PropertyChangedBase
    {
        private readonly MyDatabaseContext _dbContext;
        private readonly Пользователь _currentUser;
        private ObservableCollection<Врач> _doctors;
        private ObservableCollection<Врач> _filteredDoctors;
        private ObservableCollection<Специализация> _specializations;
        private Специализация _selectedSpecialization;
        private string _searchText = string.Empty;

        public ObservableCollection<Врач> Doctors
        {
            get => _doctors;
            set { _doctors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Врач> FilteredDoctors
        {
            get => _filteredDoctors;
            set { _filteredDoctors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Специализация> Specializations
        {
            get => _specializations;
            set { _specializations = value; OnPropertyChanged(); }
        }

        public Специализация SelectedSpecialization
        {
            get => _selectedSpecialization;
            set
            {
                _selectedSpecialization = value;
                OnPropertyChanged();
                FilterDoctors();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterDoctors();
            }
        }

        public string WelcomeMessage => $"Выбор врача ({_currentUser.ПолноеИмя})";

        // Команды
        public MyCommand BackCommand { get; }
        public MyCommand ClearFilterCommand { get; }

        // События
        public event Action? BackRequested;

        public DoctorSpecializationViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;
            _dbContext = new MyDatabaseContext("Data Source=medicalclinic.db");

            BackCommand = new MyCommand(_ => BackRequested?.Invoke());
            ClearFilterCommand = new MyCommand(_ => ClearFilter());

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загружаем врачей с информацией о пользователе и специализации
                var doctors = _dbContext.Врач
                    .Include(d => d.Пользователь)
                    .Include(d => d.Специализация)
                    .Where(d => d.статус == "активен")
                    .ToList();

                Doctors = new ObservableCollection<Врач>(doctors);
                FilteredDoctors = new ObservableCollection<Врач>(doctors);

                // Загружаем все специализации
                var specializations = _dbContext.Специализация.ToList();
                Specializations = new ObservableCollection<Специализация>(specializations);

                System.Diagnostics.Debug.WriteLine($"=== DoctorSpecializationView ===");
                System.Diagnostics.Debug.WriteLine($"Загружено врачей: {doctors.Count}");
                System.Diagnostics.Debug.WriteLine($"Загружено специализаций: {specializations.Count}");

                // Проверка данных
                foreach (var doctor in doctors.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"Врач: {doctor.Пользователь?.ПолноеИмя}, " +
                        $"Специализация: {doctor.Специализация?.название}, " +
                        $"ID: {doctor.id}, Пользователь ID: {doctor.пользователь_id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки данных: {ex.Message}");
                Doctors = new ObservableCollection<Врач>();
                FilteredDoctors = new ObservableCollection<Врач>();
                Specializations = new ObservableCollection<Специализация>();
            }
        }

        private void FilterDoctors()
        {
            if (Doctors == null) return;

            var query = Doctors.AsEnumerable();

            // Фильтрация по выбранной специализации
            if (SelectedSpecialization != null)
            {
                query = query.Where(d => d.специализация_id == SelectedSpecialization.id);
            }

            // Фильтрация по поисковому тексту
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                query = query.Where(d =>
                    (d.Пользователь?.имя?.ToLower().Contains(searchLower) ?? false) ||
                    (d.Пользователь?.фамилия?.ToLower().Contains(searchLower) ?? false) ||
                    (d.Специализация?.название?.ToLower().Contains(searchLower) ?? false) ||
                    (d.Специализация?.описание?.ToLower().Contains(searchLower) ?? false));
            }

            FilteredDoctors = new ObservableCollection<Врач>(query.ToList());
        }

        private void ClearFilter()
        {
            SelectedSpecialization = null;
            SearchText = string.Empty;
            FilteredDoctors = new ObservableCollection<Врач>(Doctors);
        }
    }
}
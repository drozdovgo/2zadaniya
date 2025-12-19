using System;
using System.Windows;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private object _currentViewModel = null!;
        private Пользователь? _currentUser;
        private string _windowTitle = "Медицинская клиника";

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public Пользователь? CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        public MainWindowViewModel()
        {
            ShowLoginView();
        }

        public void ShowLoginView()
        {
            var loginViewModel = new LoginViewModel();
            loginViewModel.LoginSuccessful += OnLoginSuccessful;
            loginViewModel.ShowRegisterRequested += ShowRegisterView;
            loginViewModel.CloseApplicationRequested += CloseApplication;

            CurrentViewModel = loginViewModel;
            WindowTitle = "Медицинская клиника - Авторизация";
        }

        public void ShowRegisterView()
        {
            var registerViewModel = new RegisterViewModel();
            registerViewModel.ShowLoginRequested += ShowLoginView;
            registerViewModel.RegistrationSuccessful += OnLoginSuccessful;

            CurrentViewModel = registerViewModel;
            WindowTitle = "Медицинская клиника - Регистрация";
        }

        public void ShowMainView()
        {
            if (CurrentUser == null) return;

            var mainViewModel = new MainContentViewModel(CurrentUser);
            mainViewModel.LogoutRequested += OnLogout;
            mainViewModel.ViewDoctorSpecializationsRequested += ShowDoctorSpecializationView;

            // Подписываемся на события в зависимости от роли
            if (CurrentUser.роль == "пациент")
            {
                mainViewModel.ViewPatientAppointmentsRequested += ShowPatientAppointmentView;
            }
            else if (CurrentUser.роль == "врач")
            {
                mainViewModel.ViewDoctorScheduleRequested += ShowDoctorScheduleView;
            }
            else if (CurrentUser.роль == "администратор")
            {
                mainViewModel.ViewAdminScheduleRequested += ShowAdminScheduleView;
            }

            CurrentViewModel = mainViewModel;
            WindowTitle = $"Медицинская клиника - {CurrentUser.ПолноеИмя}";
        }

        public void ShowDoctorSpecializationView()
        {
            if (CurrentUser == null) return;

            var doctorSpecializationViewModel = new DoctorSpecializationViewModel(CurrentUser);
            doctorSpecializationViewModel.BackRequested += ShowMainView;

            CurrentViewModel = doctorSpecializationViewModel;
            WindowTitle = $"Медицинская клиника - Выбор врача";
        }

        public void ShowPatientAppointmentView()
        {
            if (CurrentUser == null) return;

            var patientAppointmentViewModel = new PatientAppointmentViewModel(CurrentUser);
            patientAppointmentViewModel.BackRequested += ShowMainView;

            CurrentViewModel = patientAppointmentViewModel;
            WindowTitle = $"Медицинская клиника - Мои записи";
        }

        public void ShowDoctorScheduleView()
        {
            if (CurrentUser == null) return;

            var doctorScheduleViewModel = new DoctorScheduleViewModel(CurrentUser);
            doctorScheduleViewModel.BackRequested += ShowMainView;

            CurrentViewModel = doctorScheduleViewModel;
            WindowTitle = $"Медицинская клиника - Расписание врача";
        }

        public void ShowAdminScheduleView()
        {
            if (CurrentUser == null) return;

            var adminScheduleViewModel = new AdminScheduleViewModel(CurrentUser);
            adminScheduleViewModel.BackRequested += ShowMainView;

            CurrentViewModel = adminScheduleViewModel;
            WindowTitle = $"Медицинская клиника - Управление записями";
        }

        private void OnLoginSuccessful(Пользователь user)
        {
            CurrentUser = user;
            ShowMainView();
        }

        private void OnLogout()
        {
            CurrentUser = null;
            ShowLoginView();
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
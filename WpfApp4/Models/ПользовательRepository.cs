using WpfApp4.Domain;
using WpfApp4.Interfaces;
using WpfApp4.Models.Entities;

namespace WpfApp4.Models
{
    public class ПользовательRepository : IRepository<Пользователь>
    {
        private readonly string conn = "Data Source=medicalclinic.db";



        public IEnumerable<Пользователь> GetAll()
        {
            try
            {
                using var context = new MyDatabaseContext(conn);
                var users = context.Пользователь.ToList();
                System.Diagnostics.Debug.WriteLine($"✅ Загружено {users.Count} пользователей из базы данных");
                return users;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения данных: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return new List<Пользователь>();
            }
        }

        public Пользователь? Get(int id)
        {
            try
            {
                using var context = new MyDatabaseContext(conn);
                return context.Пользователь.FirstOrDefault(u => u.id == id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения пользователя: {ex.Message}");
                return null;
            }
        }

        public bool Add(Пользователь user)
        {
            try
            {
                using var context = new MyDatabaseContext(conn);
                context.Пользователь.Add(user);
                var result = context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"✅ Пользователь добавлен, изменений: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления пользователя: {ex.Message}");
                return false;
            }
        }

        public bool Remove(int id)
        {
            try
            {
                using var context = new MyDatabaseContext(conn);
                var user = context.Пользователь.Find(id);
                if (user != null)
                {
                    context.Пользователь.Remove(user);
                    var result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления пользователя: {ex.Message}");
            }
            return false;
        }

        public bool Update(int id, Пользователь entity)
        {
            try
            {
                using var context = new MyDatabaseContext(conn);
                var user = context.Пользователь.Find(id);
                if (user != null)
                {
                    user.email = entity.email;
                    user.пароль = entity.пароль;
                    user.роль = entity.роль;
                    user.имя = entity.имя;
                    user.фамилия = entity.фамилия;
                    user.телефон = entity.телефон;
                    user.дата_рождения = entity.дата_рождения;
                    user.активен = entity.активен;
                    var result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления пользователя: {ex.Message}");
            }
            return false;
        }
    
    public bool TestConnection()
        {
            try
            {
                using var context = new MyDatabaseContext(conn);
                var canConnect = context.Database.CanConnect();
                System.Diagnostics.Debug.WriteLine($"🔌 Тест подключения: {canConnect}");
                return canConnect;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка подключения к БД: {ex.Message}");
                return false;
            }
        }

    }
}

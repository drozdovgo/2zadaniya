using WpfApp4.Domain;
using WpfApp4.Interfaces;
using WpfApp4.Models.Entities;

namespace WpfApp4.Models
{
    public class SimpleUserRepository : IRepository<Пользователь>
    {
        public IEnumerable<Пользователь> GetAll()
        {
            try
            {
                using var context = new SimpleDatabaseContext();
                context.Database.EnsureCreated();

                if (!context.Пользователь.Any())
                {
                    System.Diagnostics.Debug.WriteLine("📝 База пустая, добавляем тестовые данные...");
                    context.Пользователь.AddRange(
                        new Пользователь
                        {
                            id = 1,
                            имя = "Иван",
                            фамилия = "Петров",
                            email = "ivan@mail.ru",
                            пароль = "123456",
                            роль = "пациент",
                            телефон = "+79161111111",
                            дата_рождения = new DateTime(1985, 5, 15),
                            активен = true
                        },
                        new Пользователь
                        {
                            id = 2,
                            имя = "Мария",
                            фамилия = "Сидорова",
                            email = "maria@clinic.ru",
                            пароль = "123456",
                            роль = "врач",
                            телефон = "+79162222222",
                            дата_рождения = new DateTime(1978, 12, 3),
                            активен = true
                        },
                        new Пользователь
                        {
                            id = 3,
                            имя = "Алексей",
                            фамилия = "Козлов",
                            email = "alexey@clinic.ru",
                            пароль = "123456",
                            роль = "администратор",
                            телефон = "+79163333333",
                            дата_рождения = new DateTime(1990, 8, 22),
                            активен = true
                        }
                    );
                    context.SaveChanges();
                }

                var users = context.Пользователь.ToList();
                System.Diagnostics.Debug.WriteLine($"✅ SQLite: Загружено {users.Count} пользователей");
                return users;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка SQLite: {ex.Message}");
                return GetFallbackData();
            }
        }

        public Пользователь? Get(int id)
        {
            try
            {
                using var context = new SimpleDatabaseContext();
                return context.Пользователь.FirstOrDefault(u => u.id == id);
            }
            catch
            {
                return GetFallbackData().FirstOrDefault(u => u.id == id);
            }
        }

        public bool Add(Пользователь user)
        {
            try
            {
                using var context = new SimpleDatabaseContext();
                context.Пользователь.Add(user);
                var result = context.SaveChanges();
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления: {ex.Message}");
                return false;
            }
        }

        public bool Remove(int id)
        {
            try
            {
                using var context = new SimpleDatabaseContext();
                var user = context.Пользователь.Find(id);
                if (user != null)
                {
                    context.Пользователь.Remove(user);
                    return context.SaveChanges() > 0;
                }
            }
            catch { }
            return false;
        }

        public bool Update(int id, Пользователь entity)
        {
            try
            {
                using var context = new SimpleDatabaseContext();
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
                    return context.SaveChanges() > 0;
                }
            }
            catch { }
            return false;
        }

        private List<Пользователь> GetFallbackData()
        {
            return new List<Пользователь>
            {
                new Пользователь
                {
                    id = 1,
                    имя = "Резервный",
                    фамилия = "Пользователь 1",
                    email = "fallback1@test.ru",
                    пароль = "123456",
                    роль = "пациент",
                    телефон = "+79160000001",
                    активен = true
                },
                new Пользователь
                {
                    id = 2,
                    имя = "Резервный",
                    фамилия = "Пользователь 2",
                    email = "fallback2@test.ru",
                    пароль = "123456",
                    роль = "врач",
                    телефон = "+79160000002",
                    активен = true
                },
                new Пользователь
                {
                    id = 3,
                    имя = "Резервный",
                    фамилия = "Пользователь 3",
                    email = "fallback3@test.ru",
                    пароль = "123456",
                    роль = "администратор",
                    телефон = "+79160000003",
                    активен = true
                }
            };
        }
    }
}
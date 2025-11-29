using System.Collections.Generic;

namespace WpfApp4.Interfaces
{
    public interface IRepository<T>
    {
        public T? Get(int id);
        public IEnumerable<T>? GetAll();
        public bool Add(T entity);
        public bool Remove(int id);
        public bool Update(int id, T entity);
    }
}
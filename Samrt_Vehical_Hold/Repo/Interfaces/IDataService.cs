using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samrt_Vehical_Hold.Repo.Interface
{
    public interface IDataService
    {
        Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
        Task<T> GetByIdAsync<T>(object key) where T : class;
        Task<T> CreateAsync<T>(T entity) where T : class;
        Task<T> UpdateAsync<T>(T entity) where T : class;
        Task<bool> DeleteAsync<T>(object key) where T : class;
        IQueryable<T> Query<T>() where T : class;
    }
}

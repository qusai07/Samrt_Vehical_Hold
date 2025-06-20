using Microsoft.EntityFrameworkCore;
using Samrt_Vehical_Hold.Data;
using Samrt_Vehical_Hold.Models;
using Samrt_Vehical_Hold.Repo.Impement;
using Samrt_Vehical_Hold.Repo.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samrt_Vehical_Hold.Repo.Impement
{
    public class DataService : IDataService
    {
        private readonly ApplicationDbContext _context;

        public DataService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync<T>(object key) where T : class
        {
            return await _context.Set<T>().FindAsync(key);
        }

        public async Task<T> CreateAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync<T>(object key) where T : class
        {
            var entity = await _context.Set<T>().FindAsync(key);
            if (entity == null)
                return false;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return _context.Set<T>().AsQueryable();
        }
    }
}


//var allUsers = await _dataService.GetAllAsync<ApplicationUser>();
//var singleUser = await _dataService.GetByIdAsync<ApplicationUser>(userId);
//await _dataService.CreateAsync(new ApplicationUser { UserName = "test" });
//await _dataService.DeleteAsync<ApplicationUser>(userId);

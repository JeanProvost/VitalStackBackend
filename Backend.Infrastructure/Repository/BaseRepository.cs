using Backend.Core.Interfaces.IRepository;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected DbSet<T> DbSet => _context.Set<T>();

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T> Create(T model)
        {
            await _context.Set<T>().AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<T> Update(T model)
        {
            _context.Entry(model).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task Delete(T model)
        {
            _context.Set<T>().Remove(model);
            await _context.SaveChangesAsync();
        }
    }
}

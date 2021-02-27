using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        public UnitOfWork(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        //DB.SaveChanges
        public void Commit()
        {
            _context.SaveChanges();
        }

        //DB.SaveChanges asenkonron
        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore.Storage;
using RumahMakanPadangAuth.dal.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RumahMakanPadangAuth.dal.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<User> UserRepository { get; }
        IBaseRepository<Role> RoleRepository { get; }
        IBaseRepository<UserRole> UserRoleRepository { get; }
        void Save();
        Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken));
        IDbContextTransaction StartNewTransaction();
        Task<IDbContextTransaction> StartNewTransactionAsync();
        Task<int> ExecuteSqlCommandAsync(string sql, object[] parameters, CancellationToken cancellationToken = default(CancellationToken));
    }
}

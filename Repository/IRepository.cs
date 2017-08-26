using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using SampleApi.Models;
using Microsoft.AspNetCore.Identity;

namespace SampleApi.Repository
{
    public interface IRepository
    {
        string TenantId { get; set; }

        Task<IEnumerable<TResult>> GetAllAsync<TEntity, TResult>(
        int? skip = null,
        int? limit = null,
        string[] includeProperties = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        where TEntity : class, IEntity;

        Task<IEnumerable<TEntity>> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            int? skip = null,
            int? limit = null,
            string[] includeProperties = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
            where TEntity : class, IEntity;

        Task<IEnumerable<TResult>> GetAsync<TEntity, TResult>(
            Expression<Func<TEntity, bool>> filter = null,
            int? skip = null,
            int? limit = null,
            string[] includeProperties = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
            where TEntity : class, IEntity;

        Task<TEntity> GetOneAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            string[] includeProperties = null)
            where TEntity : class, IEntity;

        Task<TEntity> GetByIdAsync<TEntity>(string id, string[] includeProperties = null)
            where TEntity : class, IEntity;

        Task<TResult> GetByIdAsync<TEntity, TResult>(string id, string[] includeProperties = null)
            where TEntity : class, IEntity;

        Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;
        
        Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;

        void Create<TEntity>(TEntity entity) where TEntity : class, IEntity;

        void Update<TEntity>(TEntity entity) where TEntity : class, IEntity;

        void Update<TEntity, TModel>(TEntity entity, TModel updatedEntity) where TEntity : class, IEntity;

        void Delete<TEntity>(TEntity entity) where TEntity : class, IEntity;

        void Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

        Task SaveAsync();

        bool Any<TEntity>() where TEntity : class, IEntity;

        void EnsureDatabaseCreated();

        UserManager<User> GetUserManager();

        SignInManager<User> GetSignInManager();

        RoleManager<Role> GetRoleManager();
    }
}
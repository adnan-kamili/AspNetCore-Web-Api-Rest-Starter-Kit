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
        Expression<Func<TEntity, TResult>> selectProperties,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string[] includeProperties = null,
        int? skip = null,
        int? limit = null)
        where TEntity : class, IEntity;

        Task<IEnumerable<TResult>> GetAsync<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string[] includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity;

        Task<TEntity> GetByIdAsync<TEntity>(string id, string[] includeProperties = null)
            where TEntity : class, IEntity;

        Task<TResult> GetByIdAsync<TEntity, TResult>(string id,string[] includeProperties = null)
            where TEntity : class, IEntity;

        Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;

        void Create<TEntity>(TEntity entity) where TEntity : class, IEntity;

        void Update<TEntity, TModel>(TEntity entity, TModel updatedEntity) where TEntity : class, IEntity;

        void Delete<TEntity>(TEntity entity) where TEntity : class, IEntity;

        Task SaveAsync();

        bool Any<TEntity>() where TEntity : class, IEntity;

        void EnsureDatabaseCreated();

        UserManager<User> GetUserManager();

        SignInManager<User> GetSignInManager();

        RoleManager<Role> GetRoleManager();
    }
}
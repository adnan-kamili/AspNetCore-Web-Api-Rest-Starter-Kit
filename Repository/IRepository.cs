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
        IEnumerable<TResult> GetAll<TEntity, TResult>(
       Expression<Func<TEntity, TResult>> selectProperties,
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
       string includeProperties = null,
       int? skip = null,
       int? limit = null)
       where TEntity : class, IEntity;

        Task<IEnumerable<TResult>> GetAllAsync<TEntity, TResult>(
        Expression<Func<TEntity, TResult>> selectProperties,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = null,
        int? skip = null,
        int? limit = null)
        where TEntity : class, IEntity;

        IEnumerable<TResult> Get<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity;

        Task<IEnumerable<TResult>> GetAsync<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity;

        TEntity GetById<TEntity>(string id, string includeProperties = null)
            where TEntity : class, IEntity;

        Task<TEntity> GetByIdAsync<TEntity>(string id, string includeProperties = null)
            where TEntity : class, IEntity;

        TResult GetById<TEntity, TResult>(
            string id,
            Expression<Func<TEntity, TResult>> selectProperties,
            string includeProperties = null)
            where TEntity : class, IEntity;

        Task<TResult> GetByIdAsync<TEntity, TResult>(
            string id,
            Expression<Func<TEntity, TResult>> selectProperties,
            string includeProperties = null)
            where TEntity : class, IEntity;

        int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;

        Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;

        bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;

        Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity;

        void Create<TEntity>(TEntity entity) where TEntity : class, IEntity;

        void Update<TEntity>(TEntity entity, TEntity updatedEntity) where TEntity : class, IEntity;

        void Delete<TEntity>(TEntity entity) where TEntity : class, IEntity;

        void Save();

        Task SaveAsync();

        bool Any<TEntity>() where TEntity : class, IEntity;

        void EnsureDatabaseCreated();

        UserManager<ApplicationUser> GetUserManager();

        SignInManager<ApplicationUser> GetSignInManager();

        RoleManager<ApplicationRole> GetRoleManager();
    }
}
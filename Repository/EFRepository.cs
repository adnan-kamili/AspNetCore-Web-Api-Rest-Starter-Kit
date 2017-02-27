using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using SampleApi.Models;

namespace SampleApi.Repository
{
    public class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        public string TenantId { get; set; }
        protected readonly TContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EFRepository(
            TContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
        }

        protected virtual IQueryable<TEntity> GetQueryable<TEntity>(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string[] includeProperties = null,
        int? skip = null,
        int? limit = null)
        where TEntity : class, IEntity
        {
            IQueryable<TEntity> query = context.Set<TEntity>();
            includeProperties = includeProperties ?? new string[] { };
            if (typeof(ITenantEntity).GetTypeInfo().IsAssignableFrom(typeof(TEntity).Ge‌​tTypeInfo()))
            {
                query = query.Where(entity => ((ITenantEntity)entity).TenantId == TenantId);
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }
            return query;
        }

        public virtual IEnumerable<TResult> GetAll<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,

       string[] includeProperties = null,
       int? skip = null,
       int? limit = null)
       where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(null, orderBy, includeProperties, skip, limit).Select(selectProperties).ToList();
        }

        public virtual async Task<IEnumerable<TResult>> GetAllAsync<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string[] includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(null, orderBy, includeProperties, skip, limit).Select(selectProperties).ToListAsync();
        }

        public virtual IEnumerable<TResult> Get<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,

            string[] includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter, orderBy, includeProperties, skip, limit).Select(selectProperties).ToList();
        }

        public virtual async Task<IEnumerable<TResult>> GetAsync<TEntity, TResult>(
            Expression<Func<TEntity, TResult>> selectProperties,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,

            string[] includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, orderBy, includeProperties, skip, limit).Select(selectProperties).ToListAsync();
        }

        public virtual TEntity GetById<TEntity>(string id, string[] includeProperties = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(e => e.Id == id, null, includeProperties).SingleOrDefault();
        }

        public virtual Task<TEntity> GetByIdAsync<TEntity>(string id, string[] includeProperties = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(e => e.Id == id, null, includeProperties).SingleOrDefaultAsync();
        }

        public virtual TResult GetById<TEntity, TResult>(
            string id,
            Expression<Func<TEntity, TResult>> selectProperties,
            string[] includeProperties = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(e => e.Id == id, null, includeProperties).Select(selectProperties).SingleOrDefault();
        }

        public virtual Task<TResult> GetByIdAsync<TEntity, TResult>(
             string id,
             Expression<Func<TEntity, TResult>> selectProperties,
             string[] includeProperties = null)
             where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(e => e.Id == id, null, includeProperties).Select(selectProperties).SingleOrDefaultAsync();
        }

        public virtual int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).Count();
        }

        public virtual Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).CountAsync();
        }

        public virtual bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).Any();
        }

        public virtual Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).AnyAsync();
        }

        public virtual void Create<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
            entity.CreatedAt = DateTime.UtcNow;

            if (entity is ITenantEntity)
            {
                ((ITenantEntity)entity).TenantId = TenantId;
            }

            context.Set<TEntity>().Add(entity);
        }
        public virtual void Update<TEntity, TModel>(TEntity entity, TModel updatedModel) where TEntity : class, IEntity
        {
            bool modified = false;
            // copy the value of properties from view model into entity
            PropertyInfo[] entityProperties = entity.GetType().GetProperties();
            foreach (PropertyInfo entityPropertyInfo in entityProperties)
            {
                PropertyInfo updatedModelPropertyInfo = updatedModel.GetType().GetProperty(entityPropertyInfo.Name);
                if (updatedModelPropertyInfo != null)
                {
                    var value = updatedModelPropertyInfo.GetValue(updatedModel, null);
                    if (value != null)
                    {
                        entityPropertyInfo.SetValue(entity, value, null);
                        modified = true;
                    }
                }
            }
            if (modified)
            {
                entity.ModifiedAt = DateTime.UtcNow;
                context.Entry(entity);
            }
        }

        public virtual void Delete<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
            var dbSet = context.Set<TEntity>();
            if (context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        public virtual Task SaveAsync()
        {
            return context.SaveChangesAsync();
        }

        public virtual void Save()
        {
            context.SaveChanges();
        }

        public virtual bool Any<TEntity>() where TEntity : class, IEntity
        {
            return context.Set<TEntity>().Any();
        }

        public virtual void EnsureDatabaseCreated()
        {
            context.Database.EnsureCreated();
        }

        public UserManager<ApplicationUser> GetUserManager()
        {
            return _userManager;
        }

        public SignInManager<ApplicationUser> GetSignInManager()
        {
            return _signInManager;
        }

        public RoleManager<ApplicationRole> GetRoleManager()
        {
            return _roleManager;
        }
    }
}




using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


using SampleApi.Models;

namespace SampleApi.Repository
{
    public class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        public object TenantId { get; set; }
        protected readonly TContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EFRepository(
            TContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
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
        string includeProperties = null,
        int? skip = null,
        int? limit = null)
        where TEntity : class, IEntity
        {
            includeProperties = includeProperties ?? string.Empty;
            IQueryable<TEntity> query = context.Set<TEntity>();
            if (typeof(ITenantEntity).GetTypeInfo().IsAssignableFrom(typeof(TEntity).Ge‌​tTypeInfo()))
            {
                query = query.Where(entity => ((ITenantEntity)entity).TenantId == TenantId);
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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

        public virtual IEnumerable<TEntity> GetAll<TEntity>(
       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
       string includeProperties = null,
       int? skip = null,
       int? limit = null)
       where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(null, orderBy, includeProperties, skip, limit).ToList();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(null, orderBy, includeProperties, skip, limit).ToListAsync();
        }

        public virtual IEnumerable<TEntity> Get<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter, orderBy, includeProperties, skip, limit).ToList();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? limit = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, orderBy, includeProperties, skip, limit).ToListAsync();
        }

        public virtual TEntity GetOne<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = "")
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter, null, includeProperties).SingleOrDefault();
        }

        public virtual async Task<TEntity> GetOneAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, null, includeProperties).SingleOrDefaultAsync();
        }

        public virtual TEntity GetById<TEntity>(object id)
            where TEntity : class, IEntity
        {
            object[] param = new object[2];
            param[0] = id;
            if (typeof(ITenantEntity).GetTypeInfo().IsAssignableFrom(typeof(TEntity).Ge‌​tTypeInfo()))
            {
                param[1] = new
                {
                    TenantId = TenantId
                };
            }

            return context.Set<TEntity>().Find(param);
        }

        public virtual Task<TEntity> GetByIdAsync<TEntity>(object id) where TEntity : class, IEntity
        {
            object[] param = new object[2];
            param[0] = id;
            if (typeof(ITenantEntity).GetTypeInfo().IsAssignableFrom(typeof(TEntity).Ge‌​tTypeInfo()))
            {
                param[1] = new
                {
                    TenantId = TenantId
                };
            }
            return context.Set<TEntity>().FindAsync(param);
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
        public virtual void Create<TEntity>(TEntity entity) where TEntity :  class, IEntity
        {
            entity.CreatedAt = DateTime.UtcNow;

            if (entity is ITenantEntity)
            {
                ((ITenantEntity)entity).TenantId = TenantId;
            }
            
            context.Set<TEntity>().Add(entity);
        }
        public virtual void Update<TEntity>(TEntity entity, TEntity updatedEntity) where TEntity : class, IEntity
        {
            if (updatedEntity is ITenantEntity)
            {
                ((ITenantEntity)entity).TenantId = TenantId;
            }
            updatedEntity.Id = entity.Id;
            PropertyInfo[] properties = entity.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.GetValue(updatedEntity, null) != null)
                {
                    propertyInfo.SetValue(entity, propertyInfo.GetValue(updatedEntity, null), null);
                }
            }
            entity.ModifiedAt = DateTime.UtcNow;
            context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
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

        public RoleManager<IdentityRole> GetRoleManager()
        {
            return _roleManager;
        }
    }
}




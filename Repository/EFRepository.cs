using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

using SampleApi.Models;

namespace SampleApi.Repository
{
    public class EFRepository<TContext> : IRepository where TContext : DbContext
    {
        public string TenantId { get; set; }
        protected readonly TContext context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public EFRepository(
            TContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IMapper mapper)
        {
            this.context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
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

        public virtual async Task<IEnumerable<TResult>> GetAllAsync<TEntity, TResult>(
            int? skip = null,
            int? limit = null,
            string[] includeProperties = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(null, orderBy, includeProperties, skip, limit).Select(entity => _mapper.Map<TResult>(entity)).ToListAsync();
        }

        public virtual async Task<IEnumerable<TResult>> GetAsync<TEntity, TResult>(
            Expression<Func<TEntity, bool>> filter = null,
            int? skip = null,
            int? limit = null,
            string[] includeProperties = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, orderBy, includeProperties, skip, limit).Select(entity => _mapper.Map<TResult>(entity)).ToListAsync();
        }

        public virtual Task<TEntity> GetByIdAsync<TEntity>(string id, string[] includeProperties = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(e => e.Id == id, null, includeProperties).SingleOrDefaultAsync();
        }

        public virtual Task<TResult> GetByIdAsync<TEntity, TResult>(
             string id,
             string[] includeProperties = null)
             where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(e => e.Id == id, null, includeProperties).Select(entity => _mapper.Map<TResult>(entity)).SingleOrDefaultAsync();
        }

        public virtual Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).CountAsync();
        }

        public virtual void Create<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;

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

        public virtual bool Any<TEntity>() where TEntity : class, IEntity
        {
            return context.Set<TEntity>().Any();
        }

        public virtual void EnsureDatabaseCreated()
        {
            context.Database.EnsureCreated();
        }

        public UserManager<User> GetUserManager()
        {
            return _userManager;
        }

        public SignInManager<User> GetSignInManager()
        {
            return _signInManager;
        }

        public RoleManager<Role> GetRoleManager()
        {
            return _roleManager;
        }
    }
}




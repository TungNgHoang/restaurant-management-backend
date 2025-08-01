﻿using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.DbContexts;
using RestaurantManagement.DataAccess.Models;

namespace RestaurantManagement.DataAccess.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly RestaurantDBContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public Repository(RestaurantDBContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        // Helper: Kiểm tra xem T có thuộc tính "IsDeleted" hay không.
        private bool HasSoftDelete() =>
            typeof(T).GetProperty("IsDeleted") != null;

        // Helper: Xây dựng biểu thức lambda: x => x.IsDeleted == false
        private Expression<Func<T, bool>> GetIsNotDeletedExpression()
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "IsDeleted");
            var constant = Expression.Constant(false);
            var body = Expression.Equal(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        // 1. Truy vấn và Lọc Dữ liệu
        public async Task<IEnumerable<T>> ActiveRecordsAsync()
        {
            IQueryable<T> query = _dbSet;
            if (HasSoftDelete())
                query = query.Where(GetIsNotDeletedExpression());
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> AsNoTrackingAsync()
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            if (HasSoftDelete())
                query = query.Where(GetIsNotDeletedExpression());
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> ActiveRecordsWithoutSoftDeleteAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> AsNoTrackingWithoutSoftDeleteAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        // 2. Tìm kiếm và Lấy Dữ liệu
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public async Task<IEnumerable<T>> FilterAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task<IEnumerable<T>> FindElementAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

        //public async Task<IEnumerable<object>> GetDistinctColumnAsync(string columnName)
        //{
        //    var list = await _dbSet.AsNoTracking().ToListAsync();
        //    return list
        //        .Select(x => x.GetType().GetProperty(columnName)?.GetValue(x))
        //        .Where(value => value != null)
        //        .Distinct();
        //}

        public async Task<IEnumerable<T>> GetListAsync()
        {
            IQueryable<T> query = _dbSet;
            if (HasSoftDelete())
                query = query.Where(GetIsNotDeletedExpression());
            return await query.ToListAsync();
        }

        public async Task<T> FindByIdAsync(Guid id) =>
            await _dbSet.FindAsync(id);

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<List<T>> FindListAsync(Expression<Func<T, bool>> predicate) =>
            //await _dbSet.FirstOrDefaultAsync(predicate);
            await _dbSet.Where(predicate).ToListAsync();

        // 3. Thêm, Cập nhật và Xóa Dữ liệu
        public async Task InsertAsync(T obj)
        {
            await _dbSet.AddAsync(obj);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(T obj)
        {
            _dbSet.Update(obj);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateManyAsync(IEnumerable<T> objs)
        {
            _dbSet.UpdateRange(objs);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T obj)
        {
            if (HasSoftDelete())
            {
                var property = typeof(T).GetProperty("IsDeleted");
                if (property != null)
                {
                    property.SetValue(obj, true);
                    await UpdateAsync(obj);
                }
            }
            else
            {
                _dbSet.Remove(obj);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var entity = await FindByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public async Task<T> CreateAsync(T obj)
        {
            await _dbSet.AddAsync(obj);
            await _dbContext.SaveChangesAsync();
            return obj;
        }

        // 4. Phân Trang
        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> predicate = null, int pageIndex = 0, int pageSize = 20)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null)
                query = query.Where(predicate);

            int totalCount = await query.CountAsync();
            var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        // 5. Xóa Nhiều Bản Ghi
        public async Task DeleteManyAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync();
            if (HasSoftDelete())
            {
                foreach (var entity in entities)
                {
                    var property = typeof(T).GetProperty("IsDeleted");
                    if (property != null)
                        property.SetValue(entity, true);
                }
                _dbSet.UpdateRange(entities);
            }
            else
            {
                _dbSet.RemoveRange(entities);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertManyAsync(IEnumerable<T> objs)
        {
            await _dbContext.Set<T>().AddRangeAsync(objs);
            await _dbContext.SaveChangesAsync();
        }
    }
}

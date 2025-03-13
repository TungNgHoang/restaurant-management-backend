using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // 1. Truy vấn và Lọc Dữ liệu
        Task<IEnumerable<T>> ActiveRecordsAsync();
        Task<IEnumerable<T>> AsNoTrackingAsync();
        Task<IEnumerable<T>> ActiveRecordsWithoutSoftDeleteAsync();
        Task<IEnumerable<T>> AsNoTrackingWithoutSoftDeleteAsync();

        // 2. Tìm kiếm và Lấy Dữ liệu
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FilterAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindElementAsync(Expression<Func<T, bool>> predicate);
        //Task<IEnumerable<object>> GetDistinctColumnAsync(string columnName);
        Task<IEnumerable<T>> GetListAsync();
        Task<T> FindByIdAsync(Guid id);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);

        // 3. Thêm, Cập nhật và Xóa Dữ liệu
        Task InsertAsync(T obj);
        Task UpdateAsync(T obj);
        Task UpdateManyAsync(IEnumerable<T> objs);
        Task DeleteAsync(T obj); // Soft delete nếu có cột IsDeleted
        Task DeleteByIdAsync(Guid id);
        Task<T> CreateAsync(T obj);

        // 4. Phân Trang
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> predicate = null, int pageIndex = 0, int pageSize = 20);

        // 5. Xóa Nhiều Bản Ghi
        Task DeleteManyAsync(Expression<Func<T, bool>> predicate);
    }
}
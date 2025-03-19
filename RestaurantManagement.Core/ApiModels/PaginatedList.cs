using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Exceptions;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Core.ApiModels
{
    public class PaginatedList<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }

        public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
        {
            if (pageNumber < 0)
            {
                throw new ErrorException(StatusCodeEnum.PageIndexInvalid);
            }

            if (pageSize < 0)
            {
                throw new ErrorException(StatusCodeEnum.PageSizeInvalid);
            }

            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ErrorException(StatusCodeEnum.PageIndexInvalid);
            }

            if (pageSize < 0)
            {
                throw new ErrorException(StatusCodeEnum.PageSizeInvalid);
            }

            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }

        public static PaginatedList<T> Create(List<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ErrorException(StatusCodeEnum.PageIndexInvalid);
            }

            if (pageSize < 0)
            {
                throw new ErrorException(StatusCodeEnum.PageSizeInvalid);
            }

            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}

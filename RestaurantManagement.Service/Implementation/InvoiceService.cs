using AutoMapper;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.DataAccess.Models;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.ReportsDto;
using RestaurantManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Implementation
{
    public class InvoiceService : BaseService, IInvoiceService
    {
        private readonly IRepository<TblTableInfo> _tableRepository;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblOrderInfo> _orderInfoRepository;
        private readonly IRepository<TblPayment> _paymentRepository;
        public InvoiceService(
            AppSettings appSettings, 
            IMapper mapper, 
            IRepository<TblTableInfo> tableRepository,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblOrderInfo> orderInfoRepository,
            IRepository<TblPayment> paymentRepository
            ) : base(appSettings, mapper)
        {
            _tableRepository = tableRepository;
            _reservationsRepository = reservationsRepository;
            _orderInfoRepository = orderInfoRepository;
            _paymentRepository = paymentRepository;

        }

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoiceAsync(InvoiceModels pagingModel)
        {
            var reservations = await _reservationsRepository.AsNoTrackingAsync();
            var tables = await _tableRepository.AsNoTrackingAsync();
            var order = await _orderInfoRepository.AsNoTrackingAsync();
            var payment = await _paymentRepository.AsNoTrackingAsync();

            var data = from o in order
                        join r in reservations on o.ResId equals r.ResId
                        join t in tables on r.TbiId equals t.TbiId
                        join p in payment on o.OrdId equals p.OrdId
                        select new
                        {
                            t.TbiTableNumber,
                            r.TempCustomerName,
                            r.TempCustomerPhone,
                            r.ResDate,
                            r.ResEndTime,
                            r.ResNumber,
                            o.TotalPrice,
                            p.PayMethod
                        };
            //Ánh xạ sang Dto
            var invoiceDto = data.Select(x => new InvoiceDto
            {
                TableNumber = x.TbiTableNumber,
                CustomerName = x.TempCustomerName,
                CustomerPhone = x.TempCustomerPhone,
                Date = x.ResDate.Date,
                TimeIn = x.ResDate.TimeOfDay,
                TimeOut = x.ResEndTime?.TimeOfDay ?? TimeSpan.Zero,
                People = x.ResNumber,
                TotalPrice = x.TotalPrice,
                PayMethod = x.PayMethod,
            }).ToList();

            var result = AdvancedFilter(invoiceDto.AsEnumerable(), pagingModel, nameof(InvoiceDto.TimeOut));
            return result;
        }
    }
}

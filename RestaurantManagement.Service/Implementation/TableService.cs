﻿namespace RestaurantManagement.Service.Implementation
{
    public class TableService : BaseService, ITableService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<TblTableInfo> _tableRepository;

        public TableService(AppSettings appSettings, IMapper mapper, IRepository<TblTableInfo> tableRepository) : base(appSettings, mapper)
        {
            _mapper = mapper;
            _tableRepository = tableRepository;
        }

        public async Task<IEnumerable<TableDto>> GetAllTableAsync(TableModels pagingModel)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(pagingModel);

            var data = await _tableRepository.AsNoTrackingAsync();

            var tableDtos = _mapper.Map<List<TableDto>>(data);
            var result = AdvancedFilter(tableDtos.AsEnumerable(), pagingModel, nameof(TableDto.TbiTableNumber));

            return result;
        }

        public Task<IEnumerable<TableDto>> GetAllTableAsync(TableModels pagingModel, string userEmail, string userRole)
        {
            throw new NotImplementedException();
        }

        private void ValidatePagingModel(TableModels pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageSizeInvalid);
        }
    }
}

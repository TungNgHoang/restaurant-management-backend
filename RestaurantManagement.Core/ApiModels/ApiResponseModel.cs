using RestaurantManagement.Core.Enums;
using RestaurantManagement.Core.Extensions;

namespace RestaurantManagement.Core.ApiModels
{
    public class ApiResponseModel
    {
        public ApiResponseModel()
        {
            StatusCode = StatusCodeEnum.Success.ToString();
        }
        public ApiResponseModel(object data)
        {
            StatusCode = StatusCodeEnum.Success.ToString();
            Data = data;
        }
        public ApiResponseModel(StatusCodeEnum statusCode)
        {
            StatusCode = statusCode.ToString().Split("_")[0];
        }
        public ApiResponseModel(StatusCodeEnum statusCode, object data)
        {
            StatusCode = statusCode.ToString().Split("_")[0];
            Data = data;
            StatusEnum = statusCode;
        }

        private StatusCodeEnum StatusEnum { get; set; }
        public string StatusCode { get; set; }
        public string Message
        {
            get
            {
                return StatusEnum.GetDescription();
            }
        }
        public object? Data { get; set; }
    }

    public class ApiResponseModel<T>
    {
        public ApiResponseModel(T data)
        {
            StatusCode = StatusCodeEnum.Success.ToString();
            Data = data;
        }

        private StatusCodeEnum StatusEnum { get; set; }
        public string StatusCode { get; set; }
        public string Message
        {
            get
            {
                return StatusEnum.GetDescription();
            }
        }
        public T Data { get; set; }
    }
}

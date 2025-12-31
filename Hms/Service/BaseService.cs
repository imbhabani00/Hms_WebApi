using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace Hms.Service
{
    public abstract class BaseService
    {
        protected readonly IMapper _mapper;
        protected readonly IConfiguration _configuration;

        protected BaseService(
            IMapper mapper,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _configuration = configuration;
        }

        protected async Task<TResponse> ExecuteWithLoggingAsync<TRequest, TResponse>(
            Func<TRequest, Task<TResponse>> operation,
            TRequest request,
            string operationName)
        {
            try
            {
                var result = await operation(request);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
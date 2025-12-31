using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Hms.Service
{
    #region Interface 
    public interface IEmailSenderService
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }
    #endregion

    public class EmailSenderService : IEmailSenderService
    {
        #region Properties
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Constructor
        public EmailSenderService(IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            ITempDataProvider tempDataProvider,
            IRazorViewEngine razorViewEngine)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
            _razorViewEngine = razorViewEngine;
        }
        #endregion

        #region RenderViewToStringAsync
        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View '{viewName}' not found");
                }

                var viewDictionary = new ViewDataDictionary<TModel>(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    Model = model
                };
                var viewContext = new ViewContext(
                   actionContext,
                   viewResult.View,
                   viewDictionary,
                   new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                   sw,
                   new HtmlHelperOptions()
               );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
        #endregion
    }
}

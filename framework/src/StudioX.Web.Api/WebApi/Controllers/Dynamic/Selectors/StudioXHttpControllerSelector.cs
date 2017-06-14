using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using StudioX.Collections.Extensions;
using StudioX.WebApi.Controllers.Dynamic.Builders;

namespace StudioX.WebApi.Controllers.Dynamic.Selectors
{
    /// <summary>
    /// This class is used to extend default controller selector to add dynamic api controller creation feature of StudioX.
    /// It checks if requested controller is a dynamic api controller, if it is,
    /// returns <see cref="HttpControllerDescriptor"/> to ASP.NET system.
    /// </summary>
    public class StudioXHttpControllerSelector : DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration configuration;
        private readonly DynamicApiControllerManager dynamicApiControllerManager;

        /// <summary>
        /// Creates a new <see cref="StudioXHttpControllerSelector"/> object.
        /// </summary>
        /// <param name="configuration">Http configuration</param>
        /// <param name="dynamicApiControllerManager"></param>
        public StudioXHttpControllerSelector(HttpConfiguration configuration, DynamicApiControllerManager dynamicApiControllerManager)
            : base(configuration)
        {
            this.configuration = configuration;
            this.dynamicApiControllerManager = dynamicApiControllerManager;
        }

        /// <summary>
        /// This method is called by Web API system to select the controller for this request.
        /// </summary>
        /// <param name="request">Request object</param>
        /// <returns>The controller to be used</returns>
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            //Get request and route data
            if (request == null)
            {
                return base.SelectController(null);
            }

            var routeData = request.GetRouteData();
            if (routeData == null)
            {
                return base.SelectController(request);
            }

            //Get serviceNameWithAction from route
            string serviceNameWithAction;
            if (!routeData.Values.TryGetValue("serviceNameWithAction", out serviceNameWithAction))
            {
                return base.SelectController(request);                
            }

            //Normalize serviceNameWithAction
            if (serviceNameWithAction.EndsWith("/"))
            {
                serviceNameWithAction = serviceNameWithAction.Substring(0, serviceNameWithAction.Length - 1);
                routeData.Values["serviceNameWithAction"] = serviceNameWithAction;
            }

            //Get the dynamic controller
            var hasActionName = false;
            var controllerInfo = dynamicApiControllerManager.FindOrNull(serviceNameWithAction);
            if (controllerInfo == null)
            {
                if (!DynamicApiServiceNameHelper.IsValidServiceNameWithAction(serviceNameWithAction))
                {
                    return base.SelectController(request);
                }
                
                var serviceName = DynamicApiServiceNameHelper.GetServiceNameInServiceNameWithAction(serviceNameWithAction);
                controllerInfo = dynamicApiControllerManager.FindOrNull(serviceName);
                if (controllerInfo == null)
                {
                    return base.SelectController(request);                    
                }

                hasActionName = true;
            }
            
            //Create the controller descriptor
            var controllerDescriptor = new DynamicHttpControllerDescriptor(configuration, controllerInfo);
            controllerDescriptor.Properties["__StudioXDynamicApiControllerInfo"] = controllerInfo;
            controllerDescriptor.Properties["__StudioXDynamicApiHasActionName"] = hasActionName;
            return controllerDescriptor;
        }
    }
}
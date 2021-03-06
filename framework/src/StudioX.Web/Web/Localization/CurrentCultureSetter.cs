using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using StudioX.Collections.Extensions;
using StudioX.Configuration;
using StudioX.Dependency;
using StudioX.Extensions;
using StudioX.Localization;
using StudioX.Runtime.Session;
using StudioX.Timing;
using StudioX.Web.Configuration;

namespace StudioX.Web.Localization
{
    public class CurrentCultureSetter : ICurrentCultureSetter, ITransientDependency
    {
        private readonly IStudioXWebLocalizationConfiguration webLocalizationConfiguration;
        private readonly ISettingManager settingManager;
        private readonly IStudioXSession studioXSession;

        public CurrentCultureSetter(
            IStudioXWebLocalizationConfiguration webLocalizationConfiguration,
            ISettingManager settingManager,
            IStudioXSession studioxSession)
        {
            this.webLocalizationConfiguration = webLocalizationConfiguration;
            this.settingManager = settingManager;
            studioXSession = studioxSession;
        }

        public virtual void SetCurrentCulture(HttpContext httpContext)
        {
            if (IsCultureSpecifiedInGlobalizationConfig())
            {
                return;
            }

            // 1: Query String
            var culture = GetCultureFromQueryString(httpContext);
            if (culture != null)
            {
                SetCurrentCulture(culture);
                return;
            }

            // 2: User preference
            culture = GetCultureFromUserSetting();
            if (culture != null)
            {
                SetCurrentCulture(culture);
                return;
            }

            // 3 & 4: Header / Cookie
            culture = GetCultureFromHeader(httpContext) ?? GetCultureFromCookie(httpContext);
            if (culture != null)
            {
                if (studioXSession.UserId.HasValue)
                {
                    SetCultureToUserSetting(studioXSession.ToUserIdentifier(), culture);
                }

                SetCurrentCulture(culture);
                return;
            }

            // 5 & 6: Default / Browser
            culture = GetDefaultCulture() ?? GetBrowserCulture(httpContext);
            if (culture != null)
            {
                SetCurrentCulture(culture);
                SetCultureToCookie(httpContext, culture);
            }
        }

        private void SetCultureToUserSetting(UserIdentifier user, string culture)
        {
            settingManager.ChangeSettingForUser(
                user,
                LocalizationSettingNames.DefaultLanguage,
                culture
            );
        }

        private string GetCultureFromUserSetting()
        {
            if (studioXSession.UserId == null)
            {
                return null;
            }

            var culture = settingManager.GetSettingValueForUser(
                LocalizationSettingNames.DefaultLanguage,
                studioXSession.TenantId,
                studioXSession.UserId.Value,
                fallbackToDefault: false
            );

            if (culture.IsNullOrEmpty() || !GlobalizationHelper.IsValidCultureCode(culture))
            {
                return null;
            }

            return culture;
        }

        protected virtual bool IsCultureSpecifiedInGlobalizationConfig()
        {
            var globalizationSection = WebConfigurationManager.GetSection("system.web/globalization") as GlobalizationSection;
            if (globalizationSection == null || globalizationSection.UICulture.IsNullOrEmpty())
            {
                return false;
            }

            return !string.Equals(globalizationSection.UICulture, "auto", StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual string GetCultureFromCookie(HttpContext httpContext)
        {
            var culture = httpContext.Request.Cookies[webLocalizationConfiguration.CookieName]?.Value;
            if (culture.IsNullOrEmpty() || !GlobalizationHelper.IsValidCultureCode(culture))
            {
                return null;
            }

            return culture;
        }

        protected virtual void SetCultureToCookie(HttpContext context, string culture)
        {
            context.Response.SetCookie(
                new HttpCookie(webLocalizationConfiguration.CookieName, culture)
                {
                    Expires = Clock.Now.AddYears(2),
                    Path = context.Request.ApplicationPath
                }
            );
        }

        protected virtual string GetDefaultCulture()
        {
            var culture = settingManager.GetSettingValue(LocalizationSettingNames.DefaultLanguage);
            if (culture.IsNullOrEmpty() || !GlobalizationHelper.IsValidCultureCode(culture))
            {
                return null;
            }

            return culture;
        }

        protected virtual string GetCultureFromHeader(HttpContext httpContext)
        {
            var culture = httpContext.Request.Headers[webLocalizationConfiguration.CookieName];
            if (culture.IsNullOrEmpty() || !GlobalizationHelper.IsValidCultureCode(culture))
            {
                return null;
            }

            return culture;
        }

        protected virtual string GetBrowserCulture(HttpContext httpContext)
        {
            if (httpContext.Request.UserLanguages.IsNullOrEmpty())
            {
                return null;
            }

            return httpContext.Request?.UserLanguages?.FirstOrDefault(GlobalizationHelper.IsValidCultureCode);
        }

        protected virtual string GetCultureFromQueryString(HttpContext httpContext)
        {
            var culture = httpContext.Request.QueryString[webLocalizationConfiguration.CookieName];
            if (culture.IsNullOrEmpty() || !GlobalizationHelper.IsValidCultureCode(culture))
            {
                return null;
            }

            return culture;
        }

        protected virtual void SetCurrentCulture(string language)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfoHelper.Get(language);
            Thread.CurrentThread.CurrentUICulture = CultureInfoHelper.Get(language);
        }
    }
}
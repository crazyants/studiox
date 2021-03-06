﻿using System.Threading.Tasks;
using StudioX.Dependency;
using StudioX.Runtime.Session;

namespace StudioX.Application.Features
{
    /// <summary>
    /// Default implementation for <see cref="IFeatureChecker"/>.
    /// </summary>
    public class FeatureChecker : IFeatureChecker, ITransientDependency
    {
        /// <summary>
        /// Reference to current session.
        /// </summary>
        public IStudioXSession StudioXSession { get; set; }

        /// <summary>
        /// Reference to the store used to get feature values.
        /// </summary>
        public IFeatureValueStore FeatureValueStore { get; set; }

        private readonly IFeatureManager featureManager;

        /// <summary>
        /// Creates a new <see cref="FeatureChecker"/> object.
        /// </summary>
        public FeatureChecker(IFeatureManager featureManager)
        {
            this.featureManager = featureManager;

            FeatureValueStore = NullFeatureValueStore.Instance;
            StudioXSession = NullStudioXSession.Instance;
        }

        /// <inheritdoc/>
        public Task<string> GetValueAsync(string name)
        {
            if (!StudioXSession.TenantId.HasValue)
            {
                throw new StudioXException("FeatureChecker can not get a feature value by name. TenantId is not set in the IStudioXSession!");
            }

            return GetValueAsync(StudioXSession.TenantId.Value, name);
        }

        /// <inheritdoc/>
        public async Task<string> GetValueAsync(int tenantId, string name)
        {
            var feature = featureManager.Get(name);

            var value = await FeatureValueStore.GetValueOrNullAsync(tenantId, feature);
            if (value == null)
            {
                return feature.DefaultValue;
            }

            return value;
        }
    }
}
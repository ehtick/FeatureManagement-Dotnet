﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.FeatureManagement.FeatureFilters;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Used to evaluate the enabled state of a feature and/or get the assigned variant of a feature, if any.
    /// </summary>
    public interface IVariantFeatureManager
    {
        /// <summary>
        /// Retrieves a list of feature names registered in the feature manager.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>An enumerator which provides asynchronous iteration over the feature names registered in the feature manager.</returns>
        IAsyncEnumerable<string> GetFeatureNamesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a given feature is enabled.
        /// </summary>
        /// <param name="feature">The name of the feature to check.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>True if the feature is enabled, otherwise false.</returns>
        ValueTask<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a given feature is enabled.
        /// </summary>
        /// <param name="feature">The name of the feature to check.</param>
        /// <param name="context">A context that provides information to evaluate whether a feature should be on or off.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>True if the feature is enabled, otherwise false.</returns>
        ValueTask<bool> IsEnabledAsync<TContext>(string feature, TContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the assigned variant for a specific feature.
        /// </summary>
        /// <param name="feature">The name of the feature to evaluate.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A variant assigned to the user based on the feature's configured allocation.</returns>
        ValueTask<Variant> GetVariantAsync(string feature, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the assigned variant for a specific feature.
        /// </summary>
        /// <param name="feature">The name of the feature to evaluate.</param>
        /// <param name="context">A context that provides information to evaluate which variant will be assigned to the user.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A variant assigned to the user based on the feature's configured allocation.</returns>
        ValueTask<Variant> GetVariantAsync(string feature, ITargetingContext context, CancellationToken cancellationToken = default);
    }
}

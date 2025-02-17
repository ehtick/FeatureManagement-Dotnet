﻿using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace Microsoft.FeatureManagement.Telemetry.ApplicationInsights
{
    /// <summary>
    /// Listens to <see cref="Activity"/> events from feature management and sends them to Application Insights.
    /// </summary>
    internal sealed class ApplicationInsightsEventPublisher : IDisposable
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ActivityListener _activityListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsEventPublisher"/> class.
        /// </summary>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        public ApplicationInsightsEventPublisher(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));

            _activityListener = new ActivityListener
            {
                ShouldListenTo = (activitySource) => activitySource.Name == "Microsoft.FeatureManagement",
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                ActivityStopped = (activity) =>
                {
                    ActivityEvent? evaluationEvent = activity.Events.FirstOrDefault((activityEvent) => activityEvent.Name == "FeatureFlag");

                    if (evaluationEvent.HasValue && evaluationEvent.Value.Tags.Any())
                    {
                        HandleFeatureFlagEvent(evaluationEvent.Value);
                    }
                }
            };

            ActivitySource.AddActivityListener(_activityListener);
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="ApplicationInsightsEventPublisher"/>.
        /// </summary>
        public void Dispose()
        {
            _activityListener.Dispose();
        }

        private void HandleFeatureFlagEvent(ActivityEvent activityEvent)
        {
            var properties = new Dictionary<string, string>();

            foreach (var tag in activityEvent.Tags)
            {
                // FeatureEvaluation event schema: https://github.com/microsoft/FeatureManagement/blob/main/Schema/FeatureEvaluationEvent/FeatureEvaluationEvent.v1.0.0.schema.json
                if (tag.Value is VariantAssignmentReason reason)
                {
                    switch (reason)
                    {
                        case VariantAssignmentReason.None:
                            properties[tag.Key] = "None";
                            break;
                        case VariantAssignmentReason.DefaultWhenDisabled:
                            properties[tag.Key] = "DefaultWhenDisabled";
                            break;
                        case VariantAssignmentReason.DefaultWhenEnabled:
                            properties[tag.Key] = "DefaultWhenEnabled";
                            break;
                        case VariantAssignmentReason.User:
                            properties[tag.Key] = "User";
                            break;
                        case VariantAssignmentReason.Group:
                            properties[tag.Key] = "Group";
                            break;
                        case VariantAssignmentReason.Percentile:
                            properties[tag.Key] = "Percentile";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(activityEvent), "The variant assignment reason is unrecognizable.");
                    }
                }
                else if (tag.Value is bool val)
                {
                    properties[tag.Key] = val ? "True" : "False";
                }
                else
                {
                    properties[tag.Key] = tag.Value?.ToString();
                }
            }

            _telemetryClient.TrackEvent("FeatureEvaluation", properties);
        }
    }
}

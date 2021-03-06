﻿namespace ApplicationInsightsDataROI
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.WindowsServer.Channel.Implementation;
    using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;

    /// <summary>
    /// This initializer applies aggressive sampling to dependency telemetry that runs faster than threshold value.
    /// </summary>
    internal class AggressivelySampleFastDependencies : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor next;
        private readonly AdaptiveSamplingTelemetryProcessor samplingProcessor;

        public AggressivelySampleFastDependencies(ITelemetryProcessor next)
        {
            this.next = next;
            this.samplingProcessor = new AdaptiveSamplingTelemetryProcessor(next);
        }

        /// <summary>
        /// Threshold defining whether request call considered fast or slow.
        /// </summary>
        public TimeSpan Threshold { get; set; } = TimeSpan.FromMilliseconds(300);

        public double InitialSamplingPercentage { get => this.samplingProcessor.InitialSamplingPercentage; set => this.samplingProcessor.InitialSamplingPercentage = value; }

        public double MinSamplingPercentage { get => this.samplingProcessor.MinSamplingPercentage; set => this.samplingProcessor.MinSamplingPercentage = value; }

        public double MaxSamplingPercentage { get => this.samplingProcessor.MaxSamplingPercentage; set => this.samplingProcessor.MaxSamplingPercentage = value; }

        public void Process(ITelemetry item)
        {
            // check the telemetry type and duration
            if (item is DependencyTelemetry)
            {
                var d = item as DependencyTelemetry;
                if (d.Duration < this.Threshold)
                {
                    // let sampling processor decide what to do
                    // with this fast incoming request
                    this.samplingProcessor.Process(item);
                    return;
                }
            }

            // in all other cases simply call next
            this.next.Process(item);
        }
    }
}

//
//    Copyright (C) Microsoft.  All rights reserved.
//

using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// Unregisters from an event on an object.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Unregister, "Event", SupportsShouldProcess = true, DefaultParameterSetName = "BySource", HelpUri = "http://go.microsoft.com/fwlink/?LinkID=135269")]
    public class UnregisterEventCommand : PSCmdlet
    {
        #region parameters

        /// <summary>
        /// An identifier for this event subscription
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "BySource")]
        public string SourceIdentifier
        {
            get
            {
                return _sourceIdentifier;
            }
            set
            {
                _sourceIdentifier = value;

                if (value != null)
                {
                    _matchPattern = WildcardPattern.Get(value, WildcardOptions.IgnoreCase);
                }
            }
        }
        private string _sourceIdentifier = null;

        /// <summary>
        /// An identifier for this event subscription
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "ById")]
        public int SubscriptionId
        {
            get
            {
                return _subscriptionId;
            }
            set
            {
                _subscriptionId = value;
            }
        }
        private int _subscriptionId = -1;

        /// <summary>
        /// Flag that determines if we should include subscriptions used to support
        /// other subscriptions
        /// </summary>
        [Parameter()]
        public SwitchParameter Force
        {
            get
            {
                return _force;
            }
            set
            {
                _force = value;
            }
        }
        private SwitchParameter _force;

        #endregion parameters

        private WildcardPattern _matchPattern;
        private bool _foundMatch = false;

        /// <summary>
        /// Unsubscribe from the event
        /// </summary>
        protected override void ProcessRecord()
        {
            // Go through all the received events and write them to the output
            // pipeline
            foreach (PSEventSubscriber subscriber in Events.Subscribers)
            {
                // If the event identifier matches, remove the subscription
                if (
                    ((_sourceIdentifier != null) && _matchPattern.IsMatch(subscriber.SourceIdentifier)) ||
                    ((SubscriptionId >= 0) && (subscriber.SubscriptionId == SubscriptionId))
                   )
                {
                    // If this is a support event but they aren't explicitly
                    // looking for them, continue.
                    if (subscriber.SupportEvent && (!Force))
                    {
                        continue;
                    }

                    _foundMatch = true;

                    if (ShouldProcess(
                        String.Format(
                            System.Globalization.CultureInfo.CurrentCulture,
                            EventingStrings.EventSubscription,
                            subscriber.SourceIdentifier),
                        EventingStrings.Unsubscribe))
                    {
                        Events.UnsubscribeEvent(subscriber);
                    }
                }
            }

            // Generate an error if we couldn't find the subscription identifier,
            // and no globbing was done.
            if ((_sourceIdentifier != null) &&
               (!WildcardPattern.ContainsWildcardCharacters(_sourceIdentifier)) &&
               (!_foundMatch))
            {
                ErrorRecord errorRecord = new ErrorRecord(
                    new ArgumentException(
                        String.Format(
                            System.Globalization.CultureInfo.CurrentCulture,
                            EventingStrings.EventSubscriptionNotFound, _sourceIdentifier)),
                    "INVALID_SOURCE_IDENTIFIER",
                    ErrorCategory.InvalidArgument,
                    null);

                WriteError(errorRecord);
            }
            else if ((SubscriptionId >= 0) &&
               (!_foundMatch))
            {
                ErrorRecord errorRecord = new ErrorRecord(
                    new ArgumentException(
                        String.Format(
                            System.Globalization.CultureInfo.CurrentCulture,
                            EventingStrings.EventSubscriptionNotFound, SubscriptionId)),
                    "INVALID_SUBSCRIPTION_IDENTIFIER",
                    ErrorCategory.InvalidArgument,
                    null);

                WriteError(errorRecord);
            }
        }
    }
}
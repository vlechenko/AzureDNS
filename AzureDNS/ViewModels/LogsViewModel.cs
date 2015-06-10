﻿using System.Collections.ObjectModel;
using System.Windows;
using AzureDNS.Common;
using AzureDNS.Events;
using AzureDNS.Views;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

namespace AzureDNS.ViewModels
{
    public class LogsViewModel: BaseViewModel
    {
        private readonly ILogView view;
        private readonly IUnityContainer container;
        private readonly ObservableCollection<LogMessage> messages = new ObservableCollection<LogMessage>();

        public LogsViewModel(ILogView view, IUnityContainer container)
        {
            this.view = view;
            this.container = container;

            view.Loaded += OnLoaded;
            view.Unloaded += OnUnloaded;
        }

        public ObservableCollection<LogMessage> Messages
        {
            get { return messages; }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var aggregator = container.Resolve<IEventAggregator>();
            aggregator.GetEvent<LogEvent>().Unsubscribe(OnLogEvent);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var aggregator = container.Resolve<IEventAggregator>();
            aggregator.GetEvent<LogEvent>().Subscribe(OnLogEvent, ThreadOption.UIThread);

            container.RegisterInstance(new LogViewer());
        }

        private void OnLogEvent(LogMessage item)
        {
            Messages.Add(item);
            view.FocusItem(item);
        }
    }
}

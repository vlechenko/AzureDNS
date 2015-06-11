﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AzureDNS.Common;
using AzureDNS.Core;
using AzureDNS.Events;
using AzureDNS.Views;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

namespace AzureDNS.ViewModels
{
    public class DnsZonesViewModel: BaseViewModel
    {
        private readonly IDnsZonesView view;
        private readonly IUnityContainer container;
        private readonly ILoggerFacade logger;
        private readonly IEventAggregator eventAggregator;
        private readonly ObservableCollection<DnsZoneViewModel> zones = new ObservableCollection<DnsZoneViewModel>();
        private DnsZoneViewModel currentZone;

        private bool isEnabled = true;
        private bool loading = false;
        private DelegateCommand addZoneCommand;
        private string currentSubscription;

        public DnsZonesViewModel(IDnsZonesView view, IUnityContainer container)
        {
            this.view = view;
            this.container = container;
            eventAggregator = container.Resolve<IEventAggregator>();
            logger = container.Resolve<ILoggerFacade>();

            view.Loaded += OnLoaded;
            view.Unloaded += OnUnloaded;

            AddZoneCommand = new DelegateCommand(OnAddZoneClick, () => !string.IsNullOrEmpty(currentSubscription));
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool Loading
        {
            get { return loading; }
            set
            {
                loading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DnsZoneViewModel> Zones
        {
            get { return zones; }
        }

        public DnsZoneViewModel CurrentZone
        {
            get { return currentZone; }
            set
            {
                currentZone = value;
                OnPropertyChanged();
                LoadDnsRecordsAsync();
            }
        }

        public DelegateCommand AddZoneCommand
        {
            get { return addZoneCommand; }
            set
            {
                addZoneCommand = value;
                OnPropertyChanged();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            eventAggregator.GetEvent<AzureSubscriptionChangedEvent>().Subscribe(OnAzureSubscriptionChange, ThreadOption.UIThread);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            eventAggregator.GetEvent<AzureSubscriptionChangedEvent>().Unsubscribe(OnAzureSubscriptionChange);
        }

        private async void OnAzureSubscriptionChange(string subscription)
        {
            currentSubscription = subscription;
            AddZoneCommand.RaiseCanExecuteChanged();
            await LoadDnsZonesAsync();
        }

        private void LoadDnsRecordsAsync()
        {
            var aggregator = container.Resolve<IEventAggregator>();
            aggregator.GetEvent<DnsZoneChangedEvent>().Publish(CurrentZone);
        }

        private async Task LoadDnsZonesAsync()
        {
            logger.Log("Getting AzureDnsZones...", Category.Info, Priority.Low);

            try
            {
                Loading = true;
                IsEnabled = false;

                var ps = container.Resolve<AzurePowerShell>();
                var items = await ps.GetAzureDnsZoneAsync();
                Zones.Clear();
                foreach (var item in items)
                {
                    Zones.Add(item);
                }
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message, Category.Exception, Priority.High);
            }
            finally
            {
                Loading = false;
                IsEnabled = true;
            }
        }
        private void OnAddZoneClick()
        {
        }
    }
}
using System.Windows;
using AzureDNS.ViewModels;

namespace AzureDNS.Views.Interfaces
{
    public interface IDnsRecordEditor: IBaseView
    {
        bool? ShowDialog();
        Window Owner { get; set; }
        void Complete();

        bool EditMode { get; set; }
        DnsZoneViewModel DnsZone { get; set; }
        DnsRecordViewModel DnsRecord { get; set; }
    }
}
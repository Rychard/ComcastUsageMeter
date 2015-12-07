using System;
using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "account")]
    public class Account
    {
        [XmlAttribute(AttributeName = "ID")]
        public String AccountID { get; set; }

        [XmlElement(ElementName = "additional_billable_blocks_used")]
        public String AdditionalBillableBlocksUsed { get; set; }

        [XmlElement(ElementName = "additional_billable_cost_per_block")]
        public String AdditionalBillableCostPerBlock { get; set; }

        [XmlElement(ElementName = "additional_billable_grace_amount_exceeded")]
        public String AdditionalBillableGraceAmountExceeded { get; set; }

        [XmlElement(ElementName = "additional_billable_included")]
        public String AdditionalBillableIncluded { get; set; }

        [XmlElement(ElementName = "additional_billable_percentUsed")]
        public String AdditionalBillablePercentUsed { get; set; }

        [XmlElement(ElementName = "additional_billable_remaining")]
        public String AdditionalBillableRemaining { get; set; }

        [XmlElement(ElementName = "additional_billable_units_per_block")]
        public String AdditionalBillableUnitsPerBlock { get; set; }

        [XmlElement(ElementName = "additional_billable_used")]
        public String AdditionalBillableUsed { get; set; }

        [XmlElement(ElementName = "counter_end")]
        public DateTime CounterEnd { get; set; }

        [XmlElement(ElementName = "counter_start")]
        public DateTime CounterStart { get; set; }

        [XmlElement(ElementName = "home_device_details")]
        public HomeDeviceDetails HomeDeviceDetails { get; set; }
        
        [XmlElement(ElementName = "minutes_since_last_update")]
        public String MinutesSinceLastUpdate { get; set; }

        [XmlElement(ElementName = "billable_overage")]
        public String OverageBillable { get; set; }

        [XmlElement(ElementName = "non_billable_overage")]
        public String OverageNonBillable { get; set; }

        [XmlElement(ElementName = "policy_display_name")]
        public String PolicyDisplayName { get; set; }

        [XmlElement(ElementName = "policy_name")]
        public String PolicyName { get; set; }

        [XmlElement(ElementName = "usage_allowable")]
        public Double UsageAllowable { get; set; }

        [XmlElement(ElementName = "home_usage")]
        public String UsageHome { get; set; }

        [XmlElement(ElementName = "overage_usage")]
        public String UsageOverage { get; set; }

        [XmlElement(ElementName = "usage_percent")]
        public Double UsagePercent { get; set; }

        [XmlElement(ElementName = "usage_remaining")]
        public Double UsageRemaining { get; set; }

        [XmlElement(ElementName = "usage_total")]
        public Double UsageTotal { get; set; }

        [XmlElement(ElementName = "usage_uom")]
        public String UsageUnitOfMeasurement { get; set; }

        [XmlElement(ElementName = "wifi_usage")]
        public String UsageWiFi { get; set; }
    }
}
using System;
using System.Xml.Serialization;

namespace ComcastUsageMeter.Shared.Responses
{
    [XmlRoot(ElementName = "device")]
    public class Device
    {
        [XmlAttribute(AttributeName = "mac")]
        public String MacAddress { get; set; }

        [XmlElement(ElementName = "additional_billable_blocks_used")]
        public int AdditionalBillableBlocksUsed { get; set; }

        [XmlElement(ElementName = "additional_billable_cost_per_block")]
        public Decimal AdditionalBillableCostPerBlock { get; set; }

        [XmlElement(ElementName = "additional_billable_grace_amount_exceeded")]
        public Boolean AdditionalBillableGraceAmountExceeded { get; set; }

        [XmlElement(ElementName = "additional_billable_included")]
        public int AdditionalBillableIncluded { get; set; }

        [XmlElement(ElementName = "additional_billable_percentUsed")]
        public int AdditionalBillablePercentUsed { get; set; }

        [XmlElement(ElementName = "additional_billable_remaining")]
        public int AdditionalBillableRemaining { get; set; }

        [XmlElement(ElementName = "additional_billable_units_per_block")]
        public int AdditionalBillableUnitsPerBlock { get; set; }

        [XmlElement(ElementName = "additional_billable_used")]
        public int AdditionalBillableUsed { get; set; }

        [XmlElement(ElementName = "context_code")]
        public String ContextCode { get; set; }

        [XmlElement(ElementName = "counter_end")]
        public DateTime CounterEnd { get; set; }

        [XmlElement(ElementName = "counter_start")]
        public DateTime CounterStart { get; set; }

        [XmlElement(ElementName = "minutes_since_last_update")]
        public int MinutesSinceLastUpdate { get; set; }

        [XmlElement(ElementName = "usage_allowable")]
        public int UsageAllowable { get; set; }

        [XmlElement(ElementName = "usage_percent")]
        public int UsagePercent { get; set; }

        [XmlElement(ElementName = "usage_remaining")]
        public int UsageRemaining { get; set; }

        [XmlElement(ElementName = "usage_total")]
        public int UsageTotal { get; set; }

        [XmlElement(ElementName = "usage_uom")]
        public String UsageUnitOfMeasurement { get; set; }
    }
}

namespace EquipmentAccounting.Models
{
    public class DropdownData
    {
        public List<DropdownItem> ConsumableTypes { get; set; } = new();
        public List<DropdownItem> Users { get; set; } = new();
        public List<DropdownItem> Consumables { get; set; } = new();
        public List<DropdownItem> ConsumableCharacteristics { get; set; } = new();
        public List<DropdownItem> Equipment { get; set; } = new();
        public List<DropdownItem> Rooms { get; set; } = new();
        public List<DropdownItem> Software { get; set; } = new();
        public List<DropdownItem> Inventories { get; set; } = new();
        public List<DropdownItem> EquipmentTypes { get; set; } = new();
        public List<DropdownItem> Developers { get; set; } = new();
        public List<DropdownItem> Statuses { get; set; } = new();
        public List<DropdownItem> Directions { get; set; } = new();
        public List<DropdownItem> Models { get; set; } = new();
    }
    public class DropdownItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}
namespace WarehouseAPI.Models 
{
    public enum UserRole
    {
        Manager = 1,
        Sales = 2,
        Storekeeper = 3
    }

    public enum OrderStatus
    {
        Pending = 1,
        Prepared = 2,
        Cancelled = 3
    }

    public enum AuditStatus
    {
        Pending = 1,
        Approved = 2
    }
}
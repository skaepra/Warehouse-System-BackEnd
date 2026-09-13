namespace WarehouseAPI.Models 
{
    public enum OrderStatus
    {
        Pending = 1,   // قيد الانتظار
        Prepared = 2,  // تم التجهيز بداخل المستودع 
        Delivered = 3, // تم التسليم للزبون 
        Cancelled = 4  // ملغى
    }
}
namespace WarehouseAPI.Models 
{
    public enum OrderStatus
    {
        Pending ,   // قيد الانتظار
        Prepared ,  // تم التجهيز بداخل المستودع 
        Delivered , // تم التسليم للزبون 
        Cancelled   // ملغى
    }
}
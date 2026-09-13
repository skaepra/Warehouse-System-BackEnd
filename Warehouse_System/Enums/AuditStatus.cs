namespace Online_Store_Backend.Models
{
    public enum AuditStatus
    {
        Pending = 0,   // قيد الانتظار
        Approved = 1,  // تم الاعتماد وتعديل المخزون
        Rejected = 2   // مقتول/مرفوض
    }
}

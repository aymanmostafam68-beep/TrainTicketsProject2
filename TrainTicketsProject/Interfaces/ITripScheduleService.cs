using Microsoft.AspNetCore.Mvc.ModelBinding;
using TrainTicketsProject.ViewModels;

namespace TrainTicketsProject.Interfaces
{
    public interface ITripScheduleService
    {
        // جلب البيانات لصفحة العرض الرئيسية مع الترقيم (Pagination)
        Task<TripSchedulesVM> GetIndexVmAsync(int page);

        // تجهيز موديل فارغ لصفحة الإنشاء
        Task<TripScheduleCreateVM> GetCreateVmAsync();

        // تنفيذ عملية إنشاء جدول رحلات جديد
        Task<(bool Success, string? ErrorMessage)> CreateAsync(TripScheduleCreateVM model, string userId, ModelStateDictionary modelState);

        // جلب البيانات لصفحة التعديل بناءً على المعرف
        Task<TripScheduleCreateVM?> GetEditVmAsync(int id);

        // تنفيذ عملية تحديث جدول رحلات موجود
        Task<(bool Success, string? Error)> EditAsync(TripScheduleCreateVM model, string userId, ModelStateDictionary modelState);

        // حذف جدول رحلات والرحلات المرتبطة به
        Task<bool> DeleteAsync(int id);

        // ملء القوائم المنسدلة (Routes, Trains, TimeSlots) في الموديل
         Task PopulateVmLists(TripScheduleCreateVM vm);

        // حساب وتنسيق حقل TimeSlot النصي
         void ApplyComputedFields(TripSchedule schedule);

        // مزامنة وتوليد الرحلات الفردية بناءً على الجدول (Start/End Date)
         Task SyncTripsAsync(TripSchedule schedule);

        // حذف جميع الرحلات المرتبطة بجدول معين
         Task DeleteTripsAsync(int tripScheduleId);

        // تنظيف ModelState من الحقول التي لا تحتاج للتحقق
         void RemoveModelState(ModelStateDictionary ModelState);

        // نسخة مخصصة لملء القوائم عند حدوث خطأ في الإنشاء/التعديل
         Task PopulateCreateVmLists(TripScheduleCreateVM vm);

        // جلب بيانات القطار المختار مع المسار المرتبط به
         Task<Train?> GetSelectedTrainAsync(int trainId);

        // ربط المسار (Route) بالعربة المختارة في الموديل
         void ApplyAssignedRoute(TripScheduleCreateVM model, Train selectedTrain);

        // محاولة تطبيق الأوقات المختارة للرحلة الذهاب

        // محاولة تطبيق الأوقات المختارة لرحلة العودة
        public bool TryApplyReturnTimes(
             TripScheduleCreateVM model,
             out TimeOnly departureTime,
             out TimeOnly arrivalTime,
 ModelStateDictionary modelState);
        // تحويل النصوص (Strings) إلى توقيتات (TimeOnly) والتحقق من صحتها
         bool TryParseTimes(string? selectedDepartureTime, string? selectedArrivalTime, bool isNextDay, string departureKey, string arrivalKey, out TimeOnly departureTime, out TimeOnly arrivalTime, ModelStateDictionary modelstate);

        // التحقق من صحة نطاق التاريخ (تاريخ النهاية بعد البداية)
         bool ValidateScheduleRange(TripSchedule schedule, string endDateKey, string everyKey, ModelStateDictionary modelstate);

        // حساب المدة الزمنية بين الإقلاع والوصول
         TimeSpan CalculateDuration(TimeOnly departureTime, TimeOnly arrivalTime, bool isNextDay);

        // التحقق من عدم تكرار جدول بنفس القطار والوقت والمسار
         Task<bool> ScheduleExistsAsync(int routeId, int trainId, TimeOnly departureTime, int? ignoreScheduleId);

        // تجهيز كائن جدول رحلة العودة
         Task<TripSchedule?> PrepareReturnScheduleAsync(TripScheduleCreateVM model, Train selectedTrain, string userId, int? existingReturnScheduleId, ModelStateDictionary modelstate);

        // حفظ أو تحديث جدول رحلة العودة في قاعدة البيانات
         Task<TripSchedule?> SavePreparedReturnScheduleAsync(TripSchedule returnSchedule, ModelStateDictionary modelstate);

        // حذف رحلة العودة المرتبطة عند إلغاء تفعيل خيار العودة
         Task DeleteLinkedReturnScheduleAsync(int returnScheduleId);
        bool TryApplySelectedTimes(TripScheduleCreateVM model, ModelStateDictionary modestate);
    }
}
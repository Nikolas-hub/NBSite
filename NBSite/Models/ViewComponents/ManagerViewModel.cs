namespace NBSite.Models.ViewComponents
{
    public class ManagerViewModel
    {
        public string? FullName { get; set; }      // ФИО из профиля или имя пользователя
        public string? Phone { get; set; }          // Телефон из профиля
        public string? Email { get; set; }          // Email из auth_user
        public string Position { get; set; } = "Менеджер"; // Можно добавить поле должности, если есть
    }
}

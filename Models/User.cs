using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Логин обязателен для заполнения")]
        [Column("username")]
        [StringLength(50, ErrorMessage = "Логин не может превышать 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Логин может содержать только буквы, цифры и символ подчеркивания")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Роль обязательна для заполнения")]
        [Column("role")]
        [StringLength(50)]
        [RegularExpression(@"^(employee|teacher|administrator)$", ErrorMessage = "Роль должна быть: employee, teacher или administrator")]
        public string Role { get; set; } = "employee";

        [Column("token")]
        [StringLength(100)]
        public string? Token { get; set; }

        [Column("email")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [Column("last_name")]
        [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [Column("first_name")]
        [StringLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
        public string FirstName { get; set; }

        [Column("middle_name")]
        [StringLength(50, ErrorMessage = "Отчество не может превышать 50 символов")]
        public string? MiddleName { get; set; }

        [Column("phone")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? Phone { get; set; }

        [Column("address")]
        [StringLength(255, ErrorMessage = "Адрес не может превышать 255 символов")]
        public string? Address { get; set; }
    }
}

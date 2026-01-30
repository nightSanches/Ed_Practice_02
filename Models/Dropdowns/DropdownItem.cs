namespace API.Models.Dropdowns
{
    /// <summary>
    /// Модель элемента выпадающего списка
    /// </summary>
    public class DropdownItem
    {
        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текст для отображения
        /// </summary>
        public string DisplayText { get; set; }
    }
}

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using EquipmentAccounting.Models;
using EquipmentAccounting.Pages.Equipment;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfStyle = System.Windows.Style;

namespace EquipmentAccounting.Pages.EquipmentResponsibleHistory
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        Models.EquipmentResponsibleHistory equipment;
        public Item(Models.EquipmentResponsibleHistory equipment, int count)
        {
            InitializeComponent();
            this.equipment = equipment;

            txtId.Text = count.ToString();
            txtName.Text = UserSession.DropdownData.Users.FirstOrDefault(item => item.Id == equipment.ResponsibleUserId)?.DisplayText;
            txtCom.Text = equipment.Comment;
            txtAssignedBy.Text = UserSession.DropdownData.Users.FirstOrDefault(item => item.Id == equipment.AssignedByUserId)?.DisplayText;
            txtAssignedTime.Text = equipment.AssignedAt.ToString();
        }
        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            Main.init.DeleteEquipment(equipment.Id);
            Main.init.parent.Children.Remove(this);
        }

        private void OnEnter(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#e7ebee");
            brdrCom.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
        }

        private void OnLeave(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
            brdrCom.Background = (Brush)new BrushConverter().ConvertFrom("#f2f2f2");
        }

        private void btnDocument_Click(object sender, RoutedEventArgs e)
        {
            BtnGenerate_Click();
        }
        private void BtnGenerate_Click()
        {
            try
            {
                string date;
                if(equipment.AssignedAt != null)
                {
                    DateTime temp = Convert.ToDateTime(equipment.AssignedAt);
                    date = temp.ToString("dd.MM.yyyy");
                }
                else
                {
                    date = DateTime.Now.Date.ToString("dd.MM.yyyy");
                }

                    // Получаем значения из полей ввода
                    string employeeName = "_________________________";
                if(UserSession.DropdownData.Users.FirstOrDefault(item => item.Id == equipment.ResponsibleUserId)?.DisplayText != null)
                {
                    employeeName = UserSession.DropdownData.Users.FirstOrDefault(item => item.Id == equipment.ResponsibleUserId)?.DisplayText;
                }

                string equipmentType = "_________________________";
                if (UserSession.DropdownData.Models.FirstOrDefault(item => item.Id == Main.init.equipment.ModelId)?.DisplayText != null)
                    equipmentType = UserSession.DropdownData.Models.FirstOrDefault(item => item.Id == Main.init.equipment.ModelId)?.DisplayText;

                string equipmentName = Main.init.equipment.Name;

                int inventoryNumber = Main.init.equipment.InventoryNumber;

                decimal cost = 0;
                if (Main.init.equipment.Cost.Value != null)
                    cost = Main.init.equipment.Cost.Value;

                // Открываем диалог выбора пути сохранения
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Word Documents|*.docx",
                    DefaultExt = "docx",
                    FileName = $"Акт_приема_передачи_{DateTime.Now:dd-MM-yyyy}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    GenerateWordDocument(saveFileDialog.FileName, employeeName,
                        equipmentType, equipmentName, inventoryNumber, cost);

                    MessageBox.Show("Документ успешно создан!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Проверьте правильность ввода числовых полей",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании документа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateWordDocument(string filePath, string employeeName, 
            string equipmentType, string equipmentName, int inventoryNumber, decimal cost)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                // Создаем основную часть документа
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Добавляем настройки документа для отключения шрифтов темы
                DocumentSettingsPart settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
                Settings settings = new Settings();
                
                settingsPart.Settings = settings;
                settingsPart.Settings.Save();

                // Создаем и применяем стили
                AddStylesWithTimesNewRoman(mainPart);

                // Форматируем стоимость с двумя знаками после запятой
                string formattedCost = cost.ToString("F2");

                // Создаем параграфы документа
                
                // 1. АКТ (по центру)
                body.AppendChild(CreateParagraphWithTimesNewRoman("АКТ", JustificationValues.Center, true, 28));
                
                // 2. приема-передачи оборудования (по центру)
                body.AppendChild(CreateParagraphWithTimesNewRoman("приема-передачи оборудования", JustificationValues.Center, true, 28));
                
                // 3. Пустая строка
                body.AppendChild(CreateParagraphWithTimesNewRoman("", JustificationValues.Left, false, 24));
                
                // 4. Строка с городом и датой
                Paragraph cityDateParagraph = new Paragraph();
                
                // Свойства параграфа с табуляцией
                ParagraphProperties paragraphProperties = new ParagraphProperties();
                Tabs tabs = new Tabs();
                tabs.Append(new TabStop() { Val = TabStopValues.Right, Position = 9000 });
                paragraphProperties.Append(tabs);
                
                // Добавляем свойства параграфа с шрифтом
                ParagraphMarkRunProperties paragraphMarkProps = new ParagraphMarkRunProperties();
                paragraphMarkProps.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                paragraphMarkProps.Append(new FontSize() { Val = "24" });
                paragraphProperties.Append(paragraphMarkProps);
                
                cityDateParagraph.Append(paragraphProperties);
                
                // г. Пермь (слева)
                Run cityRun = new Run();
                cityRun.Append(new Text("г. Пермь"));
                cityRun.RunProperties = new RunProperties();
                cityRun.RunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                cityRun.RunProperties.Append(new FontSize() { Val = "24" });
                cityDateParagraph.Append(cityRun);
                
                // Табуляция и дата (справа)
                Run dateRun = new Run();
                dateRun.Append(new TabChar());
                dateRun.Append(new Text(DateTime.Now.Date.ToString("dd.MM.yyyy")));
                dateRun.RunProperties = new RunProperties();
                dateRun.RunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                dateRun.RunProperties.Append(new FontSize() { Val = "24" });
                
                cityDateParagraph.Append(dateRun);
                body.Append(cityDateParagraph);
                
                // 5. Пустая строка
                body.AppendChild(CreateParagraphWithTimesNewRoman("", JustificationValues.Left, false, 24));
                
                // 6. Основной текст
                string mainText = $"КГАПОУ Пермский Авиационный техникум им. А.Д. Швецова в целях обеспечением необходимым оборудованием для исполнения должностных обязанностей сотрудник {employeeName} принимает от учебного учреждения следующее оборудование:";
                Paragraph mainParagraph = CreateParagraphWithTimesNewRoman(mainText, JustificationValues.Both, false, 24);
                mainParagraph.ParagraphProperties.Append(new Indentation() { FirstLine = "720" });
                body.Append(mainParagraph);
                
                // 7. Пустая строка
                body.AppendChild(CreateParagraphWithTimesNewRoman("", JustificationValues.Left, false, 24));
                
                // 8. Строка с оборудованием
                string equipmentText = $"{equipmentName} {equipmentType}, инвентарный номер {inventoryNumber}, стоимостью {formattedCost} руб.";
                Paragraph equipmentParagraph = CreateParagraphWithTimesNewRoman(equipmentText, JustificationValues.Both, false, 24);
                equipmentParagraph.ParagraphProperties.Append(new Indentation() { FirstLine = "720" }); // Отступ первой строки 1.25 см
                body.Append(equipmentParagraph);
                
                // 9. Пустая строка
                body.AppendChild(CreateParagraphWithTimesNewRoman("", JustificationValues.Left, false, 24));
                
                // 10. Строка с подписями
                Paragraph signaturesParagraph = new Paragraph();
                
                // Свойства параграфа с табуляцией
                ParagraphProperties sigParagraphProperties = new ParagraphProperties();
                Tabs sigTabs = new Tabs();
                sigTabs.Append(new TabStop() { Val = TabStopValues.Right, Position = 9000 });
                sigParagraphProperties.Append(sigTabs);
                
                // Добавляем свойства параграфа с шрифтом
                ParagraphMarkRunProperties sigParagraphMarkProps = new ParagraphMarkRunProperties();
                sigParagraphMarkProps.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                sigParagraphMarkProps.Append(new FontSize() { Val = "24" });
                sigParagraphProperties.Append(sigParagraphMarkProps);
                
                signaturesParagraph.Append(sigParagraphProperties);
                
                // И.И. Иванов (слева)
                Run nameRun = new Run();
                nameRun.Append(new Text(employeeName));
                nameRun.RunProperties = new RunProperties();
                nameRun.RunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                nameRun.RunProperties.Append(new FontSize() { Val = "24" });
                signaturesParagraph.Append(nameRun);
                
                // Табуляция и подпись (справа)
                Run signatureRun = new Run();
                signatureRun.Append(new TabChar());
                signatureRun.Append(new Text("________________ (_____________)"));
                signatureRun.RunProperties = new RunProperties();
                signatureRun.RunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                signatureRun.RunProperties.Append(new FontSize() { Val = "24" });
                
                signaturesParagraph.Append(signatureRun);
                body.Append(signaturesParagraph);

                // Сохраняем документ
                mainPart.Document.Save();
            }
        }

        private Paragraph CreateParagraphWithTimesNewRoman(string text, JustificationValues alignment, bool isBold, int fontSize)
        {
            Paragraph paragraph = new Paragraph();
            
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            paragraphProperties.Justification = new Justification() { Val = alignment };
            
            paragraphProperties.SpacingBetweenLines = new SpacingBetweenLines() { Line = "360", LineRule = LineSpacingRuleValues.Auto };
            
            ParagraphMarkRunProperties paragraphMarkRunProperties = new ParagraphMarkRunProperties();
            paragraphMarkRunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
            paragraphMarkRunProperties.Append(new FontSize() { Val = fontSize.ToString() });
            
            if (isBold)
            {
                paragraphMarkRunProperties.Append(new Bold());
            }
            
            paragraphProperties.Append(paragraphMarkRunProperties);
            paragraph.Append(paragraphProperties);
            
            if (!string.IsNullOrEmpty(text))
            {
                Run run = new Run();
                RunProperties runProperties = new RunProperties();
                
                if (isBold)
                {
                    runProperties.Append(new Bold());
                }
                
                runProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
                runProperties.Append(new FontSize() { Val = fontSize.ToString() });
                
                run.Append(new Text(text));
                run.RunProperties = runProperties;
                
                paragraph.Append(run);
            }
            
            return paragraph;
        }

        private void AddStylesWithTimesNewRoman(MainDocumentPart mainPart)
        {
            StyleDefinitionsPart stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
            Styles styles = new Styles();
            
            DocumentFormat.OpenXml.Wordprocessing.Style defaultStyle = new DocumentFormat.OpenXml.Wordprocessing.Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            
            StyleParagraphProperties styleParagraphProperties = new StyleParagraphProperties();
            styleParagraphProperties.Append(new SpacingBetweenLines() { Line = "360", LineRule = LineSpacingRuleValues.Auto });
            defaultStyle.Append(styleParagraphProperties);
            
            StyleRunProperties styleRunProperties = new StyleRunProperties();
            styleRunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
            styleRunProperties.Append(new FontSize() { Val = "24" });
            defaultStyle.Append(styleRunProperties);
            
            styles.Append(defaultStyle);
            
            DocumentFormat.OpenXml.Wordprocessing.Style titleStyle = new DocumentFormat.OpenXml.Wordprocessing.Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = "Title"
            };
            
            StyleParagraphProperties titleParagraphProperties = new StyleParagraphProperties();
            titleParagraphProperties.Append(new Justification() { Val = JustificationValues.Center });
            titleStyle.Append(titleParagraphProperties);
            
            StyleRunProperties titleRunProperties = new StyleRunProperties();
            titleRunProperties.Append(new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
            titleRunProperties.Append(new FontSize() { Val = "28" });
            titleRunProperties.Append(new Bold());
            titleStyle.Append(titleRunProperties);
            
            styles.Append(titleStyle);
            
            stylePart.Styles = styles;
        }
    }
}

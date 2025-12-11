namespace DiscreteRVAnalyzer.Resources
{
    public static class Strings
    {
        // General
        public const string EmptyParameter = "Пусте значення параметра.";
        public const string InvalidInputCell = "Неправильний формат в одній з клітинок таблиці. Використовуйте ціле для X і десятковий роздільник '.' або ','.";
        public const string TableEmpty = "Таблиця вводу порожня. Додайте хоча б один рядок.";
        public const string CriticalError = "Критична помилка:\n";

        // Status
        public const string CheckingParameters = "? Перевірка параметрів...";
        public const string CalculationInProgress = "?? Обчислення характеристик...";
        public const string CalculationDone = "? Обчислено";
        public const string Exported = "Експорт виконано";
        public const string ResetParameters = "Параметри скинуті";
        public const string LoadExampleStatus = "Завантажено приклад";

        // Messages / Titles
        public const string ValidationErrorsTitle = "Помилки валідації";
        public const string UserWarningTitle = "Попередження";
        public const string UserErrorTitle = "Помилка";
        public const string InfoTitle = "Інформація";

        public const string CannotCreateDistribution = "Не вдалося створити розподіл.";
        public const string CalculationError = "Помилка при обчисленні характеристик";
        public const string UiDisplayError = "Помилка при відображенні результатів";

        public const string NoCalculationYet = "Спочатку виконайте обчислення.";

        // Export
        public const string PmfNotBuilt = "PMF не побудовано.";
        public const string CdfNotBuilt = "CDF не побудовано.";
        public const string PmfExported = "PMF експортовано";
        public const string CdfExported = "CDF експортовано";
        public const string ExportPmfError = "Помилка експорту PMF: {0}";
        public const string ExportCdfError = "Помилка експорту CDF: {0}";

        // Config
        public const string ErrorLoadingConfig = "Помилка завантаження: {0}";
        public const string ErrorSavingConfig = "Помилка збереження: {0}";
        public const string ConfigLoaded = "Конфігурація завантажена";
        public const string ConfigSaved = "Конфігурація збережена";
        public const string NoCurrentDistribution = "Немає поточного розподілу.";

        // First run / help
        public const string WelcomeTitle = "Ласкаво просимо";
        public const string WelcomeText = "Цей додаток обчислює числові характеристики дискретних випадкових величин\nта будує графіки PMF і CDF.\n\nПочніть з вводу довільної ДВВ у таблицю X,P.";
        public const string AboutTitle = "Про програму";
        public const string AboutText = "Аналіз дискретних випадкових величин\n.NET / WinForms / OxyPlot";
        public const string GuideTitle = "Довідка";
        public const string GuideText = "1. Введіть таблицю X,P для довільної ДВВ.\n2. Натисніть 'Розрахувати'.\n3. Перегляньте характеристики та графіки.";

        // First run coach messages
        public const string Coach_SelectDistribution = "Виберіть тип розподілу тут. У нашому режимі — 'Довільна ДВВ'";
        public const string Coach_FillTable = "Введіть значення X та ймовірності в таблицю нижче";
        public const string Coach_PressCalculate = "Натисніть 'Розрахувати' щоб отримати характеристики та графіки";

        // Manual table errors
        public const string ErrorRowPrefix = "Помилка в рядку ";

        // Theme
        public const string ThemeDark = "Тема темна";
        public const string ThemeLight = "Тема світла";
    }
}

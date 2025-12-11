using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.ComponentModel;
using CsvHelper;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.ImageSharp;
using DiscreteRVAnalyzer.Models;
using DiscreteRVAnalyzer.Services;
using DiscreteRVAnalyzer.Services.Distributions;
using DiscreteRVAnalyzer.Utils;
using DiscreteRVAnalyzer;
using DiscreteRVAnalyzer.Resources;

namespace DiscreteRVAnalyzer.UI
{
    public partial class MainForm : Form
    {
        private DiscreteRandomVariable? _currentRV;
        private StatisticalCharacteristics? _currentStats;

        private ThemeMode _currentTheme = ThemeMode.Light;
        private const string SettingsFileName = "user_settings.json";

        private record UserSettings
        {
            public int DistributionIndex { get; init; } = 0;
            public string TextN { get; init; } = "10";
            public string TextP { get; init; } = "0,5";
            public string TextLambda { get; init; } = "3";
            public string TextK { get; init; } = "5";
            public ThemeMode Theme { get; init; } = ThemeMode.Light;
        }

        public MainForm()
        {
            InitializeComponent();

            // Ensure combobox contains only manual distribution in this simplified mode
            distributionComboBox.Items.Clear();
            distributionComboBox.Items.Add("🔧 Довільна ДВВ");
            distributionComboBox.SelectedIndex = 0;

            // Initialize manual input grid columns and behavior
            InitializeManualInputGrid();

            // Hook DataError to show friendly message instead of default dialog
            if (manualInputGrid != null)
                manualInputGrid.DataError += ManualInputGrid_DataError;

            // Load saved settings early
            LoadSettings();

            // Apply visual theme (use mode-aware overload)
            Theme.Apply(this, _currentTheme);

            // set sensible defaults (may be overridden by settings)
            distributionComboBox.SelectedIndex = distributionComboBox.SelectedIndex >= 0 ? distributionComboBox.SelectedIndex : 0;

            // If first run, show guided coach sequence
            if (!File.Exists("firstrun.flag"))
            {
                RunFirstRunCoach();
                File.WriteAllText("firstrun.flag", "");
            }

            UpdateParameterVisibility();

            // Save settings on exit
            this.FormClosing += (s, e) => SaveSettings();
        }

        /// <summary>
        /// Завантажити приклад даних у таблицю (з дошки)
        /// </summary>
        private void LoadExampleFromBoard()
        {
            if (manualInputGrid == null) return;

            manualInputGrid.Rows.Clear();

            // Дані: X={1,2,3,4,5}, P={0.16, 0.31, 0.32, 0.15, 0.06}
            manualInputGrid.Rows.Add(1, 0.16);
            manualInputGrid.Rows.Add(2, 0.31);
            manualInputGrid.Rows.Add(3, 0.32);
            manualInputGrid.Rows.Add(4, 0.15);
            manualInputGrid.Rows.Add(5, 0.06);

            statusLabel.Text = "Завантажено приклад";
        }

        private bool ValidateVisibleParameters()
        {
            var errors = new List<string>();

            // Проверка N
            if (textBoxN.Visible)
            {
                string errMsg;
                if (!InputValidator.TryParseInt(
                    textBoxN.Text,
                    InputValidator.MIN_N,
                    InputValidator.MAX_N,
                    out var n,
                    out errMsg))
                {
                    errors.Add($"Параметр N: {errMsg}");

                    textBoxN.BackColor = System.Drawing.Color.MistyRose;
                }
                else
                {
                    textBoxN.BackColor = System.Drawing.Color.White;

                    // Дополнительная валидация для конкретных распределений
                    switch (distributionComboBox.SelectedIndex)
                    {
                        case 0: // Биномиальное
                            var pErr = InputValidator.TryParseProbability(textBoxP.Text, out var p, out _);
                            if (pErr)
                            {
                                var binErr = InputValidator.ValidateBinomial(n, p);
                                if (binErr != null) errors.Add($"Параметри біноміального: {binErr}");
                            }
                            break;
                        case 3: // Гипергеометрическое
                            if (InputValidator.TryParseInt(textBoxK.Text, 0, n, out var k, out _) &&
                                InputValidator.TryParseInt(textBoxP.Text, 1, int.MaxValue, out var sampleSize, out _))
                            {
                                var hypErr = InputValidator.ValidateHypergeometric(n, k, sampleSize);
                                if (hypErr != null) errors.Add($"Параметри гіпергеометричного: {hypErr}");
                            }
                            break;
                    }
                }
            }

            // Проверка P
            if (textBoxP.Visible)
            {
                string errMsg;
                if (!InputValidator.TryParseProbability(textBoxP.Text, out var p, out errMsg))
                {
                    errors.Add($"Параметр P: {errMsg}");

                    textBoxP.BackColor = System.Drawing.Color.MistyRose;
                }
                else
                {
                    textBoxP.BackColor = System.Drawing.Color.White;
                }
            }

            // Проверка Lambda
            if (textBoxLambda.Visible)
            {
                string errMsg;
                if (!InputValidator.TryParsePositiveDouble(
                    textBoxLambda.Text,
                    InputValidator.MIN_LAMBDA,
                    InputValidator.MAX_LAMBDA,
                    out var lambda,
                    out errMsg))
                {
                    errors.Add($"Параметр λ: {errMsg}");

                    textBoxLambda.BackColor = System.Drawing.Color.MistyRose;
                }
                else
                {
                    textBoxLambda.BackColor = System.Drawing.Color.White;
                    var poisErr = InputValidator.ValidatePoisson(lambda);
                    if (poisErr != null) errors.Add($"Параметры Пуассона: {poisErr}");
                }
            }

            // Проверка K
            if (textBoxK.Visible)
            {
                string errMsg;
                if (!InputValidator.TryParseInt(textBoxK.Text, 0, int.MaxValue, out _, out errMsg))
                {
                    errors.Add($"Параметр K: {errMsg}");

                    textBoxK.BackColor = System.Drawing.Color.MistyRose;
                }
                else
                {
                    textBoxK.BackColor = System.Drawing.Color.White;
                }
            }

            // Проверка произвольной ДВВ
            bool arbitraryOnly = distributionComboBox.Items.Count == 1;
            if (arbitraryOnly || distributionComboBox.SelectedIndex == 4)
            {
                if ((manualInputGrid?.Rows.Count ?? 0) == 0)
                {
                    errors.Add("Таблиця вводу порожня. Додайте хоча б один рядок.");
                }
            }

            // Если есть ошибки, показываем их
            if (errors.Count > 0)
            {
                string errorMessage = string.Join("\n", errors);

                ErrorHandler.ShowUserWarning(errorMessage, "Помилки валідації");

                statusLabel.Text = "✗ Помилка в параметрах";
                return false;
            }

            return true;
        }


        private void LoadSettings()
        {
            try
            {
                var config = ConfigurationManager.LoadConfig();
                _currentTheme = config.Theme == "Dark" ? ThemeMode.Dark : ThemeMode.Light;

                textBoxN.Text = config.ParameterN;
                textBoxP.Text = config.ParameterP;
                textBoxLambda.Text = config.ParameterLambda;
                textBoxK.Text = config.ParameterK;

                if (config.DistributionIndex >= 0 && config.DistributionIndex < distributionComboBox.Items.Count)
                    distributionComboBox.SelectedIndex = config.DistributionIndex;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Ошибка при загрузке настроек");
                // Продолжаем с значениями по умолчанию
            }
        }


        private void SaveSettings()
        {
            var config = new ConfigurationManager.AppConfig
            {
                DistributionIndex = distributionComboBox.SelectedIndex >= 0 ? distributionComboBox.SelectedIndex : 0,
                ParameterN = textBoxN.Text ?? "",
                ParameterP = textBoxP.Text ?? "",
                ParameterLambda = textBoxLambda.Text ?? "",
                ParameterK = textBoxK.Text ?? "",
                Theme = _currentTheme == ThemeMode.Dark ? "Dark" : "Light"
            };
            ConfigurationManager.SaveConfig(config);
        }

        private void RunFirstRunCoach()
        {
            var steps = new (string message, object target)[]
            {
                (Strings.Coach_SelectDistribution, (object)distributionComboBox),
                (Strings.Coach_FillTable, (object)manualInputGrid),
                (Strings.Coach_PressCalculate, (object)calculateButton)
            };

            FirstRunCoachForm.RunSequence(this, steps);
        }

        // DataError handler to suppress default DataGridView dialog and show user-friendly message
        private void ManualInputGrid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            // Log error
            ErrorHandler.LogError(e.Exception, "Помилка вводу в таблиці");

            // Show friendly Ukrainian message and cancel the error so default dialog doesn't appear
            MessageBox.Show(Strings.InvalidInputCell, Strings.UserErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            e.ThrowException = false;
            e.Cancel = true;
        }


        // ===== ВСПОМОГАТЕЛЬНЫЙ ПАРСЕР DOUBLE =====
        private static double ParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new FormatException("Пустое значение параметра.");

            text = text.Trim().Replace(',', '.');

            return double.Parse(
                text,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture);
        }

        // ===== ОБРАБОТЧИКИ UI =====

        private void DistributionComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateParameterVisibility();
        }

        private void UpdateParameterVisibility()
        {
            bool showN = false, showP = false, showLambda = false, showK = false;

            // If combobox contains only the arbitrary option, treat as arbitrary-only mode
            bool arbitraryOnly = distributionComboBox.Items.Count == 1;

            int selectedIndex = distributionComboBox.SelectedIndex;
            if (arbitraryOnly)
            {
                // hide all parameter inputs when only manual distribution is available
                showN = showP = showLambda = showK = false;
            }
            else
            {
                switch (selectedIndex)
                {
                    case 0: // Биномиальное
                        showN = showP = true;
                        labelP.Text = "p (вероятность):";
                        break;
                    case 1: // Пуассона
                        showLambda = true;
                        break;
                    case 2: // Геометрическое
                        showP = true;
                        labelP.Text = "p (вероятность):";
                        break;
                    case 3: // Гипергеометрическое
                        showN = showK = showP = true;
                        labelP.Text = "n (размер выборки):";
                        break;
                    case 4: // Произвольная ДВВ
                        // все параметры скрываем — работаем с таблицей
                        break;
                }
            }

            labelN.Visible = textBoxN.Visible = showN;
            labelP.Visible = textBoxP.Visible = showP;
            labelLambda.Visible = textBoxLambda.Visible = showLambda;
            labelK.Visible = textBoxK.Visible = showK;
            // manual input grid visible only for arbitrary distribution
            if (manualInputGrid != null)
                manualInputGrid.Visible = arbitraryOnly || distributionComboBox.SelectedIndex == 4;
        }

        private void CalculateButton_Click(object? sender, EventArgs e)
        {
            try
            {
                statusLabel.Text = Strings.CheckingParameters;
                statusProgressBar.Visible = true;
                statusProgressBar.Value = 5;
                Application.DoEvents();

                // ВАЛИДАЦИЯ ВИДИМЫХ ПОЛЕЙ
                if (!ValidateVisibleParameters())
                    return; // Ошибка уже показана пользователю

                statusProgressBar.Value = 20;

                // ПОЛУЧЕНИЕ РАСПРЕДЕЛЕНИЯ
                DiscreteRandomVariable currentRV = null;
                try
                {
                    bool arbitraryOnly = distributionComboBox.Items.Count == 1;

                    // Если режим — только произвольная ДВВ или выбран явно произвольный пункт (индекс 4),
                    // строим RV из таблицы manualInputGrid
                    if (arbitraryOnly || distributionComboBox.SelectedIndex == 4)
                    {
                        // Явно строим из manualInputGrid
                        currentRV = BuildManualRandomVariable();
                    }
                    else
                    {
                        var dist = GetSelectedDistribution();
                        currentRV = dist?.Generate();
                    }

                    if (currentRV == null)
                    {
                        ErrorHandler.ShowUserWarning("Не удалось создать распределение");
                        return;
                    }
                }
                catch (ArgumentException argEx)
                {
                    ErrorHandler.LogError(argEx, "Ошибка параметров");
                    ErrorHandler.ShowUserWarning($"Ошибка параметров: {argEx.Message}");
                    return;
                }

                statusProgressBar.Value = 40;
                Application.DoEvents();

                // ВАЛИДАЦИЯ РАСПРЕДЕЛЕНИЯ
                try
                {
                    currentRV.Validate();
                }
                catch (InvalidOperationException invalidEx)
                {
                    ErrorHandler.LogError(invalidEx, "Распределение невалидно");
                    ErrorHandler.ShowUserWarning($"Распределение некорректно: {invalidEx.Message}");
                    return;
                }

                // Сохраняем текущее распределение
                _currentRV = currentRV;

                statusProgressBar.Value = 60;
                Application.DoEvents();

                // РАСЧЁТ ХАРАКТЕРИСТИК
                statusLabel.Text = "⚙️ Расчёт характеристик...";
                try
                {
                    // CalculationService умеет работать с DiscreteRandomVariable
                    _currentStats = CalculationService.Calculate(_currentRV);
                    if (_currentStats == null)
                        throw new InvalidOperationException("Расчёт вернул null");

                    statusLabel.Text = "✓ Расчёт виконано";
                }
                catch (Exception calcEx)
                {
                    ErrorHandler.LogError(calcEx, "Ошибка расчёта характеристик");
                    ErrorHandler.ShowUserWarning("Ошибка при расчёте характеристик");
                    return;
                }

                statusProgressBar.Value = 85;
                Application.DoEvents();

                // ОБНОВЛЕНИЕ UI
                try
                {
                    UpdateStatistics();
                    UpdateCharts();
                }
                catch (Exception uiEx)
                {
                    ErrorHandler.LogError(uiEx, "Ошибка обновления UI");
                    ErrorHandler.ShowUserWarning("Ошибка при отображении результатов");
                    return;
                }

                statusProgressBar.Value = 100;
                statusLabel.Text = $"✓ Готово | Размер носителя: {_currentRV?.SupportSize ?? 0} | Точність: {(_currentStats?.Mean ?? 0):F4}";

                // Сохранение параметров
                SaveSettings();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Необработанная ошибка в CalculateButton_Click");
                ErrorHandler.ShowUserError($"Критическая ошибка:\n{ex.Message}");
            }
            finally
            {
                statusProgressBar.Visible = false;
            }
        }

        private void ThemeToggleButton_Click(object? sender, EventArgs e)
        {
            _currentTheme = _currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            Theme.Apply(this, _currentTheme);
            // Update button text to indicate current theme
            themeToggleButton.Text = _currentTheme == ThemeMode.Dark ? "☀️ Светлая" : "🌙 Тёмная";
            statusLabel.Text = _currentTheme == ThemeMode.Dark ? "Тёмна тема" : "Світла тема";
            SaveSettings();
        }

        // ===== ВАЛИДАЦИЯ ПОЛЕЙ =====
        private void TextBoxInteger_Validating(object? sender, CancelEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(tb.Text) || !int.TryParse(tb.Text.Trim(), out var v) || v < 0)
                {
                    errorProvider.SetError(tb, "Введите неотрицательное целое число");
                    tb.BackColor = System.Drawing.Color.MistyRose;
                    e.Cancel = true;
                }
                else
                {
                    errorProvider.SetError(tb, string.Empty);
                    tb.BackColor = System.Drawing.Color.White;
                }
            }
        }

        private void TextBoxProbability_Validating(object? sender, CancelEventArgs e)
        {
            if (sender is TextBox tb)
            {
                var s = tb.Text?.Trim().Replace(',', '.');
                if (string.IsNullOrWhiteSpace(s) || !double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) || d < 0 || d > 1)
                {
                    errorProvider.SetError(tb, "Введите вероятность в диапазоне [0,1]");
                    tb.BackColor = System.Drawing.Color.MistyRose;
                    e.Cancel = true;
                }
                else
                {
                    errorProvider.SetError(tb, string.Empty);
                    tb.BackColor = System.Drawing.Color.White;
                }
            }
        }

        private void TextBoxPositiveDouble_Validating(object? sender, CancelEventArgs e)
        {
            if (sender is TextBox tb)
            {
                var s = tb.Text?.Trim().Replace(',', '.');
                if (string.IsNullOrWhiteSpace(s) || !double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) || d <= 0)
                {
                    errorProvider.SetError(tb, "Введите положительное число");
                    tb.BackColor = System.Drawing.Color.MistyRose;
                    e.Cancel = true;
                }
                else
                {
                    errorProvider.SetError(tb, string.Empty);
                    tb.BackColor = System.Drawing.Color.White;
                }
            }
        }

        // ===== СОЗДАНИЕ РАСПРЕДЕЛЕНИЯ ПО ПАРАМЕТРАМ =====

        private DistributionBase GetSelectedDistribution()
        {
            switch (distributionComboBox.SelectedIndex)
            {
                case 0:
                    return new BinomialDistribution(
                        int.Parse(textBoxN.Text.Trim()),
                        ParseDouble(textBoxP.Text));
                case 1:
                    return new PoissonDistribution(
                        ParseDouble(textBoxLambda.Text));
                case 2:
                    return new GeometricDistribution(
                        ParseDouble(textBoxP.Text));
                case 3:
                    return new HypergeometricDistribution(
                        int.Parse(textBoxN.Text.Trim()),
                        int.Parse(textBoxK.Text.Trim()),
                        int.Parse(textBoxP.Text.Trim()));
                default:
                    throw new InvalidOperationException("Для произвольной ДВВ используется таблица X,P.");
            }
        }

        // ===== ПРОИЗВОЛЬНАЯ ДВВ ИЗ ТАБЛИЦЫ X,P =====

        private DiscreteRandomVariable BuildManualRandomVariable()
        {
            var rv = new DiscreteRandomVariable
            {
                Name = "X",
                Description = "Произвольная ДВВ"
            };

            var dict = new Dictionary<int, double>();

            // Используем только manualInputGrid как источник
            if (manualInputGrid == null || manualInputGrid.Rows.Count == 0)
                throw new InvalidOperationException("Таблица ДВВ пуста. Заполните хотя бы одну строку.");

            double totalProbability = 0;
            int validRowCount = 0;

            // Парсим таблицу
            foreach (DataGridViewRow row in manualInputGrid.Rows)
            {
                // Пропускаем пустые строки и строку для добавления
                if (row.IsNewRow) continue;

                try
                {
                    // Читаем значения из колонок "colX" и "colP"
                    object xObj = row.Cells["colX"].Value;
                    object pObj = row.Cells["colP"].Value;

                    // Пропускаем строки с пустыми ячейками
                    if (xObj == null || pObj == null ||
                        string.IsNullOrWhiteSpace(xObj.ToString()) ||
                        string.IsNullOrWhiteSpace(pObj.ToString()))
                        continue;

                    // Парсим X (целое число)
                    string xStr = xObj.ToString().Trim().Replace(',', '.');
                    if (!int.TryParse(xStr, out int xVal))
                    {
                        throw new FormatException($"Некорректное значение X в строке {row.Index + 1}: '{xObj}'");
                    }

                    // Парсим P (вероятность)
                    string pStr = pObj.ToString().Trim().Replace(',', '.');
                    if (!double.TryParse(pStr,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double pVal))
                    {
                        throw new FormatException($"Некорректное значение P в строке {row.Index + 1}: '{pObj}'");
                    }

                    // Валидация вероятности
                    if (pVal < 0 || pVal > 1)
                    {
                        throw new ArgumentException(
                            $"Вероятность P(X={xVal}) = {pVal} должна быть в диапазоне [0, 1] (строка {row.Index + 1})");
                    }

                    // Проверка на дубликаты
                    if (dict.ContainsKey(xVal))
                    {
                        throw new ArgumentException(
                            $"Значение X={xVal} встречается несколько раз! Каждое значение должно быть уникальным.");
                    }

                    dict[xVal] = pVal;
                    totalProbability += pVal;
                    validRowCount++;
                }
                catch (Exception rowEx)
                {
                    ErrorHandler.LogError(rowEx, $"Ошибка при парсинге строки {row.Index + 1}");
                    throw new InvalidOperationException($"Ошибка в строке {row.Index + 1}: {rowEx.Message}", rowEx);
                }
            }

            // Проверка, что хоть что-то прочитано
            if (validRowCount == 0)
                throw new InvalidOperationException("Таблица не содержит корректных данных.");

            // Проверка суммы вероятностей
            if (Math.Abs(totalProbability - 1.0) > 0.01)
            {
                ErrorHandler.LogError(
                    new Exception($"Сумма вероятностей = {totalProbability:F6}"),
                    "Предупреждение о нормализации");
            }

            rv.LoadDistribution(dict);

            // Нормализуем, если сумма не ровно 1
            if (Math.Abs(totalProbability - 1.0) > 1e-9)
            {
                rv.Normalize();
            }

            rv.Validate();

            return rv;
        }

        // ===== ОБНОВЛЕНИЕ СТАТИСТИКИ И ГРАФИКОВ =====

        private void UpdateStatistics()
        {
            if (_currentStats == null) return;

            if (statisticsListView1 != null)
            {
                statisticsListView1.BeginUpdate();
                statisticsListView1.Items.Clear();
                statisticsListView1.Groups.Clear();

                // Ensure columns
                if (statisticsListView1.Columns.Count < 2)
                {
                    statisticsListView1.Columns.Clear();
                    statisticsListView1.Columns.Add("Параметр", 220);
                    statisticsListView1.Columns.Add("Значение", 140);
                }

                // Create groups
                var gInit = new ListViewGroup("Начальные моменты");
                var gVar = new ListViewGroup("Дисперсия и СКО");
                var gCentral = new ListViewGroup("Центральные моменты");
                var gCoef = new ListViewGroup("Асимметрия и эксцесс");
                var gMed = new ListViewGroup("Мода и медиана");
                var gQuart = new ListViewGroup("Квартили");
                var gRange = new ListViewGroup("Диапазон" );
                var gVaria = new ListViewGroup("Вариативность");

                statisticsListView1.Groups.AddRange(new[] { gInit, gVar, gCentral, gCoef, gMed, gQuart, gRange, gVaria });

                ListViewItem AddItem(string name, string value, ListViewGroup group)
                {
                    var lvi = new ListViewItem(name) { Group = group };
                    lvi.SubItems.Add(value);
                    statisticsListView1.Items.Add(lvi);
                    return lvi;
                }

                // Начальные моменты
                AddItem("M(X)", _currentStats.Mean.ToString("F6"), gInit);
                AddItem("M(X²)", _currentStats.SecondMoment.ToString("F6"), gInit);
                AddItem("M(X³)", _currentStats.ThirdMoment.ToString("F6"), gInit);
                AddItem("M(X⁴)", _currentStats.FourthMoment.ToString("F6"), gInit);

                // Дисперсия и СКО
                AddItem("D(X)", _currentStats.Variance.ToString("F6"), gVar);
                AddItem("σ(X)", _currentStats.StandardDeviation.ToString("F6"), gVar);

                // Центральные моменты
                AddItem("μ₂ (central)", _currentStats.CentralSecondMoment.ToString("F6"), gCentral);
                AddItem("μ₃ (central)", _currentStats.CentralThirdMoment.ToString("F6"), gCentral);
                AddItem("μ₄ (central)", _currentStats.CentralFourthMoment.ToString("F6"), gCentral);

                // Асимметрия и эксцесс
                AddItem("Skewness", _currentStats.Skewness.ToString("F6"), gCoef);
                AddItem("Kurtosis", _currentStats.Kurtosis.ToString("F6"), gCoef);

                // Мода и медиана
                AddItem("Mode", _currentStats.Mode.ToString(), gMed);
                AddItem("Median", _currentStats.Median.ToString(), gMed);

                // Квартили
                AddItem("Q1", _currentStats.QuantileQ1.ToString(), gQuart);
                AddItem("Q3", _currentStats.QuantileQ3.ToString(), gQuart);
                AddItem("IQR", _currentStats.InterquartileRange.ToString("F6"), gQuart);

                // Диапазон
                AddItem("Min", _currentStats.MinValue.ToString(), gRange);
                AddItem("Max", _currentStats.MaxValue.ToString(), gRange);
                AddItem("Range", _currentStats.Range.ToString(), gRange);

                // Вариативность
                AddItem("Coef. of variation", _currentStats.CoefficientOfVariation.ToString("F6"), gVaria);
                AddItem("Rel. std dev (%)", _currentStats.RelativeStandardDeviation.ToString("F2"), gVaria);

                // Auto-size columns
                statisticsListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                statisticsListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                statisticsListView1.EndUpdate();
            }

            UpdateAlternativeDistributions();
        }

        private void UpdateAlternativeDistributions()
        {
            if (statisticsListView1 == null) return;

            // Create or find group
            var altGroup = new ListViewGroup("Стандартные распределения");
            statisticsListView1.Groups.Add(altGroup);

            ListViewItem AddItem(string name, string value)
            {
                var lvi = new ListViewItem(name) { Group = altGroup };
                lvi.SubItems.Add(value);
                statisticsListView1.Items.Add(lvi);
                return lvi;
            }

            try
            {
                // Prefer using calculated statistics from the current RV when available
                double mean = double.NaN, variance = double.NaN;
                if (_currentStats != null)
                {
                    mean = _currentStats.Mean;
                    variance = _currentStats.Variance;
                }
                else
                {
                    // Fallback: try to parse UI fields (legacy behaviour)
                    if (!double.TryParse((textBoxN.Text ?? string.Empty).Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out mean))
                        mean = double.NaN;
                    if (!double.TryParse((textBoxP.Text ?? string.Empty).Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out variance))
                        variance = double.NaN;
                }

                // POISSON estimate: lambda = mean (if available)
                if (!double.IsNaN(mean))
                {
                    var lambda = mean;
                    AddItem($"Пуассона M(X)", lambda.ToString("F4"));
                    AddItem($"Пуассона D(X)", lambda.ToString("F4"));
                }

                // BINOMIAL estimate: from mean and variance derive p and N
                if (!double.IsNaN(mean) && !double.IsNaN(variance) && mean > 0)
                {
                    // variance = N p (1-p), mean = N p => variance/mean = 1-p => p = 1 - variance/mean
                    double pEst = 1.0 - (variance / mean);
                    if (pEst > 0 && pEst < 1)
                    {
                        double nEstReal = mean / pEst;
                        int nEst = (int)Math.Round(nEstReal);
                        if (nEst > 0)
                        {
                            double theoMean = nEst * pEst;
                            double theoVar = nEst * pEst * (1 - pEst);
                            AddItem($"Биномиальный p", pEst.ToString("F4"));
                            AddItem($"Биномиальный N", nEst.ToString());
                            AddItem($"Биномиальный M(X)", theoMean.ToString("F4"));
                            AddItem($"Биномиальный D(X)", theoVar.ToString("F4"));
                        }
                    }
                }

                // GEOMETRIC estimate: mean = 1/p => p = 1/mean
                if (!double.IsNaN(mean) && mean > 0)
                {
                    double pGeo = 1.0 / mean;
                    if (pGeo > 0 && pGeo < 1)
                    {
                        double theoMean = 1.0 / pGeo;
                        double theoVar = (1 - pGeo) / (pGeo * pGeo);
                        AddItem($"Геометрический p", pGeo.ToString("F4"));
                        AddItem($"Геометрический M(X)", theoMean.ToString("F4"));
                        AddItem($"Геометрический D(X)", theoVar.ToString("F4"));
                    }
                }

                // UNIFORM estimate: if we have current RV, use its support min/max
                if (_currentRV != null)
                {
                    var distDict = _currentRV.Distribution; // IReadOnlyDictionary<int,double>
                    if (distDict != null && distDict.Count > 0)
                    {
                        int a = int.MaxValue, b = int.MinValue;
                        foreach (var x in distDict.Keys)
                        {
                            if (x < a) a = x;
                            if (x > b) b = x;
                        }
                        if (a <= b)
                        {
                            var uni = new DiscreteRVAnalyzer.UniformDist(a, b);
                            AddItem($"Равномерный a", a.ToString());
                            AddItem($"Равномерный b", b.ToString());
                            AddItem($"Равномерный M(X)", uni.Mean.ToString("F4"));
                            AddItem($"Равномерный D(X)", uni.Variance.ToString("F4"));
                        }
                    }
                }
            }
            catch
            {
                // ignore any construction/parsing errors for alternatives
            }
        }

        private void UpdateCharts()
        {
            if (_currentRV == null) return;
            pmfPlotView.Model = BuildPmfModel(_currentRV);
            cdfPlotView.Model = BuildCdfModel(_currentRV);
        }

        private PlotModel BuildPmfModel(DiscreteRandomVariable rv)
        {
            var model = new PlotModel { Title = "Многоугольник распределения (PMF)" };

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X"
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "P(X = x)",
                Minimum = 0
            });

            var series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                StrokeThickness = 2
            };

            foreach (var (x, y) in GraphService.GetPolygonPoints(rv))
                series.Points.Add(new OxyPlot.DataPoint(x, y));

            model.Series.Add(series);
            return model;
        }

        private PlotModel BuildCdfModel(DiscreteRandomVariable rv)
        {
            var model = new PlotModel { Title = "Интегральная функция распределения (CDF)" };

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X"
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "F(x)",
                Minimum = 0,
                Maximum = 1.05
            });

            var series = new LineSeries
            {
                StrokeThickness = 2
            };

            foreach (var (x, y) in GraphService.GetCumulativePoints(rv))
                series.Points.Add(new OxyPlot.DataPoint(x, y));

            model.Series.Add(series);
            return model;
        }

        // ===== КНОПКИ И МЕНЮ =====

        private void ResetParameters()
        {
            textBoxN.Text = "10";
            textBoxP.Text = "0,5";
            textBoxLambda.Text = "3";
            textBoxK.Text = "5";
            distributionComboBox.SelectedIndex = 0;

            if (statisticsListView1 != null) statisticsListView1.Items.Clear();
            pmfPlotView.Model = null;
            cdfPlotView.Model = null;
            gridManual.Rows.Clear();
            if (manualInputGrid != null) manualInputGrid.Rows.Clear();

            statusLabel.Text = "Параметри сброшені";

            SaveSettings();
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            if (_currentRV == null || _currentStats == null)
            {
                MessageBox.Show("Сначала выполните расчет.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "Текст (*.txt)|*.txt|CSV (*.csv)|*.csv|JSON (*.json)|*.json",
                DefaultExt = "txt"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
            switch (ext)
            {
                case ".txt":
                    ExportService.ExportReportToFile(dlg.FileName, _currentRV, _currentStats);
                    break;
                case ".csv":
                    ExportService.ExportToCsv(dlg.FileName, _currentRV);
                    break;
                case ".json":
                    File.WriteAllText(dlg.FileName, ExportService.ExportToJson(_currentRV));
                    break;
                default:
                    ExportService.ExportReportToFile(dlg.FileName, _currentRV, _currentStats);
                    break;
            }

            statusLabel.Text = "Експортовано";
        }

        private void OnSaveReport(object? sender, EventArgs e) => ExportButton_Click(sender, e);

        private void OnLoadConfig(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                var json = File.ReadAllText(dlg.FileName);
                _currentRV = ExportService.ImportFromJson(json);
                _currentStats = CalculationService.Calculate(_currentRV);
                UpdateStatistics();
                UpdateCharts();
                statusLabel.Text = "Конфігурація завантажена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSaveConfig(object? sender, EventArgs e)
        {
            if (_currentRV == null)
            {
                MessageBox.Show("Немає поточного розподілу.", "Інформація",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                DefaultExt = "json"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(dlg.FileName, ExportService.ExportToJson(_currentRV));
                statusLabel.Text = "Конфігурація збережена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnShowAbout(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Аналіз дискретних випадкових величин\n.NET / WinForms / OxyPlot",
                "О програмі",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnShowGuide(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "1. Виберіть розподіл.\n" +
                "2. Введіть параметри зліва або заповніть таблицю X,P для довільної ДВВ.\n" +
                "3. Натисніть 'Розрахувати'.\n" +
                "4. Дивіться характеристики та графіки справа.",
                "Довідка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void CopyResults()
        {
            if (_currentStats == null) return;
            Clipboard.SetText(_currentStats.GetFormattedReport());
        }

        private void ShowFirstRunWizard()
        {
            MessageBox.Show(
                "Цей додаток обчислює числові характеристики дискретних випадкових величин\n" +
                "та будує графіки PMF і CDF.\n\n" +
                "Почніть з вибору розподілу та введення параметрів або таблиці X,P.",
                "Ласкаво просимо",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Инициализация DataGridView для ввода произвольной ДВВ
        /// </summary>
        private void InitializeManualInputGrid()
        {
            if (manualInputGrid == null) return;

            manualInputGrid.Columns.Clear();

            var colX = new DataGridViewTextBoxColumn
            {
                Name = "colX",
                HeaderText = "X (значение)",
                Width = 100,
                ValueType = typeof(int)
            };
            manualInputGrid.Columns.Add(colX);

            var colP = new DataGridViewTextBoxColumn
            {
                Name = "colP",
                HeaderText = "P (вероятность)",
                Width = 120,
                ValueType = typeof(double)
            };
            manualInputGrid.Columns.Add(colP);

            manualInputGrid.AllowUserToAddRows = true;
            manualInputGrid.AllowUserToDeleteRows = true;
            manualInputGrid.MultiSelect = false;
            manualInputGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Accept both comma and dot as decimal separator when user edits the cell
            manualInputGrid.CellParsing -= ManualInputGrid_CellParsing;
            manualInputGrid.CellParsing += ManualInputGrid_CellParsing;
        }

        // Parse user input in probability column to double accepting both ',' and '.':
        private void ManualInputGrid_CellParsing(object? sender, DataGridViewCellParsingEventArgs e)
        {
            if (manualInputGrid == null) return;

            try
            {
                var col = manualInputGrid.Columns[e.ColumnIndex];
                if (col == null) return;

                if (col.Name == "colP")
                {
                    if (e.Value == null) return;
                    var s = e.Value.ToString();
                    if (string.IsNullOrWhiteSpace(s)) return;

                    // normalize both comma and dot to invariant format
                    var norm = s.Trim().Replace(',', '.');
                    if (double.TryParse(norm, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
                    {
                        e.Value = d;
                        e.ParsingApplied = true;
                    }
                }

                if (col.Name == "colX")
                {
                    if (e.Value == null) return;
                    var s = e.Value.ToString();
                    if (string.IsNullOrWhiteSpace(s)) return;

                    // allow integer parsing with trimming
                    if (int.TryParse(s.Trim(), out var iv))
                    {
                        e.Value = iv;
                        e.ParsingApplied = true;
                    }
                }
            }
            catch
            {
                // let default handling show the error via DataError event / default dialog
            }
        }

        // Обработчик для кнопки "Пример"
        private void LoadExampleButton_Click(object? sender, EventArgs e)
        {
            LoadExampleFromBoard();
        }

        // Event handler wrappers expected by Designer
        private void ExportPmfButton_Click(object? sender, EventArgs e)
        {
            if (pmfPlotView?.Model == null)
            {
                MessageBox.Show(Strings.PmfNotBuilt, Strings.UserErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png", DefaultExt = "png" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var bmp = new System.Drawing.Bitmap(pmfPlotView.Width, pmfPlotView.Height);
                pmfPlotView.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                statusLabel.Text = Strings.PmfExported;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка експорту PMF: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCdfButton_Click(object? sender, EventArgs e)
        {
            if (cdfPlotView?.Model == null)
            {
                MessageBox.Show("CDF не побудовано.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png", DefaultExt = "png" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var bmp = new System.Drawing.Bitmap(cdfPlotView.Width, cdfPlotView.Height);
                cdfPlotView.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                statusLabel.Text = "CDF експортовано";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка експорту CDF: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCopyResultsClick(object? sender, EventArgs e)
        {
            CopyResults();
        }

        private void OnResetClick(object? sender, EventArgs e)
        {
            ResetParameters();
        }

        private void OnTestCoachClick(object? sender, EventArgs e)
        {
            // Run the guided coach sequence for testing (do not modify firstrun.flag)
            RunFirstRunCoach();
        }

        private void OnExitClick(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}

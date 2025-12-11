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
                                if (binErr != null) errors.Add($"Параметры биномиального: {binErr}");
                            }
                            break;
                        case 3: // Гипергеометрическое
                            if (InputValidator.TryParseInt(textBoxK.Text, 0, n, out var k, out _) &&
                                InputValidator.TryParseInt(textBoxP.Text, 1, int.MaxValue, out var sampleSize, out _))
                            {
                                var hypErr = InputValidator.ValidateHypergeometric(n, k, sampleSize);
                                if (hypErr != null) errors.Add($"Параметры гипергеометрического: {hypErr}");
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
                    errors.Add("Таблица ввода пуста. Добавьте хотя бы одну строку.");
                }
            }

            // Если есть ошибки, показываем их
            if (errors.Count > 0)
            {
                string errorMessage = string.Join("\n", errors);

                ErrorHandler.ShowUserWarning(errorMessage, "Ошибки валидации");

                statusLabel.Text = "✗ Ошибка в параметрах";
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
        ("Выберите тип распределения здесь.\nНапример: 'Биномиальное B(n,p)'", (object)distributionComboBox),
        ("Введите число испытаний 'n' здесь (целое).", (object)textBoxN),
        ("Введите вероятность 'p' здесь (0..1).", (object)textBoxP),
        ("Нажмите 'Рассчитать' чтобы выполнить вычисления и построить графики.", (object)calculateButton),
        ("Переключитесь на вкладку 'Таблица значений' чтобы внести произвольную ДВВ.\nДобавьте строки X и P.", (object)chartTabControl)
            };

            FirstRunCoachForm.RunSequence(this, steps);
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
                statusLabel.Text = "⏳ Проверка параметров...";
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

                    statusLabel.Text = "✓ Расчёт выполнен";
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
                statusLabel.Text = $"✓ Готово | Размер носителя: {_currentRV?.SupportSize ?? 0} | Точность: {(_currentStats?.Mean ?? 0):F4}";

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
            statusLabel.Text = _currentTheme == ThemeMode.Dark ? "Тёмная тема" : "Светлая тема";
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
            DataGridView source = null;

            // Выбираем источник данных
            if (manualInputGrid?.Rows.Count > 0)
                source = manualInputGrid;
            else if (gridManual?.Rows.Count > 0)
                source = gridManual;

            if (source == null)
                throw new InvalidOperationException("Таблица ДВВ пуста. Заполните хотя бы одну строку.");


            double totalProbability = 0;

            // Парсим таблицу
            foreach (DataGridViewRow row in source.Rows)
            {
                if (row.IsNewRow) continue;

                try
                {
                    object xObj = source == manualInputGrid
                        ? row.Cells["colX"].Value 

                        : (row.Cells.Count > 0 ? row.Cells[0].Value : null);

                    object pObj = source == manualInputGrid
                        ? row.Cells["colP"].Value 

                        : (row.Cells.Count > 1 ? row.Cells[1].Value : null);

                    if (xObj == null || pObj == null)
                        continue;

                    // Парсим X
                    string xStr = xObj.ToString()?.Trim().Replace(',', '.') ?? "";
                    if (!double.TryParse(xStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var xVal))
                        throw new FormatException($"Некорректное значение X: '{xObj}'");

            // Парсим P
                    string pStr = pObj.ToString()?.Trim().Replace(',', '.') ?? "";
                    if (!double.TryParse(pStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var pVal))
                        throw new FormatException($"Некорректное значение P: '{pObj}'");

            if (pVal < 0)
                        throw new ArgumentException($"Вероятность не может быть отрицательной (X={xVal})");

            if (pVal > 1)
                        throw new ArgumentException($"Вероятность не может быть > 1 (X={xVal})");


                    int xInt = (int)Math.Round(xVal);
                    totalProbability += pVal;

                    if (dict.ContainsKey(xInt))
                        dict[xInt] += pVal;
                    else
                        dict[xInt] = pVal;
                }
                catch (Exception rowEx)
                {
                    ErrorHandler.LogError(rowEx, $"Ошибка при парсинге строки {row.Index}");

                    throw new InvalidOperationException($"Ошибка в строке {row.Index}: {rowEx.Message}");
                }
            }

            if (Math.Abs(totalProbability) < 0.01)
                throw new InvalidOperationException("Сумма вероятностей близка к нулю");


            rv.LoadDistribution(dict);
            rv.Normalize();
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

            bool parsedN = int.TryParse(textBoxN.Text?.Trim(), out var n);
            bool parsedK = int.TryParse(textBoxK.Text?.Trim(), out var k);
            bool parsedP = double.TryParse((textBoxP.Text ?? string.Empty).Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var p);
            bool parsedLambda = double.TryParse((textBoxLambda.Text ?? string.Empty).Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lambda);

            ListViewItem AddItem(string name, string value)
            {
                var lvi = new ListViewItem(name) { Group = altGroup };
                lvi.SubItems.Add(value);
                statisticsListView1.Items.Add(lvi);
                return lvi;
            }

            try
            {
                if (parsedN && parsedP)
                {
                    var bin = new DiscreteRVAnalyzer.BinomialDist(n, p);
                    AddItem($"{bin.Name} M(X)", bin.Mean.ToString("F4"));
                    AddItem($"{bin.Name} D(X)", bin.Variance.ToString("F4"));
                }

                if (parsedLambda)
                {
                    var poi = new DiscreteRVAnalyzer.PoissonDist(lambda);
                    AddItem($"{poi.Name} M(X)", poi.Mean.ToString("F4"));
                    AddItem($"{poi.Name} D(X)", poi.Variance.ToString("F4"));
                }

                if (parsedP)
                {
                    var geo = new DiscreteRVAnalyzer.GeometricDist(p);
                    AddItem($"{geo.Name} M(X)", geo.Mean.ToString("F4"));
                    AddItem($"{geo.Name} D(X)", geo.Variance.ToString("F4"));
                }

                if (parsedN && parsedK)
                {
                    // For uniform as an example use K as upper bound? We'll skip hypergeometric here.
                    // If K looks like b use uniform
                    int a = 0, b = 0;
                    if (parsedK)
                    {
                        // interpret textBoxN as a and textBoxK as b only if sensible
                        a = n; b = k;
                        if (b >= a)
                        {
                            var uni = new DiscreteRVAnalyzer.UniformDist(a, b);
                            AddItem($"{uni.Name} M(X)", uni.Mean.ToString("F4"));
                            AddItem($"{uni.Name} D(X)", uni.Variance.ToString("F4"));
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

            statusLabel.Text = "Параметры сброшены";

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

            statusLabel.Text = "Экспортировано";
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
                statusLabel.Text = "Конфигурация загружена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSaveConfig(object? sender, EventArgs e)
        {
            if (_currentRV == null)
            {
                MessageBox.Show("Нет текущего распределения.", "Информация",
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
                statusLabel.Text = "Конфигурация сохранена";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnShowAbout(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Анализ дискретных случайных величин\n.NET / WinForms / OxyPlot",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnShowGuide(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "1. Выберите распределение.\n" +
                "2. Введите параметры слева или заполните таблицу X,P для произвольной ДВВ.\n" +
                "3. Нажмите 'Рассчитать'.\n" +
                "4. Смотрите характеристики и графики справа.",
                "Справка",
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
                "Это приложение считает числовые характеристики дискретных случайных величин\n" +
                "и строит графики PMF и CDF.\n\n" +
                "Начните с выбора распределения и ввода параметров или таблицы X,P.",
                "Добро пожаловать",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        // Добавьте этот код в класс MainForm (в файле MainForm.cs)

        private void OnExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            // Вызывает ваш существующий метод сброса
            ResetParameters();
        }

        private void OnCopyResultsClick(object sender, EventArgs e)
        {
            // Вызывает ваш существующий метод копирования
            CopyResults();
        }

        private void OnTestCoachClick(object? sender, EventArgs e)
        {
            // Run the guided coach sequence for testing (do not modify firstrun.flag)
            RunFirstRunCoach();
        }

        private void ExportPmfButton_Click(object sender, EventArgs e)
        {
            if (pmfPlotView?.Model == null)
            {
                MessageBox.Show("PMF не построен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png", DefaultExt = "png" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var bmp = new System.Drawing.Bitmap(pmfPlotView.Width, pmfPlotView.Height);
                pmfPlotView.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                statusLabel.Text = "PMF экспортирован";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта PMF: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCdfButton_Click(object sender, EventArgs e)
        {
            if (cdfPlotView?.Model == null)
            {
                MessageBox.Show("CDF не построен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png", DefaultExt = "png" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var bmp = new System.Drawing.Bitmap(cdfPlotView.Width, cdfPlotView.Height);
                cdfPlotView.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                statusLabel.Text = "CDF экспортирован";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта CDF: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

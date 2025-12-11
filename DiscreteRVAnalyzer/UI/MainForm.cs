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

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFileName)) return;
                var json = File.ReadAllText(SettingsFileName);
                var settings = JsonSerializer.Deserialize<UserSettings>(json);
                if (settings == null) return;

                _currentTheme = settings.Theme;

                // apply texts after InitializeComponent
                textBoxN.Text = settings.TextN;
                textBoxP.Text = settings.TextP;
                textBoxLambda.Text = settings.TextLambda;
                textBoxK.Text = settings.TextK;

                if (settings.DistributionIndex >= 0 && settings.DistributionIndex < distributionComboBox.Items.Count)
                    distributionComboBox.SelectedIndex = settings.DistributionIndex;
            }
            catch
            {
                // ignore errors, use defaults
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new UserSettings
                {
                    DistributionIndex = distributionComboBox.SelectedIndex >= 0 ? distributionComboBox.SelectedIndex : 0,
                    TextN = textBoxN.Text ?? "",
                    TextP = textBoxP.Text ?? "",
                    TextLambda = textBoxLambda.Text ?? "",
                    TextK = textBoxK.Text ?? "",
                    Theme = _currentTheme
                };
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFileName, json);
            }
            catch
            {
                // ignore persistence errors
            }
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

            switch (distributionComboBox.SelectedIndex)
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

            labelN.Visible = textBoxN.Visible = showN;
            labelP.Visible = textBoxP.Visible = showP;
            labelLambda.Visible = textBoxLambda.Visible = showLambda;
            labelK.Visible = textBoxK.Visible = showK;
            // manual input grid visible only for arbitrary distribution
            if (manualInputGrid != null)
                manualInputGrid.Visible = distributionComboBox.SelectedIndex == 4;
        }

        private void CalculateButton_Click(object? sender, EventArgs e)
        {
            // Validate only visible parameter fields to avoid unrelated validation blocking calculation
            bool valid = true;
            var cea = new CancelEventArgs();

            if (textBoxN.Visible)
            {
                cea.Cancel = false;
                TextBoxInteger_Validating(textBoxN, cea);
                if (cea.Cancel) valid = false;
            }

            if (textBoxP.Visible)
            {
                cea.Cancel = false;
                TextBoxProbability_Validating(textBoxP, cea);
                if (cea.Cancel) valid = false;
            }

            if (textBoxLambda.Visible)
            {
                cea.Cancel = false;
                TextBoxPositiveDouble_Validating(textBoxLambda, cea);
                if (cea.Cancel) valid = false;
            }

            if (textBoxK.Visible)
            {
                cea.Cancel = false;
                TextBoxInteger_Validating(textBoxK, cea);
                if (cea.Cancel) valid = false;
            }

            if (!valid)
            {
                statusLabel.Text = "Ошибка в параметрах";
                MessageBox.Show("Проверьте поля ввода слева — они содержат неверные значения.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                statusLabel.Text = "Проверка данных...";
                statusProgressBar.Visible = true;
                statusProgressBar.Value = 10;
                Application.DoEvents();

                if (distributionComboBox.SelectedIndex == 4)
                {
                    // Произвольная ДВВ из таблицы (приоритет manualInputGrid в параметрах)
                    _currentRV = BuildManualRandomVariable();
                }
                else
                {
                    var dist = GetSelectedDistribution();
                    statusProgressBar.Value = 40;
                    _currentRV = dist.Generate();
                }

                // НОВОЕ: Валидация перед расчётом
                statusLabel.Text = "Валидация распределения...";
                statusProgressBar.Value = 50;
                _currentRV.Validate();

                statusProgressBar.Value = 70;

                statusLabel.Text = "Расчёт характеристик...";
                _currentStats = CalculationService.Calculate(_currentRV);
                statusProgressBar.Value = 90;

                UpdateStatistics();
                UpdateCharts();

                statusLabel.Text = $"✓ Готово | Размер носителя: {_currentRV.SupportSize}";
                statusProgressBar.Visible = false;

                // Save current parameters
                SaveSettings();
            }
            catch (Exception ex)
            {
                statusProgressBar.Visible = false;
                statusLabel.Text = "✗ Ошибка";
                MessageBox.Show(
                    $"Ошибка:\n\n{ex.Message}\n\n" +
                    $"Убедитесь, что:\n" +
                    $"• Все вероятности в диапазоне [0, 1]\n" +
                    $"• Сумма всех вероятностей = 1\n" +
                    $"• Параметры распределения корректны",
                    "Ошибка расчёта",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                Description = "Произвольная ДВВ (ввод с таблицы)"
            };

            var dict = new System.Collections.Generic.Dictionary<int, double>();

            // prefer manualInputGrid (in parameters) if it exists and has rows, otherwise fallback to gridManual (tab)
            DataGridView source = null;
            if (manualInputGrid != null && manualInputGrid.Rows.Count > 0)
                source = manualInputGrid;
            else if (gridManual != null && gridManual.Rows.Count > 0)
                source = gridManual;

            if (source == null)
                throw new InvalidOperationException("Таблица ДВВ пуста. Заполните хотя бы одну строку.");

            foreach (DataGridViewRow row in source.Rows)
            {
                if (row.IsNewRow) continue;

                object xObj, pObj;
                // manualInputGrid uses named columns colX/colP; gridManual may not
                if (source == manualInputGrid)
                {
                    xObj = row.Cells["colX"].Value;
                    pObj = row.Cells["colP"].Value;
                }
                else
                {
                    xObj = row.Cells.Count > 0 ? row.Cells[0].Value : null;
                    pObj = row.Cells.Count > 1 ? row.Cells[1].Value : null;
                }

                if (xObj == null || pObj == null) continue;

                if (!double.TryParse(xObj.ToString()?.Replace(',', '.'),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out var xVal))
                    throw new FormatException($"Некорректное значение X: '{xObj}'");

                if (!double.TryParse(pObj.ToString()?.Replace(',', '.'),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out var pVal))
                    throw new FormatException($"Некорректное значение P: '{pObj}'");

                if (pVal < 0)
                    throw new ArgumentException($"Вероятность не может быть отрицательной (X={xVal}).");

                int xInt = (int)Math.Round(xVal);

                if (dict.ContainsKey(xInt))
                    dict[xInt] += pVal;
                else
                    dict[xInt] = pVal;
            }

            rv.LoadDistribution(dict);
            rv.Normalize();   // сумма P -> 1
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

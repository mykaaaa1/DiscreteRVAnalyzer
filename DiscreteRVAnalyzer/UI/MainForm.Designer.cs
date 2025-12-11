using System;
using System.Drawing;
using System.Windows.Forms;

namespace DiscreteRVAnalyzer.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // ============= UI ELEMENTS =============
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem fileLoadConfigMenuItem;
        private ToolStripMenuItem fileSaveConfigMenuItem;
        private ToolStripMenuItem fileExitMenuItem;
        private ToolStripMenuItem editMenu;
        private ToolStripMenuItem editCopyResultsMenuItem;
        private ToolStripMenuItem editResetMenuItem;
        private ToolStripMenuItem helpMenu;
        private ToolStripMenuItem helpGuideMenuItem;
        private ToolStripMenuItem helpAboutMenuItem;
        private ToolStripMenuItem exportPmfMenuItem;
        private ToolStripMenuItem exportCdfMenuItem;

        // Toolbar
        private ToolStrip toolStrip;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox distributionComboBox;
        private ToolStripButton calculateButton;
        private ToolStripButton exportButton;
        private ToolStripButton themeToggleButton;
        private ToolStripButton testCoachButton; // тестовая кнопка

        // Main Layout
        private TableLayoutPanel mainTableLayout;
        private Panel leftPanel;
        private TableLayoutPanel leftInnerTable;
        private Panel rightPanel;

        // Left Panel - Parameters
        private GroupBox parametersGroupBox;
        private TableLayoutPanel paramsTableLayout;
        private Label labelN;
        private TextBox textBoxN;
        private Label labelP;
        private TextBox textBoxP;
        private Label labelLambda;
        private TextBox textBoxLambda;
        private Label labelK;
        private TextBox textBoxK;
        private Button resetButton;

        // Left Panel - Statistics
        private GroupBox statisticsGroupBox;
        private ListView statisticsListView1;

        // Right Panel - Charts & Grid
        private TabControl chartTabControl;
        private TabPage pmfTabPage;
        private TabPage cdfTabPage;
        private TabPage tableTabPage; // Вкладка для таблицы

        private OxyPlot.WindowsForms.PlotView pmfPlotView;
        private OxyPlot.WindowsForms.PlotView cdfPlotView;
        private DataGridView gridManual; // Таблица (которой не хватало)
        public DataGridView manualInputGrid; // новая таблица для ввода значений - ПУБЛИЧНАЯ!
        private GroupBox manualInputGroupBox;

        // Status Bar
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar statusProgressBar;

        // UX helpers
        private ToolTip toolTip;
        private ErrorProvider errorProvider;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menuStrip = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            fileLoadConfigMenuItem = new ToolStripMenuItem();
            fileSaveConfigMenuItem = new ToolStripMenuItem();
            exportPmfMenuItem = new ToolStripMenuItem();
            exportCdfMenuItem = new ToolStripMenuItem();
            fileExitMenuItem = new ToolStripMenuItem();
            editMenu = new ToolStripMenuItem();
            editCopyResultsMenuItem = new ToolStripMenuItem();
            editResetMenuItem = new ToolStripMenuItem();
            helpMenu = new ToolStripMenuItem();
            helpGuideMenuItem = new ToolStripMenuItem();
            helpAboutMenuItem = new ToolStripMenuItem();
            toolStrip = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            distributionComboBox = new ToolStripComboBox();
            calculateButton = new ToolStripButton();
            exportButton = new ToolStripButton();
            themeToggleButton = new ToolStripButton();
            testCoachButton = new ToolStripButton();
            mainTableLayout = new TableLayoutPanel();
            leftPanel = new Panel();
            leftInnerTable = new TableLayoutPanel();
            parametersGroupBox = new GroupBox();
            paramsTableLayout = new TableLayoutPanel();
            labelN = new Label();
            textBoxN = new TextBox();
            labelP = new Label();
            textBoxP = new TextBox();
            labelLambda = new Label();
            textBoxLambda = new TextBox();
            labelK = new Label();
            textBoxK = new TextBox();
            resetButton = new Button();
            manualInputGroupBox = new GroupBox();
            manualInputGrid = new DataGridView();
            colX = new DataGridViewTextBoxColumn();
            colP = new DataGridViewTextBoxColumn();
            statisticsGroupBox = new GroupBox();
            statisticsListView1 = new ListView();
            rightPanel = new Panel();
            chartTabControl = new TabControl();
            pmfTabPage = new TabPage();
            pmfPlotView = new OxyPlot.WindowsForms.PlotView();
            cdfTabPage = new TabPage();
            cdfPlotView = new OxyPlot.WindowsForms.PlotView();
            tableTabPage = new TabPage();
            gridManual = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusProgressBar = new ToolStripProgressBar();
            toolTip = new ToolTip(components);
            errorProvider = new ErrorProvider(components);
            menuStrip.SuspendLayout();
            toolStrip.SuspendLayout();
            mainTableLayout.SuspendLayout();
            leftPanel.SuspendLayout();
            leftInnerTable.SuspendLayout();
            parametersGroupBox.SuspendLayout();
            paramsTableLayout.SuspendLayout();
            manualInputGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)manualInputGrid).BeginInit();
            statisticsGroupBox.SuspendLayout();
            rightPanel.SuspendLayout();
            chartTabControl.SuspendLayout();
            pmfTabPage.SuspendLayout();
            cdfTabPage.SuspendLayout();
            tableTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridManual).BeginInit();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.BackColor = Color.FromArgb(44, 62, 80);
            menuStrip.ForeColor = Color.White;
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, helpMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1600, 24);
            menuStrip.TabIndex = 3;
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { fileLoadConfigMenuItem, fileSaveConfigMenuItem, exportPmfMenuItem, exportCdfMenuItem, fileExitMenuItem });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(63, 20);
            fileMenu.Text = "📁 Файл";
            // 
            // fileLoadConfigMenuItem
            // 
            fileLoadConfigMenuItem.Name = "fileLoadConfigMenuItem";
            fileLoadConfigMenuItem.Size = new Size(228, 22);
            fileLoadConfigMenuItem.Text = "Загрузить конфигурацию...";
            fileLoadConfigMenuItem.Click += OnLoadConfig;
            // 
            // fileSaveConfigMenuItem
            // 
            fileSaveConfigMenuItem.Name = "fileSaveConfigMenuItem";
            fileSaveConfigMenuItem.Size = new Size(228, 22);
            fileSaveConfigMenuItem.Text = "Сохранить конфигурацию...";
            fileSaveConfigMenuItem.Click += OnSaveConfig;
            // 
            // exportPmfMenuItem
            // 
            exportPmfMenuItem.Name = "exportPmfMenuItem";
            exportPmfMenuItem.Size = new Size(228, 22);
            exportPmfMenuItem.Text = "Экспорт PMF...";
            exportPmfMenuItem.Click += ExportPmfButton_Click;
            // 
            // exportCdfMenuItem
            // 
            exportCdfMenuItem.Name = "exportCdfMenuItem";
            exportCdfMenuItem.Size = new Size(228, 22);
            exportCdfMenuItem.Text = "Экспорт CDF...";
            exportCdfMenuItem.Click += ExportCdfButton_Click;
            // 
            // fileExitMenuItem
            // 
            fileExitMenuItem.Name = "fileExitMenuItem";
            fileExitMenuItem.Size = new Size(228, 22);
            fileExitMenuItem.Text = "Выход";
            fileExitMenuItem.Click += OnExitClick;
            // 
            // editMenu
            // 
            editMenu.DropDownItems.AddRange(new ToolStripItem[] { editCopyResultsMenuItem, editResetMenuItem });
            editMenu.Name = "editMenu";
            editMenu.Size = new Size(74, 20);
            editMenu.Text = "✏️ Правка";
            // 
            // editCopyResultsMenuItem
            // 
            editCopyResultsMenuItem.Name = "editCopyResultsMenuItem";
            editCopyResultsMenuItem.Size = new Size(204, 22);
            editCopyResultsMenuItem.Text = "Копировать результаты";
            editCopyResultsMenuItem.Click += OnCopyResultsClick;
            // 
            // editResetMenuItem
            // 
            editResetMenuItem.Name = "editResetMenuItem";
            editResetMenuItem.Size = new Size(204, 22);
            editResetMenuItem.Text = "Сбросить параметры";
            editResetMenuItem.Click += OnResetClick;
            // 
            // helpMenu
            // 
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { helpGuideMenuItem, helpAboutMenuItem });
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new Size(80, 20);
            helpMenu.Text = "❓ Справка";
            // 
            // helpGuideMenuItem
            // 
            helpGuideMenuItem.Name = "helpGuideMenuItem";
            helpGuideMenuItem.Size = new Size(224, 22);
            helpGuideMenuItem.Text = "Интерактивная инструкция";
            helpGuideMenuItem.Click += OnShowGuide;
            // 
            // helpAboutMenuItem
            // 
            helpAboutMenuItem.Name = "helpAboutMenuItem";
            helpAboutMenuItem.Size = new Size(224, 22);
            helpAboutMenuItem.Text = "О программе";
            helpAboutMenuItem.Click += OnShowAbout;
            // 
            // toolStrip
            // 
            toolStrip.BackColor = Color.FromArgb(52, 73, 94);
            toolStrip.ForeColor = Color.White;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { toolStripLabel1, distributionComboBox, calculateButton, exportButton, themeToggleButton, testCoachButton });
            toolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip.Location = new Point(0, 24);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(10, 5, 10, 5);
            toolStrip.Size = new Size(1600, 33);
            toolStrip.TabIndex = 2;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(109, 20);
            toolStripLabel1.Text = "📊 Распределение:";
            // 
            // distributionComboBox
            // 
            distributionComboBox.AutoSize = false;
            distributionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            distributionComboBox.Items.AddRange(new object[] { "🔧 Произвольная ДВВ" });
            distributionComboBox.Name = "distributionComboBox";
            distributionComboBox.Size = new Size(250, 23);
            distributionComboBox.SelectedIndexChanged += DistributionComboBox_SelectedIndexChanged;
            // 
            // calculateButton
            // 
            calculateButton.BackColor = Color.FromArgb(39, 174, 96);
            calculateButton.ForeColor = Color.White;
            calculateButton.Name = "calculateButton";
            calculateButton.Size = new Size(85, 20);
            calculateButton.Text = "▶️ Рассчитать";
            calculateButton.ToolTipText = "Выполнить расчёт и построить графики";
            calculateButton.Click += CalculateButton_Click;
            // 
            // exportButton
            // 
            exportButton.BackColor = Color.FromArgb(52, 152, 219);
            exportButton.ForeColor = Color.White;
            exportButton.Name = "exportButton";
            exportButton.Size = new Size(115, 20);
            exportButton.Text = "📥 Экспортировать";
            exportButton.ToolTipText = "Экспортировать текущее распределение и отчёт";
            exportButton.Click += ExportButton_Click;
            // 
            // themeToggleButton
            // 
            themeToggleButton.BackColor = Color.FromArgb(88, 101, 128);
            themeToggleButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            themeToggleButton.ForeColor = Color.White;
            themeToggleButton.Name = "themeToggleButton";
            themeToggleButton.Size = new Size(54, 20);
            themeToggleButton.Text = "🌙 Тема";
            themeToggleButton.ToolTipText = "Переключить тему (тёмная/светлая)";
            themeToggleButton.Click += ThemeToggleButton_Click;
            // 
            // testCoachButton
            // 
            testCoachButton.BackColor = Color.FromArgb(120, 120, 120);
            testCoachButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            testCoachButton.ForeColor = Color.White;
            testCoachButton.Name = "testCoachButton";
            testCoachButton.Size = new Size(102, 20);
            testCoachButton.Text = "Тест инструкция";
            testCoachButton.ToolTipText = "Запустить тестовую инструкцию";
            testCoachButton.Click += OnTestCoachClick;
            // 
            // mainTableLayout
            // 
            mainTableLayout.ColumnCount = 2;
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            mainTableLayout.Controls.Add(leftPanel, 0, 0);
            mainTableLayout.Controls.Add(rightPanel, 1, 0);
            mainTableLayout.Dock = DockStyle.Fill;
            mainTableLayout.Location = new Point(0, 57);
            mainTableLayout.Name = "mainTableLayout";
            mainTableLayout.RowCount = 1;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.Size = new Size(1600, 821);
            mainTableLayout.TabIndex = 0;
            // 
            // leftPanel
            // 
            leftPanel.Controls.Add(leftInnerTable);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(3, 3);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(10);
            leftPanel.Size = new Size(394, 815);
            leftPanel.TabIndex = 0;
            // 
            // leftInnerTable
            // 
            leftInnerTable.ColumnCount = 1;
            leftInnerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftInnerTable.Controls.Add(parametersGroupBox, 0, 0);
            leftInnerTable.Controls.Add(manualInputGroupBox, 0, 1);
            leftInnerTable.Controls.Add(statisticsGroupBox, 0, 2);
            leftInnerTable.Dock = DockStyle.Fill;
            leftInnerTable.Location = new Point(10, 10);
            leftInnerTable.Name = "leftInnerTable";
            leftInnerTable.RowCount = 3;
            leftInnerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
            leftInnerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F));
            leftInnerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftInnerTable.Size = new Size(374, 795);
            leftInnerTable.TabIndex = 0;
            // 
            // parametersGroupBox
            // 
            parametersGroupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            parametersGroupBox.Controls.Add(paramsTableLayout);
            parametersGroupBox.Dock = DockStyle.Fill;
            parametersGroupBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            parametersGroupBox.Location = new Point(3, 3);
            parametersGroupBox.Name = "parametersGroupBox";
            parametersGroupBox.Size = new Size(368, 214);
            parametersGroupBox.TabIndex = 0;
            parametersGroupBox.TabStop = false;
            parametersGroupBox.Text = "⚙️ Параметры";
            // 
            // paramsTableLayout
            // 
            paramsTableLayout.ColumnCount = 2;
            paramsTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            paramsTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            paramsTableLayout.Controls.Add(labelN, 0, 0);
            paramsTableLayout.Controls.Add(textBoxN, 1, 0);
            paramsTableLayout.Controls.Add(labelP, 0, 1);
            paramsTableLayout.Controls.Add(textBoxP, 1, 1);
            paramsTableLayout.Controls.Add(labelLambda, 0, 2);
            paramsTableLayout.Controls.Add(textBoxLambda, 1, 2);
            paramsTableLayout.Controls.Add(labelK, 0, 3);
            paramsTableLayout.Controls.Add(textBoxK, 1, 3);
            paramsTableLayout.Controls.Add(resetButton, 0, 5);
            paramsTableLayout.Dock = DockStyle.Fill;
            paramsTableLayout.Location = new Point(3, 19);
            paramsTableLayout.Name = "paramsTableLayout";
            paramsTableLayout.RowCount = 6;
            paramsTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            paramsTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            paramsTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            paramsTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            paramsTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            paramsTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
            paramsTableLayout.Size = new Size(362, 192);
            paramsTableLayout.TabIndex = 0;
            // 
            // labelN
            // 
            labelN.AutoSize = true;
            labelN.Location = new Point(3, 0);
            labelN.Name = "labelN";
            labelN.Size = new Size(17, 15);
            labelN.TabIndex = 0;
            labelN.Text = "n:";
            // 
            // textBoxN
            // 
            textBoxN.Dock = DockStyle.Fill;
            textBoxN.Location = new Point(150, 3);
            textBoxN.Margin = new Padding(6, 3, 6, 3);
            textBoxN.Name = "textBoxN";
            textBoxN.Size = new Size(206, 23);
            textBoxN.TabIndex = 1;
            toolTip.SetToolTip(textBoxN, "Число испытаний (целое, n >= 0)");
            textBoxN.Validating += TextBoxInteger_Validating;
            // 
            // labelP
            // 
            labelP.AutoSize = true;
            labelP.Location = new Point(3, 30);
            labelP.Name = "labelP";
            labelP.Size = new Size(17, 15);
            labelP.TabIndex = 2;
            labelP.Text = "p:";
            // 
            // textBoxP
            // 
            textBoxP.Dock = DockStyle.Fill;
            textBoxP.Location = new Point(150, 33);
            textBoxP.Margin = new Padding(6, 3, 6, 3);
            textBoxP.Name = "textBoxP";
            textBoxP.Size = new Size(206, 23);
            textBoxP.TabIndex = 3;
            toolTip.SetToolTip(textBoxP, "Вероятность успеха (0 ≤ p ≤ 1)");
            textBoxP.Validating += TextBoxProbability_Validating;
            // 
            // labelLambda
            // 
            labelLambda.AutoSize = true;
            labelLambda.Location = new Point(3, 60);
            labelLambda.Name = "labelLambda";
            labelLambda.Size = new Size(17, 15);
            labelLambda.TabIndex = 4;
            labelLambda.Text = "λ:";
            // 
            // textBoxLambda
            // 
            textBoxLambda.Dock = DockStyle.Fill;
            textBoxLambda.Location = new Point(150, 63);
            textBoxLambda.Margin = new Padding(6, 3, 6, 3);
            textBoxLambda.Name = "textBoxLambda";
            textBoxLambda.Size = new Size(206, 23);
            textBoxLambda.TabIndex = 5;
            toolTip.SetToolTip(textBoxLambda, "Параметр λ (> 0)");
            textBoxLambda.Validating += TextBoxPositiveDouble_Validating;
            // 
            // labelK
            // 
            labelK.AutoSize = true;
            labelK.Location = new Point(3, 90);
            labelK.Name = "labelK";
            labelK.Size = new Size(18, 15);
            labelK.TabIndex = 6;
            labelK.Text = "K:";
            // 
            // textBoxK
            // 
            textBoxK.Dock = DockStyle.Fill;
            textBoxK.Location = new Point(150, 93);
            textBoxK.Margin = new Padding(6, 3, 6, 3);
            textBoxK.Name = "textBoxK";
            textBoxK.Size = new Size(206, 23);
            textBoxK.TabIndex = 7;
            toolTip.SetToolTip(textBoxK, "Число благоприятных (целое, K >= 0)");
            textBoxK.Validating += TextBoxInteger_Validating;
            // 
            // resetButton
            // 
            resetButton.BackColor = Color.LightGray;
            paramsTableLayout.SetColumnSpan(resetButton, 2);
            resetButton.Dock = DockStyle.Fill;
            resetButton.Location = new Point(3, 111);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(356, 78);
            resetButton.TabIndex = 8;
            resetButton.Text = "Сбросить";
            resetButton.UseVisualStyleBackColor = false;
            resetButton.Click += OnResetClick;
            // 
            // manualInputGroupBox
            // 
            manualInputGroupBox.Controls.Add(manualInputGrid);
            manualInputGroupBox.Dock = DockStyle.Fill;
            manualInputGroupBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            manualInputGroupBox.Location = new Point(3, 223);
            manualInputGroupBox.Name = "manualInputGroupBox";
            manualInputGroupBox.Size = new Size(368, 244);
            manualInputGroupBox.TabIndex = 1;
            manualInputGroupBox.TabStop = false;
            manualInputGroupBox.Text = "📋 Таблица ввода (произвольная ДВВ)";
            // 
            // manualInputGrid
            // 
            manualInputGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            manualInputGrid.BackgroundColor = Color.White;
            manualInputGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            manualInputGrid.Columns.AddRange(new DataGridViewColumn[] { colX, colP });
            manualInputGrid.Dock = DockStyle.Fill;
            manualInputGrid.Location = new Point(3, 19);
            manualInputGrid.Name = "manualInputGrid";
            manualInputGrid.RowHeadersVisible = false;
            manualInputGrid.Size = new Size(362, 222);
            manualInputGrid.TabIndex = 0;
            // 
            // colX
            // 
            colX.HeaderText = "X (значение)";
            colX.Name = "colX";
            // 
            // colP
            // 
            colP.HeaderText = "P (вероятность)";
            colP.Name = "colP";
            // 
            // statisticsGroupBox
            // 
            statisticsGroupBox.Controls.Add(statisticsListView1);
            statisticsGroupBox.Dock = DockStyle.Fill;
            statisticsGroupBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            statisticsGroupBox.Location = new Point(3, 473);
            statisticsGroupBox.Name = "statisticsGroupBox";
            statisticsGroupBox.Size = new Size(368, 319);
            statisticsGroupBox.TabIndex = 1;
            statisticsGroupBox.TabStop = false;
            statisticsGroupBox.Text = "📊 Статистика";
            // 
            // statisticsListView1
            // 
            statisticsListView1.Dock = DockStyle.Fill;
            statisticsListView1.FullRowSelect = true;
            statisticsListView1.Location = new Point(3, 19);
            statisticsListView1.Name = "statisticsListView1";
            statisticsListView1.Size = new Size(362, 297);
            statisticsListView1.TabIndex = 0;
            statisticsListView1.UseCompatibleStateImageBehavior = false;
            statisticsListView1.View = View.Details;
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(chartTabControl);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(403, 3);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(5);
            rightPanel.Size = new Size(1194, 815);
            rightPanel.TabIndex = 1;
            // 
            // chartTabControl
            // 
            chartTabControl.Controls.Add(pmfTabPage);
            chartTabControl.Controls.Add(cdfTabPage);
            chartTabControl.Controls.Add(tableTabPage);
            chartTabControl.Dock = DockStyle.Fill;
            chartTabControl.Location = new Point(5, 5);
            chartTabControl.Name = "chartTabControl";
            chartTabControl.SelectedIndex = 0;
            chartTabControl.Size = new Size(1184, 805);
            chartTabControl.TabIndex = 0;
            // 
            // pmfTabPage
            // 
            pmfTabPage.Controls.Add(pmfPlotView);
            pmfTabPage.Location = new Point(4, 24);
            pmfTabPage.Name = "pmfTabPage";
            pmfTabPage.Size = new Size(1176, 777);
            pmfTabPage.TabIndex = 0;
            pmfTabPage.Text = "PMF";
            // 
            // pmfPlotView
            // 
            pmfPlotView.Dock = DockStyle.Fill;
            pmfPlotView.Location = new Point(0, 0);
            pmfPlotView.Name = "pmfPlotView";
            pmfPlotView.PanCursor = Cursors.Hand;
            pmfPlotView.Size = new Size(1176, 777);
            pmfPlotView.TabIndex = 0;
            pmfPlotView.ZoomHorizontalCursor = Cursors.SizeWE;
            pmfPlotView.ZoomRectangleCursor = Cursors.SizeNWSE;
            pmfPlotView.ZoomVerticalCursor = Cursors.SizeNS;
            // 
            // cdfTabPage
            // 
            cdfTabPage.Controls.Add(cdfPlotView);
            cdfTabPage.Location = new Point(4, 24);
            cdfTabPage.Name = "cdfTabPage";
            cdfTabPage.Size = new Size(1176, 777);
            cdfTabPage.TabIndex = 1;
            cdfTabPage.Text = "CDF";
            // 
            // cdfPlotView
            // 
            cdfPlotView.Dock = DockStyle.Fill;
            cdfPlotView.Location = new Point(0, 0);
            cdfPlotView.Name = "cdfPlotView";
            cdfPlotView.PanCursor = Cursors.Hand;
            cdfPlotView.Size = new Size(1176, 777);
            cdfPlotView.TabIndex = 0;
            cdfPlotView.ZoomHorizontalCursor = Cursors.SizeWE;
            cdfPlotView.ZoomRectangleCursor = Cursors.SizeNWSE;
            cdfPlotView.ZoomVerticalCursor = Cursors.SizeNS;
            // 
            // tableTabPage
            // 
            tableTabPage.Controls.Add(gridManual);
            tableTabPage.Location = new Point(4, 24);
            tableTabPage.Name = "tableTabPage";
            tableTabPage.Size = new Size(1176, 777);
            tableTabPage.TabIndex = 2;
            tableTabPage.Text = "Таблица значений";
            // 
            // gridManual
            // 
            gridManual.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridManual.BackgroundColor = Color.White;
            gridManual.BorderStyle = BorderStyle.None;
            gridManual.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2 });
            gridManual.Dock = DockStyle.Fill;
            gridManual.Location = new Point(0, 0);
            gridManual.Name = "gridManual";
            gridManual.RowHeadersVisible = false;
            gridManual.Size = new Size(1176, 777);
            gridManual.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "X";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "P";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, statusProgressBar });
            statusStrip.Location = new Point(0, 878);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1600, 22);
            statusStrip.TabIndex = 1;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(0, 17);
            // 
            // statusProgressBar
            // 
            statusProgressBar.Name = "statusProgressBar";
            statusProgressBar.Size = new Size(100, 16);
            // 
            // errorProvider
            // 
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(1600, 900);
            Controls.Add(mainTableLayout);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.FromArgb(33, 33, 33);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Анализ дискретных случайных величин";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            mainTableLayout.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            leftInnerTable.ResumeLayout(false);
            parametersGroupBox.ResumeLayout(false);
            paramsTableLayout.ResumeLayout(false);
            paramsTableLayout.PerformLayout();
            manualInputGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)manualInputGrid).EndInit();
            statisticsGroupBox.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            chartTabControl.ResumeLayout(false);
            pmfTabPage.ResumeLayout(false);
            cdfTabPage.ResumeLayout(false);
            tableTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridManual).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private TextBox CreateParameterTextBox(string defaultValue)
        {
            return new TextBox
            {
                Text = defaultValue,
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Center
            };
        }

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn colX;
        private DataGridViewTextBoxColumn colP;
    }
}
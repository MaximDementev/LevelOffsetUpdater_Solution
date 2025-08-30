using LevelOffsetUpdater.Core;
using LevelOffsetUpdater.Services;
using System;
using System.Windows.Forms;

namespace LevelOffsetUpdater.UI
{
    // Форма настроек приложения
    public partial class SettingsForm : Form
    {
        #region Fields
        private readonly IAutoUpdateService _autoUpdateService;
        private readonly IManualUpdateService _manualUpdateService;
        private Settings _settings;
        #endregion

        #region Constructor
        public SettingsForm(IAutoUpdateService autoUpdateService, IManualUpdateService manualUpdateService)
        {
            _autoUpdateService = autoUpdateService ?? throw new ArgumentNullException(nameof(autoUpdateService));
            _manualUpdateService = manualUpdateService ?? throw new ArgumentNullException(nameof(manualUpdateService));

            InitializeComponent();
            LoadSettings();
        }
        #endregion

        #region Private Methods
        // Загружает настройки в элементы формы
        private void LoadSettings()
        {
            _settings = Settings.Load();
            chkAutoUpdate.Checked = _settings.AutoUpdateEnabled;
            numRoundingStep.Value = _settings.RoundingStepMm;
            numWarningThreshold.Value = _settings.WarningThreshold;
        }

        // Сохраняет настройки из элементов формы
        private void SaveSettings()
        {
            _settings.AutoUpdateEnabled = chkAutoUpdate.Checked;
            _settings.RoundingStepMm = (int)numRoundingStep.Value;
            _settings.WarningThreshold = (int)numWarningThreshold.Value;
            _settings.Save();

            _autoUpdateService.IsEnabled = _settings.AutoUpdateEnabled;
        }
        #endregion

        #region Event Handlers
        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void numRoundingStep_ValueChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void numWarningThreshold_ValueChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void btnUpdateAll_Click(object sender, EventArgs e)
        {
            _manualUpdateService.Raise();
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.lblRoundingStep = new System.Windows.Forms.Label();
            this.numRoundingStep = new System.Windows.Forms.NumericUpDown();
            this.lblWarningThreshold = new System.Windows.Forms.Label();
            this.numWarningThreshold = new System.Windows.Forms.NumericUpDown();
            this.btnUpdateAll = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblRoundingStepUnit = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numRoundingStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWarningThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.Location = new System.Drawing.Point(20, 20);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(320, 46);
            this.chkAutoUpdate.TabIndex = 0;
            this.chkAutoUpdate.Text = "Автоматически обновлять отметки расположения (настройка сохраняется при перезагру" +
    "зке Revit)";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            this.chkAutoUpdate.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // lblRoundingStep
            // 
            this.lblRoundingStep.AutoSize = true;
            this.lblRoundingStep.Location = new System.Drawing.Point(20, 95);
            this.lblRoundingStep.Name = "lblRoundingStep";
            this.lblRoundingStep.Size = new System.Drawing.Size(116, 13);
            this.lblRoundingStep.TabIndex = 1;
            this.lblRoundingStep.Text = "Шаг округления (мм):";
            // 
            // numRoundingStep
            // 
            this.numRoundingStep.Location = new System.Drawing.Point(200, 93);
            this.numRoundingStep.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numRoundingStep.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRoundingStep.Name = "numRoundingStep";
            this.numRoundingStep.Size = new System.Drawing.Size(80, 20);
            this.numRoundingStep.TabIndex = 2;
            this.numRoundingStep.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numRoundingStep.ValueChanged += new System.EventHandler(this.numRoundingStep_ValueChanged);
            // 
            // lblWarningThreshold
            // 
            this.lblWarningThreshold.Location = new System.Drawing.Point(20, 114);
            this.lblWarningThreshold.Name = "lblWarningThreshold";
            this.lblWarningThreshold.Size = new System.Drawing.Size(174, 37);
            this.lblWarningThreshold.TabIndex = 4;
            this.lblWarningThreshold.Text = "Предупреждать при количестве элементов больше:";
            this.lblWarningThreshold.Click += new System.EventHandler(this.lblWarningThreshold_Click);
            // 
            // numWarningThreshold
            // 
            this.numWarningThreshold.Location = new System.Drawing.Point(200, 128);
            this.numWarningThreshold.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numWarningThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWarningThreshold.Name = "numWarningThreshold";
            this.numWarningThreshold.Size = new System.Drawing.Size(80, 20);
            this.numWarningThreshold.TabIndex = 5;
            this.numWarningThreshold.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numWarningThreshold.ValueChanged += new System.EventHandler(this.numWarningThreshold_ValueChanged);
            // 
            // btnUpdateAll
            // 
            this.btnUpdateAll.Location = new System.Drawing.Point(32, 208);
            this.btnUpdateAll.Name = "btnUpdateAll";
            this.btnUpdateAll.Size = new System.Drawing.Size(150, 30);
            this.btnUpdateAll.TabIndex = 6;
            this.btnUpdateAll.Text = "Обновить у всех сейчас";
            this.btnUpdateAll.UseVisualStyleBackColor = true;
            this.btnUpdateAll.Click += new System.EventHandler(this.btnUpdateAll_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(188, 208);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblRoundingStepUnit
            // 
            this.lblRoundingStepUnit.AutoSize = true;
            this.lblRoundingStepUnit.Location = new System.Drawing.Point(285, 95);
            this.lblRoundingStepUnit.Name = "lblRoundingStepUnit";
            this.lblRoundingStepUnit.Size = new System.Drawing.Size(23, 13);
            this.lblRoundingStepUnit.TabIndex = 3;
            this.lblRoundingStepUnit.Text = "мм";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 250);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnUpdateAll);
            this.Controls.Add(this.numWarningThreshold);
            this.Controls.Add(this.lblWarningThreshold);
            this.Controls.Add(this.lblRoundingStepUnit);
            this.Controls.Add(this.numRoundingStep);
            this.Controls.Add(this.lblRoundingStep);
            this.Controls.Add(this.chkAutoUpdate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Настройки обновления отметок";
            ((System.ComponentModel.ISupportInitialize)(this.numRoundingStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWarningThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Windows Form Designer generated variables
        private CheckBox chkAutoUpdate;
        private Label lblRoundingStep;
        private NumericUpDown numRoundingStep;
        private Label lblRoundingStepUnit;
        private Label lblWarningThreshold;
        private NumericUpDown numWarningThreshold;
        private Button btnUpdateAll;
        private Button btnClose;
        #endregion

        private void lblWarningThreshold_Click(object sender, EventArgs e)
        {

        }
    }
}

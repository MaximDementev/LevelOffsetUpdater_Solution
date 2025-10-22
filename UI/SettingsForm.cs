using LevelOffsetUpdater.Core;
using LevelOffsetUpdater.Services;
using System;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace LevelOffsetUpdater.UI
{
    // Форма настроек приложения
    public partial class SettingsForm : System.Windows.Forms.Form
    {
        #region Fields
        private readonly IAutoUpdateService _autoUpdateService;
        private readonly IManualUpdateService _manualUpdateService;
        private Core.Settings _settings;
        private Document _doc;
        #endregion

        #region Constructor
        public SettingsForm(IAutoUpdateService autoUpdateService, IManualUpdateService manualUpdateService, Document doc)
        {
            _autoUpdateService = autoUpdateService ?? throw new ArgumentNullException(nameof(autoUpdateService));
            _manualUpdateService = manualUpdateService ?? throw new ArgumentNullException(nameof(manualUpdateService));
            _doc = doc;

            InitializeComponent();
            LoadSettings();
        }
        #endregion

        #region Private Methods
        // Загружает настройки в элементы формы
        private void LoadSettings()
        {
            // отключаем обработчики
            chkAutoUpdate.CheckedChanged -= chkAutoUpdate_CheckedChanged;
            chkAutoUpdateWallDistance.CheckedChanged -= chkAutoUpdateWallDistance_CheckedChanged;
            numRoundingStep.ValueChanged -= numRoundingStep_ValueChanged;
            numWarningThreshold.ValueChanged -= numWarningThreshold_ValueChanged;

            _settings = Core.Settings.Load();

            chkAutoUpdate.Checked = _settings.AutoUpdateEnabled;
            chkAutoUpdateWallDistance.Checked = _settings.AutoUpdateWallDistanceEnabled;
            numRoundingStep.Value = _settings.RoundingStepMm;
            numWarningThreshold.Value = _settings.WarningThreshold;

            // возвращаем обработчики
            chkAutoUpdate.CheckedChanged += chkAutoUpdate_CheckedChanged;
            chkAutoUpdateWallDistance.CheckedChanged += chkAutoUpdateWallDistance_CheckedChanged;
            numRoundingStep.ValueChanged += numRoundingStep_ValueChanged;
            numWarningThreshold.ValueChanged += numWarningThreshold_ValueChanged;
        }


        // Сохраняет настройки из элементов формы
        private void SaveSettings()
        {
            _settings.AutoUpdateEnabled = chkAutoUpdate.Checked;
            _settings.AutoUpdateWallDistanceEnabled = chkAutoUpdateWallDistance.Checked;
            _settings.RoundingStepMm = (int)numRoundingStep.Value;
            _settings.WarningThreshold = (int)numWarningThreshold.Value;
            _settings.Save();

            _autoUpdateService.IsEnabled = _settings.AutoUpdateEnabled;
        }

        private bool ValidateParameters(Document document)
        {
            var errors = new List<string>();
            bool offsetParamValid = true;
            bool wallDistanceParamValid = true;

            // Проверяем параметр отметки расположения только если включено автообновление
            if (chkAutoUpdate.Checked)
            {
                var offsetValidation = ParameterValidator.ValidateParameter(document, Constants.TARGET_PARAMETER_NAME);
                if (!offsetValidation.IsValid)
                {
                    errors.Add($"Параметр '{Constants.TARGET_PARAMETER_NAME}':\n{offsetValidation.ErrorMessage}");
                    offsetParamValid = false;
                }
            }

            // Проверяем параметр расстояния до низа стены только если включено автообновление
            if (chkAutoUpdateWallDistance.Checked)
            {
                var wallDistanceValidation = ParameterValidator.ValidateParameter(document, Constants.WALL_DISTANCE_PARAMETER_NAME);
                if (!wallDistanceValidation.IsValid)
                {
                    errors.Add($"Параметр '{Constants.WALL_DISTANCE_PARAMETER_NAME}':\n{wallDistanceValidation.ErrorMessage}");
                    wallDistanceParamValid = false;
                }
            }

            if (errors.Any())
            {
                // Отключаем автообновление только для параметров, которые не прошли валидацию
                if (!offsetParamValid)
                {
                    _settings.AutoUpdateEnabled = false;
                    chkAutoUpdate.Checked = false;
                }

                if (!wallDistanceParamValid)
                {
                    _settings.AutoUpdateWallDistanceEnabled = false;
                    chkAutoUpdateWallDistance.Checked = false;
                }

                _settings.Save();

                MessageBox.Show(
                    "Ошибка обновления\n\n" + string.Join("\n\n", errors) + "\n\nАвтоматическое обновление соответствующих параметров выключено.",
                    "Ошибка обновления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            return true;
        }
        #endregion

        #region Event Handlers
        private void chkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void chkAutoUpdateWallDistance_CheckedChanged(object sender, EventArgs e)
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
            if (!ValidateParameters(_doc))
            {
                return;
            }

            _manualUpdateService.Raise();
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (!ValidateParameters(_doc))
            {
                return;
            }

            Close();
        }
        #endregion

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.chkAutoUpdateWallDistance = new System.Windows.Forms.CheckBox();
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
            this.chkAutoUpdate.Location = new System.Drawing.Point(27, 25);
            this.chkAutoUpdate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(385, 82);
            this.chkAutoUpdate.TabIndex = 0;
            this.chkAutoUpdate.Text =  "Автоматически обновлять параметр \nADSK_Размер_Отметка расположения\n" +
                "- отметка расположения\n(настройка сохраняется при перезагрузке Revit)";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            this.chkAutoUpdate.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // chkAutoUpdateWallDistance
            // 
            this.chkAutoUpdateWallDistance.Location = new System.Drawing.Point(27, 125);
            this.chkAutoUpdateWallDistance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkAutoUpdateWallDistance.Name = "chkAutoUpdateWallDistance";
            this.chkAutoUpdateWallDistance.Size = new System.Drawing.Size(385, 86);
            this.chkAutoUpdateWallDistance.TabIndex = 8;
            this.chkAutoUpdateWallDistance.Text = "Автоматически обновлять  \nKRGP_Отверстие_Расстояние до низа стены\n" +
                "- расстояние до низа стены\n(настройка сохраняется при перезагрузке Revit)";
            this.chkAutoUpdateWallDistance.UseVisualStyleBackColor = true;
            this.chkAutoUpdateWallDistance.CheckedChanged += new System.EventHandler(this.chkAutoUpdateWallDistance_CheckedChanged);
            // 
            // lblRoundingStep
            // 
            this.lblRoundingStep.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRoundingStep.AutoSize = true;
            this.lblRoundingStep.Location = new System.Drawing.Point(34, 221);
            this.lblRoundingStep.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRoundingStep.Name = "lblRoundingStep";
            this.lblRoundingStep.Size = new System.Drawing.Size(143, 16);
            this.lblRoundingStep.TabIndex = 1;
            this.lblRoundingStep.Text = "Шаг округления (мм):";
            // 
            // numRoundingStep
            // 
            this.numRoundingStep.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numRoundingStep.Location = new System.Drawing.Point(274, 219);
            this.numRoundingStep.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.numRoundingStep.Size = new System.Drawing.Size(107, 22);
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
            this.lblWarningThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWarningThreshold.Location = new System.Drawing.Point(34, 244);
            this.lblWarningThreshold.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWarningThreshold.Name = "lblWarningThreshold";
            this.lblWarningThreshold.Size = new System.Drawing.Size(232, 46);
            this.lblWarningThreshold.TabIndex = 4;
            this.lblWarningThreshold.Text = "Предупреждать при количестве элементов больше:";
            this.lblWarningThreshold.Click += new System.EventHandler(this.lblWarningThreshold_Click);
            // 
            // numWarningThreshold
            // 
            this.numWarningThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numWarningThreshold.Location = new System.Drawing.Point(274, 262);
            this.numWarningThreshold.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.numWarningThreshold.Size = new System.Drawing.Size(107, 22);
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
            this.btnUpdateAll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateAll.Location = new System.Drawing.Point(51, 339);
            this.btnUpdateAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnUpdateAll.Name = "btnUpdateAll";
            this.btnUpdateAll.Size = new System.Drawing.Size(200, 37);
            this.btnUpdateAll.TabIndex = 6;
            this.btnUpdateAll.Text = "Обновить у всех сейчас";
            this.btnUpdateAll.UseVisualStyleBackColor = true;
            this.btnUpdateAll.Click += new System.EventHandler(this.btnUpdateAll_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(259, 339);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 37);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblRoundingStepUnit
            // 
            this.lblRoundingStepUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRoundingStepUnit.AutoSize = true;
            this.lblRoundingStepUnit.Location = new System.Drawing.Point(387, 221);
            this.lblRoundingStepUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRoundingStepUnit.Name = "lblRoundingStepUnit";
            this.lblRoundingStepUnit.Size = new System.Drawing.Size(25, 16);
            this.lblRoundingStepUnit.TabIndex = 3;
            this.lblRoundingStepUnit.Text = "мм";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 389);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnUpdateAll);
            this.Controls.Add(this.numWarningThreshold);
            this.Controls.Add(this.lblWarningThreshold);
            this.Controls.Add(this.lblRoundingStepUnit);
            this.Controls.Add(this.numRoundingStep);
            this.Controls.Add(this.lblRoundingStep);
            this.Controls.Add(this.chkAutoUpdateWallDistance);
            this.Controls.Add(this.chkAutoUpdate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
        private CheckBox chkAutoUpdateWallDistance;
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

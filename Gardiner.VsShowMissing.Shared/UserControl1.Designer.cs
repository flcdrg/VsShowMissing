namespace Gardiner.VsShowMissing
{
    partial class UserControl1
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.chkFailBuildOnError = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.lstRunWhen = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lstMessageLevel = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // chkFailBuildOnError
            // 
            this.chkFailBuildOnError.AutoSize = true;
            this.chkFailBuildOnError.Location = new System.Drawing.Point(61, 68);
            this.chkFailBuildOnError.Name = "chkFailBuildOnError";
            this.chkFailBuildOnError.Size = new System.Drawing.Size(520, 36);
            this.chkFailBuildOnError.TabIndex = 0;
            this.chkFailBuildOnError.Text = "Fail entire build if any errors detected";
            this.toolTip1.SetToolTip(this.chkFailBuildOnError, "Cancel the build if any missing files are found. This only has effect if When is " +
        "set to BeforeBuild");
            this.chkFailBuildOnError.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(412, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "When to check for missing files:";
            // 
            // lstRunWhen
            // 
            this.lstRunWhen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstRunWhen.FormattingEnabled = true;
            this.lstRunWhen.Location = new System.Drawing.Point(67, 223);
            this.lstRunWhen.Name = "lstRunWhen";
            this.lstRunWhen.Size = new System.Drawing.Size(420, 39);
            this.lstRunWhen.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(67, 309);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(286, 32);
            this.label2.TabIndex = 3;
            this.label2.Text = "Message importance:";
            // 
            // lstMessageLevel
            // 
            this.lstMessageLevel.FormattingEnabled = true;
            this.lstMessageLevel.Location = new System.Drawing.Point(67, 363);
            this.lstMessageLevel.Name = "lstMessageLevel";
            this.lstMessageLevel.Size = new System.Drawing.Size(420, 39);
            this.lstMessageLevel.TabIndex = 4;
            // 
            // UserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstMessageLevel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstRunWhen);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkFailBuildOnError);
            this.Name = "UserControl1";
            this.Size = new System.Drawing.Size(1066, 604);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkFailBuildOnError;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox lstRunWhen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox lstMessageLevel;
    }
}

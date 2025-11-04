namespace GUITest
{
    partial class MinaExportForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            extractMinaPakFilesButton = new Button();
            label1 = new Label();
            extractMinaANBFilesButton = new Button();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            exportMinaPakFile = new Button();
            textBox1 = new TextBox();
            pakBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            progressBar = new ProgressBar();
            SuspendLayout();
            // 
            // extractMinaPakFilesButton
            // 
            extractMinaPakFilesButton.Location = new Point(12, 12);
            extractMinaPakFilesButton.Name = "extractMinaPakFilesButton";
            extractMinaPakFilesButton.Size = new Size(219, 23);
            extractMinaPakFilesButton.TabIndex = 0;
            extractMinaPakFilesButton.Text = "Extract Mina pak Files";
            extractMinaPakFilesButton.UseVisualStyleBackColor = true;
            extractMinaPakFilesButton.Click += extractMinaPakFilesButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(237, 16);
            label1.Name = "label1";
            label1.Size = new Size(153, 15);
            label1.TabIndex = 1;
            label1.Text = "<- select mina \"data\" folder";
            // 
            // extractMinaANBFilesButton
            // 
            extractMinaANBFilesButton.Location = new Point(12, 41);
            extractMinaANBFilesButton.Name = "extractMinaANBFilesButton";
            extractMinaANBFilesButton.Size = new Size(219, 23);
            extractMinaANBFilesButton.TabIndex = 2;
            extractMinaANBFilesButton.Text = "Extract Mina ANB Files";
            extractMinaANBFilesButton.UseVisualStyleBackColor = true;
            extractMinaANBFilesButton.Click += extractMinaANBFilesButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(237, 45);
            label2.Name = "label2";
            label2.Size = new Size(191, 15);
            label2.TabIndex = 3;
            label2.Text = "<- use this after extracting pak files";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 67);
            label3.Name = "label3";
            label3.Size = new Size(169, 15);
            label3.TabIndex = 4;
            label3.Text = "formats reversed by Muonesce";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 82);
            label4.Name = "label4";
            label4.Size = new Size(151, 15);
            label4.TabIndex = 5;
            label4.Text = "programmed by Muonesce";
            // 
            // exportMinaPakFile
            // 
            exportMinaPakFile.Location = new Point(237, 82);
            exportMinaPakFile.Name = "exportMinaPakFile";
            exportMinaPakFile.Size = new Size(219, 23);
            exportMinaPakFile.TabIndex = 6;
            exportMinaPakFile.Text = "Extract Mina pak File";
            exportMinaPakFile.UseVisualStyleBackColor = true;
            exportMinaPakFile.Click += exportMinaPakFile_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 141);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(385, 131);
            textBox1.TabIndex = 7;
            // 
            // pakBackgroundWorker
            // 
            pakBackgroundWorker.WorkerReportsProgress = true;
            pakBackgroundWorker.WorkerSupportsCancellation = true;
            pakBackgroundWorker.DoWork += pakBackgroundWorker_DoWork;
            pakBackgroundWorker.ProgressChanged += pakBackgroundWorker_ProgressChanged;
            pakBackgroundWorker.RunWorkerCompleted += pakBackgroundWorker_RunWorkerCompleted;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 278);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(385, 23);
            progressBar.TabIndex = 8;
            // 
            // MinaExportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(667, 313);
            Controls.Add(progressBar);
            Controls.Add(textBox1);
            Controls.Add(exportMinaPakFile);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(extractMinaANBFilesButton);
            Controls.Add(label1);
            Controls.Add(extractMinaPakFilesButton);
            Name = "MinaExportForm";
            Text = "Mina Export Testing";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button extractMinaPakFilesButton;
        private Label label1;
        private Button extractMinaANBFilesButton;
        private Label label2;
        private Label label3;
        private Label label4;
        private Button exportMinaPakFile;
        private TextBox textBox1;
        private System.ComponentModel.BackgroundWorker pakBackgroundWorker;
        private ProgressBar progressBar;
    }
}

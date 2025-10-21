namespace WinFormsApp1
{
    partial class Form1
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
            listBox1 = new ListBox();
            panel1 = new Panel();
            textBoxProcessName = new TextBox();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            listBox2 = new ListBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.HorizontalScrollbar = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 346);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(512, 64);
            listBox1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(textBoxProcessName);
            panel1.Controls.Add(button4);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 410);
            panel1.Name = "panel1";
            panel1.Size = new Size(536, 40);
            panel1.TabIndex = 1;
            // 
            // textBoxProcessName
            // 
            textBoxProcessName.Location = new Point(336, 6);
            textBoxProcessName.Name = "textBoxProcessName";
            textBoxProcessName.PlaceholderText = "Process name";
            textBoxProcessName.Size = new Size(188, 23);
            textBoxProcessName.TabIndex = 4;
            // 
            // button4
            // 
            button4.Location = new Point(229, 6);
            button4.Name = "button4";
            button4.Size = new Size(87, 23);
            button4.TabIndex = 3;
            button4.Text = "Start Process";
            button4.UseVisualStyleBackColor = true;
            button4.Click += StartNewProcess_Click;
            // 
            // button3
            // 
            button3.Location = new Point(12, 92);
            button3.Name = "button3";
            button3.Size = new Size(144, 23);
            button3.TabIndex = 2;
            button3.Text = "Kill Process";
            button3.UseVisualStyleBackColor = true;
            button3.Click += KillProcess_Click;
            // 
            // button2
            // 
            button2.Location = new Point(12, 52);
            button2.Name = "button2";
            button2.Size = new Size(144, 23);
            button2.TabIndex = 1;
            button2.Text = "Refresh Processes";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Refresh_Click;
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(144, 23);
            button1.TabIndex = 0;
            button1.Text = "Turn on Server";
            button1.UseVisualStyleBackColor = true;
            button1.Click += StartProgram_Click;
            // 
            // listBox2
            // 
            listBox2.FormattingEnabled = true;
            listBox2.ItemHeight = 15;
            listBox2.Location = new Point(162, 6);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(362, 334);
            listBox2.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(536, 450);
            Controls.Add(listBox2);
            Controls.Add(panel1);
            Controls.Add(button3);
            Controls.Add(listBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox1;
        private Panel panel1;
        private Button button2;
        private Button button1;
        private Button button3;
        private Button button4;
        private TextBox textBoxProcessName;
        private ListBox listBox2;
    }
}

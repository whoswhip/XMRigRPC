namespace SupportXMR_RPC
{
    public partial class Form1 : Form
    {
        private void InitializeComponent()
        {
            label1 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(44, 9);
            label1.Name = "label1";
            label1.Size = new Size(183, 15);
            label1.TabIndex = 0;
            label1.Text = "[WIP] This will end up having a UI";
            label1.Click += label1_Click;
            // 
            // Form1
            // 
            ClientSize = new Size(284, 261);
            Controls.Add(label1);
            Name = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        private Label label1;

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

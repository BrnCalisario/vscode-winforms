using System.IO;
using System.Linq;

namespace Aula02
{
    public partial class Form1 : Form
    {

        private string CurrentDirectoryPath { get; set; } = string.Empty;
        private string CurrentFileName { get; set; } = string.Empty;
        private RichTextBox? CurrentFileRTB { get; set; }

        private string FilePath
        {
            get => CurrentDirectoryPath + "/" + CurrentFileName;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //this.openNewFolder();
            tabControl1.TabPages.Clear();
        }

        private void openNewFolder()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.CurrentDirectoryPath = folderBrowserDialog1.SelectedPath;

                this.clearTreeView();
                this.CurrentFileRTB = null;

                var files = Directory.GetFiles(CurrentDirectoryPath);

                SelectedDirectory.Text = "Diretório: " + Path.GetFileName(CurrentDirectoryPath);

                foreach (var file in files)
                {
                    string name = Path.GetFileName(file);
                    treeView1.BeginUpdate();
                    treeView1.Nodes.Add(name);
                    treeView1.EndUpdate();
                }
            }
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openNewFolder();
        }

        private void clearTreeView()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.EndUpdate();

            SelectedDirectory.Text = "Nenhum diretório selecionado";
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            foreach (TabPage tb in tabControl1.TabPages)
            {
                if (tb.Text == e.Node.Text)
                {
                    tabControl1.SelectedTab = tb;
                    return;
                }
            }

            this.CurrentFileName = e.Node.Text;

            var tabPage = new TabPage();
            tabPage.Text = this.CurrentFileName;

            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.Text = File.ReadAllText(this.FilePath);

            StatusStrip stp = new StatusStrip();

            var closeBtn = new ToolStripStatusLabel();
            closeBtn.Text = "Fechar Arquivo";

            closeBtn.Click += (o, e) =>
            {
                tabControl1.TabPages.Remove(tabPage);
            };

            stp.Items.Add(closeBtn);

            tabPage.Controls.Add(stp);
            tabPage.Controls.Add(rtb);

            tabControl1.TabPages.Add(tabPage);
            tabControl1.SelectedTab = tabPage;

            this.CurrentFileRTB = rtb;
        }

        private void salvarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentDirectoryPath == string.Empty)
            {
                MessageBox.Show(
                    "Escolha um diretório para trabalhar",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                return;
            }

            if (CurrentFileName == string.Empty)
            {
                MessageBox.Show(
                    "Escolha um arquivo para trabalhar",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                return;
            }

            if (this.CurrentFileRTB is null)
                return;

            File.WriteAllText(this.FilePath, this.CurrentFileRTB.Text);
            MessageBox.Show("Salvou");
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            var selectedTab = tabControl1.SelectedTab;

            if (selectedTab is null)
                return;

            var controls = tabControl1.SelectedTab.Controls;
            foreach (var c in controls)
            {
                if (c is not RichTextBox rtb)
                    continue;
                else
                {
                    this.CurrentFileRTB = rtb;
                    return;
                }
            }
        }
    }
}
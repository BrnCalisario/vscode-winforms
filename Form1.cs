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
            tabControl1.TabPages.Clear();
        }

        private void OpenNewFolder(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.CurrentFileRTB = null;

                this.CurrentDirectoryPath = folderBrowserDialog1.SelectedPath;

                this.RefreshTree();
            }
        }

        private void RefreshTree()
        {
            this.ClearTreeView();

            SelectedDirectory.Text = "Diretório: " + Path.GetFileName(CurrentDirectoryPath);
            
            var dirInfo = new DirectoryInfo(CurrentDirectoryPath);
            
            treeView1.Nodes.Add(CreateDirectoryNode(dirInfo));
        }

        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var dirNode = new TreeNode(directoryInfo.Name);
           
            foreach(var dir in directoryInfo.GetDirectories())
                dirNode.Nodes.Add(CreateDirectoryNode(dir));
            foreach (var file in directoryInfo.GetFiles())
                dirNode.Nodes.Add(new TreeNode(file.Name));

            return dirNode;
        }

        private void ClearTreeView()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.EndUpdate();

            SelectedDirectory.Text = "Nenhum diretório selecionado";
        }

        private void OnTreeNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var dirPath = Directory.GetParent(this.CurrentDirectoryPath).FullName + "\\" + e.Node.Text;

            if(dirPath.Equals(this.CurrentDirectoryPath))
                return;
     
            var itemAttr = File.GetAttributes(this.CurrentDirectoryPath + "/" + e.Node.Text);

            if (itemAttr.HasFlag(FileAttributes.Directory))
                return;
               


            foreach (TabPage tb in tabControl1.TabPages)
            {
                if (tb.Text == e.Node.Text)
                {
                    tabControl1.SelectedTab = tb;
                    return;
                }
            }


            this.CurrentFileName = e.Node.Text;

            this.CreateTab(this.CurrentFileName);

        }

        private void SaveFileButton(object sender, EventArgs e)
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
            MessageBox.Show("Salvou " + this.CurrentFileName);
        }

        private void OnTabChange(object sender, TabControlCancelEventArgs e)
        {

            var selectedTab = tabControl1.SelectedTab;

            if (selectedTab is null)
                return;

            var controls = tabControl1.SelectedTab.Controls;
            foreach (var c in controls)
            {
                if (c is not RichTextBox rtb)
                    continue;

                this.CurrentFileRTB = rtb;
                this.CurrentFileName = selectedTab.Text;
                return;
            }
        }

        private void NewFileButton(object sender, EventArgs e)
        {
            if (this.CurrentDirectoryPath == string.Empty)
                return;

            int count = Directory.GetFiles(this.CurrentDirectoryPath).Count(t => t.Contains("New File"));
            string fileName = "New File" + (count + 1) + ".txt";

            File.Create(this.CurrentDirectoryPath + "/" + fileName).Close();
            this.RefreshTree();

            this.CurrentFileName = fileName;
            this.CreateTab(fileName);

            MessageBox.Show(fileName);
        }

        private void CreateTab(string tabName)
        {
            TabPage tabPage = new()
            {
                Text = tabName
            };

            RichTextBox rtb = new()
            {
                Dock = DockStyle.Fill,
                Text = File.ReadAllText(this.FilePath)
            };

            var closeBtn = new ToolStripStatusLabel
            {
                Text = "Fechar Arquivo"
            };

            closeBtn.Click += (o, e) =>
            {
                tabControl1.TabPages.Remove(tabPage);
            };

            StatusStrip stp = new();
            stp.Items.Add(closeBtn);

            tabPage.Controls.Add(stp);
            tabPage.Controls.Add(rtb);

            tabControl1.TabPages.Add(tabPage);
            tabControl1.SelectedTab = tabPage;

            this.CurrentFileRTB = rtb;
            this.CurrentFileName = tabName;
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                treeView1.LabelEdit = true;
                var selected = treeView1.SelectedNode;

                if (!selected.IsEditing)               
                    selected.BeginEdit();
                
            }

            if(e.KeyCode == Keys.Delete)
            {
                var selected = treeView1.SelectedNode;

                if (selected is null)
                    return;

                var path = this.CurrentDirectoryPath + "/" +selected.Text;

                var warning = MessageBox.Show(
                    "Tem certeza que deseja apagar este arquivo",
                    "Aviso", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning
                    );
                
                if (warning != DialogResult.OK)
                    return;         
                
                foreach(TabPage tb in tabControl1.TabPages)
                {
                    if(tb.Text == selected.Text)
                    {
                        tabControl1.TabPages.Remove(tb);
                        break;
                    }
                }


                File.Delete(path);
                this.RefreshTree();
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label is null)
            {
                return;
            }


            var oldName = this.CurrentDirectoryPath + "/" + e.Node.Text;
            var newName = this.CurrentDirectoryPath + "/" + e.Label;


  
            if (oldName.Equals(newName))
                return;
            try
            {
                File.Move(oldName, newName);
            }
            catch
            {
                return;
            }

            if (this.CurrentFileName == e.Node.Text)
                this.CurrentFileName = e.Label;


            foreach (TabPage tp in tabControl1.TabPages)
            {
                if (tp.Text == e.Node.Text)
                {
                    tp.Text = e.Label;
                    break;
                }
            }

            this.RefreshTree();
            MessageBox.Show("Renomeado");
        }
    }
}